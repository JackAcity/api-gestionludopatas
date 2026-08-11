## Context

La especificación fuente exige que `crearCorte` reproduzca la respuesta para la misma
clave y huella durante 24 horas, y que una clave reutilizada con otro payload produzca
`409 GL-IDEMP-001`. También identifica expresamente el hueco de caída después de
`SP_CORTE_Crear` y antes de guardar la respuesta.

El preflight de DEV produjo esta evidencia:

```text
IDEMPOTENCY_PREFLIGHT|create_table=0|execute_sp=1|view_sp=0|view_db=0
```

No se aplicó DDL ni se expusieron secretos. La inspección DBA posterior confirmó que
`SP_CORTE_Crear` no inicia transacción propia: realiza un único `INSERT` y retorna
`OUTPUT INSERTED.id` como result set. `dbo.Corte` no tiene triggers, no usa memoria
optimizada y sus columnas obligatorias no recibidas por el SP tienen defaults locales.
El login efectivo de la API no pertenece a roles de lectura, escritura ni owner. Esto
confirma que el wrapper debe capturar el result set con `INSERT ... EXEC` y que puede
incluir el insert existente en su transacción exterior sin efectos secundarios conocidos.

## Goals / Non-Goals

**Goals:**

- Garantizar exactamente una creación de corte por `Idempotency-Key`/huella dentro de
  la vigencia configurada, incluso con varias instancias o tras reiniciar una instancia.
- Hacer atómica la creación del corte y el registro idempotente dentro de una misma
  instancia SQL Server.
- Preservar D5: ningún permiso DML directo para la API sobre `bd_autobot`.
- Mantener el endpoint fino y el núcleo independiente de `SqlConnection`.

**Non-Goals:**

- No cambiar el contrato HTTP ni agregar una transacción distribuida entre SQL y HTTP.
  La respuesta HTTP se reconstruye desde el `corteId` que quedó comprometido.
- No modificar `dbo.SP_CORTE_Crear` ni crear tablas de aplicación en `bd_autobot`.
- No ejecutar la migración ni crear una base de datos desde este repositorio.
- No resolver idempotencia de los seis endpoints de pendientes: son solo lectura.

## Decision record

```text
DECISION IDEMP-PERSIST-001
CONTEXT: framework=.NET 10; contract=corte-crear + tests; layer=Api/Application/Infrastructure;
io=sql; error_kind=mixed; atomicity=local (misma instancia SQL, pendiente DBA);
profile=pragmatic.
RULES: IDEMP-001, TX-001, REPO-001, TAP-001, TEST-001.
CHOOSE: un procedimiento semántico en una base propia de API, llamado en una conexión
SQL y transacción únicas; índice único y bloqueo de rango por Idempotency-Key.
REJECT: tabla externa + endpoint que primero consulta y luego llama SP, porque conserva
la carrera y no cubre la caída entre ambos commits.
VERIFY: prueba concurrente, replay tras reinicio, conflicto, rollback inyectado y
verificación DBA de la topología/semántica transaccional del SP.
OPEN: nombre/propietario de la base, AG/mirroring, comportamiento interno de
SP_CORTE_Crear, retención operativa y timeout aprobado.
```

### D1 — Base propia de la API, misma instancia SQL Server

La tabla `api.IdempotenciaCrearCorte` vive en una base que el DBA asigna a esta API;
no en `bd_autobot`. El procedimiento de esa base llama calificadamente a
`bd_autobot.dbo.SP_CORTE_Crear` dentro de **la misma sesión y transacción**. SQL Server
coordina internamente el commit cuando una transacción local de una instancia abarca dos
bases; esto no es válido si la topología o características de esas bases lo impiden, por
lo que DBA debe certificarlo antes de aplicar. La tabla propuesta es convencional, no
memory-optimized.

La API recibe `EXECUTE` sobre el nuevo procedimiento, no `SELECT`/`INSERT`/`UPDATE`
sobre la tabla. Mantiene el `EXECUTE` existente sobre `dbo.SP_CORTE_Crear`, necesario
para la llamada cross-database del procedimiento bajo el mismo login. No se habilita
cross-database ownership chaining ni se agregan permisos a `dbo.Corte`.

### D2 — Una fila aparece solo junto con el corte

El procedimiento adquiere un `UPDLOCK, HOLDLOCK` sobre la clave única. Si no existe,
ejecuta el SP, captura su único result set `OUTPUT INSERTED.id`, inserta `corteId`,
`status=201` y expiración, y confirma una sola vez.
Por tanto no hay estado persistente "en curso": otra sesión espera el lock y luego ve
la fila final. Si la primera sesión cae, SQL Server revierte ambas escrituras; si el
commit ocurrió, ambas quedan visibles. Esta es la propiedad que el almacén en memoria
no podía ofrecer.

La tabla guarda `corteId` en vez de JSON HTTP. Para este endpoint, la respuesta
`CrearCorteResponse` se reconstruye determinísticamente a partir de ese ID y del status
almacenado; así el modelo SQL no se acopla al serializador HTTP. Si el contrato deja de
ser solo `corteId`, se debe versionar el procedimiento y persistir la representación
necesaria antes de cambiar el endpoint.

### D3 — Resultado esperado como fila, no excepción SQL

La misma clave con huella diferente es un resultado esperado de negocio. El procedimiento
devuelve `outcome = 'conflict'`; el adaptador lo traduce al resultado tipado existente y
el borde HTTP conserva `409 GL-IDEMP-001`. Fallas SQL reales, cancelación y timeouts no
se convierten en `Result` funcional: suben al manejador global.

### D4 — Adaptación posterior de código

Después del smoke test DBA, se reemplazará `IIdempotencyStore` en memoria por un puerto
semántico específico de caso de uso, por ejemplo `ICreadorCorteIdempotente`. Su método
asíncrono recibe el request validado, clave y huella; entrega `CrearCorteResponse` más
la marca `Replayed`. `Application` no verá SQL ni HTTP; el endpoint solo validará el
header, invocará el manejador y agregará `Idempotency-Replayed` cuando corresponda.

No se implementa un repositorio CRUD genérico: la operación es `CrearCorteIdempotente`
y encapsula el protocolo transaccional completo.

## Risks / Trade-offs

- [El SP actual abre/confirmar/revierte transacciones propias] → **Descartado en DEV**:
  la definición revisada no contiene una transacción propia. El smoke test del wrapper
  sigue siendo obligatorio antes de cambiar la API.
- [Bases en AG/mirroring o distintas instancias] → DBA debe confirmar la topología. No
  se habilita MSDTC como atajo. Si no pueden participar en el mismo commit, no puede
  prometerse recuperación exacta; debe rediseñarse el SP con la clave idempotente.
- [Segundo request espera el lock y supera timeout] → el cliente recibe falla técnica
  reintentable con la misma clave, nunca una segunda creación. El timeout final debe
  aprobarlo API owner/DBA.
- [La tabla crece] → el script incluye un purge por lotes para un job DBA; no concede
  ese procedimiento al login de la API.

## Migration / rollback

1. DBA provisiona una base propia, normal (no memory-optimized), en la misma instancia.
2. DBA revisa y ejecuta `001_instalar_idempotencia_corte.sql` en DEV, con variables
   explícitas y un respaldo/ventana conforme a su proceso.
3. DBA ejecuta los casos de verificación de `VALIDACION_DBA.md`. Si alguno falla, no se
   despliega código de API y se elimina solo el objeto nuevo según el rollback del mismo
   documento.
4. Recién entonces se implementa y despliega el adaptador .NET; se ejecutan pruebas de
   concurrencia/reinicio contra DEV.

El pase autocontenido para DBA, arquitectura y QA está en
`api-gestionludopatas/database/idempotencia-persistente/PASE_DBA_Y_ARQUITECTURA.md`.

## Open Questions

1. Nombre y propietario operativo de la base propia de la API.
2. Confirmación de que ambas bases comparten instancia y no tienen una configuración AG/
   mirroring incompatible.
3. Semántica transaccional interna de `dbo.SP_CORTE_Crear`, no visible al login actual.
4. Valor aprobado de timeout y mecanismo DBA de purga/retención.
