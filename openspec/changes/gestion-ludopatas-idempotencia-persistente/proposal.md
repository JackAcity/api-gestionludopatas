## Why

`POST /api/v1/cortes` ya exige `Idempotency-Key` y la implementación actual elimina la
carrera entre solicitudes simultáneas **dentro de una instancia**. Sin embargo, su
almacén es un `ConcurrentDictionary`: un reinicio pierde el registro y dos instancias
no se coordinan. Peor aún, una caída después del `COMMIT` de `dbo.SP_CORTE_Crear` y
antes de publicar la respuesta deja al reintento sin una forma segura de saber si el
corte fue creado.

El preflight de DEV del 2026-08-11 confirmó que el login de la API tiene `EXECUTE` en
`dbo.SP_CORTE_Crear`, pero no `CREATE TABLE` ni `VIEW DEFINITION`. Esa es la política
D5 de mínimo privilegio, por lo que la aplicación no debe autoaprovisionar ni consultar
tablas de `bd_autobot`.

## What Changes

- Se entrega a DBA un script **no ejecutado por la API** que crea, en una base propia
  de la API y en la misma instancia SQL Server, una tabla y el procedimiento
  `api.SP_Corte_Crear_Idempotente`.
- El procedimiento mantiene bloqueada una sola clave, ejecuta el SP de negocio y
  persiste el `corteId`/status antes del único `COMMIT`. Una repetición con la misma
  huella devuelve el mismo resultado; una huella distinta devuelve `conflict`.
- El script no otorga DML directo a la API ni modifica `bd_autobot`, `dbo.Corte` ni
  `dbo.SP_CORTE_Crear`. El login conserva únicamente `EXECUTE` sobre procedimientos.
- Se define el posterior cambio de código: un puerto semántico de creación idempotente
  reemplazará la coordinación en memoria después de que DBA certifique el procedimiento
  en DEV. No se cambia código de producción antes de esa certificación porque quedaría
  inoperable contra el despliegue actual.

## Capabilities

### New Capabilities

- `idempotencia-corte-persistente`: creación de corte durable, compartida entre
  instancias y recuperable tras una caída entre la ejecución del SP y la respuesta HTTP.

### Modified Capabilities

- `corte-crear`: su semántica HTTP no cambia (`201`, replay y `409 GL-IDEMP-001`), pero
  el origen de la garantía deja de ser local al proceso y pasa a ser una transacción SQL.

## Impact

- Nuevo paquete DBA: `api-gestionludopatas/database/idempotencia-persistente/`.
- Nuevo change OpenSpec con las decisiones, riesgos y pruebas necesarias.
- Requiere confirmación DBA de: misma instancia SQL, ausencia de una restricción de
  Availability Group/mirroring que impida transacciones entre bases, y compatibilidad
  transaccional del SP existente. Ninguno de esos hechos se infiere del login limitado.
- Requerirá un despliegue posterior de API y una colección de pruebas de concurrencia;
  este change no marca la garantía como terminada hasta ejecutar esas verificaciones.
