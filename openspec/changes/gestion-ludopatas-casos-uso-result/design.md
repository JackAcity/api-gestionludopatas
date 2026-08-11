## Context

**Depende de**: `gestion-ludopatas-limpieza-estructura` (mueve `Endpoints/`,
`Middleware/`, `Security/` bajo `Api/`) — se aplica antes que este change; las rutas
de este documento y de `tasks.md` ya asumen esa estructura (`Api/Endpoints/...`,
`Api/Middleware/...`).

`GestionLudopatas.Api` (ver `openspec/changes/gestion-ludopatas-api/design.md` para el
contexto completo del servicio, D1-D9) expone 8 operaciones sobre `bd_autobot`. Hoy,
cada operación valida sus reglas de negocio (`tipoCorte` válido, `corteIdActual>0`,
`timeoutMinutos>=0`, `esReintentoForzado` no nulo si fue enviado, etc.) dentro del
adaptador SQL que la implementa (`Infrastructure/Sql/*`), lanzando
`ErrorFuncionalException` cuando la regla falla. Esa excepción es capturada por
`ManejadorExcepcionesGlobal` (un `IExceptionHandler` de framework) y traducida a
`ProblemaDetalle`. La rúbrica del proyecto (`dotnet-clean-style` §7) nombra
explícitamente este patrón — "catch genérico alrededor de un caso de uso completo para
convertir una excepción de negocio esperada en una respuesta HTTP" — como la señal de
que se está usando `Exception` donde correspondía `Result<T>`. Además, al no existir
una clase de "caso de uso" en `Application`, la validación quedó atrapada en el
adaptador de infraestructura (mezcla SRP: "hablar SQL Server" + "validar regla").

## Goals / Non-Goals

**Goals:**
- Fallos de negocio esperados se representan y propagan como `Result<T>`, nunca como
  excepción lanzada desde el caso de uso.
- La validación de cada regla de negocio vive en una clase de caso de uso en
  `Application`, no en el adaptador SQL. El adaptador SQL queda reducido a "ejecutar el
  SP y mapear filas" — intercambiable sin arrastrar la validación (D4 del change
  original: puertos por caso de uso, adaptador reemplazable).
- El contrato HTTP observable (código `GL-*`, status, forma de `ProblemaDetalle`) es
  idéntico antes/después — este es un cambio de mecanismo interno, verificable
  comparando la respuesta de cada regla antes y después del refactor.

**Non-Goals:**
- No toca `ApiKeyAuthenticationMiddleware` ni `IpAllowlistMiddleware`. Esos son
  middlewares transversales de autenticación/autorización que corren ANTES de llegar a
  un endpoint/caso de uso — no son el "caso de uso completo" que la rúbrica señala, y
  el patrón throw-corto-circuito-capturado-por-`IExceptionHandler` es idiomático de
  ASP.NET Core para ese tipo de gate transversal. Quedan fuera de alcance de este
  change; si se quiere revisar también, es un hallazgo aparte.
- No cambia `ErrorMapeoSql` ni el mapeo `SqlException` → `ProblemaDetalle` para errores
  verdaderamente de infraestructura (timeout, deadlock, conflicto de datos, permisos,
  no disponible) — esos siguen siendo excepciones reales lanzadas por
  `Microsoft.Data.SqlClient`, el caso "verdaderamente excepcional" de la rúbrica §7.
- No introduce una librería de Result (FluentResults, LanguageExt, ErrorOr) — mismo
  criterio YAGNI que el resto del proyecto (ver Vault: cliente HTTP mínimo sin SDK).

**Nota (hallazgo #3 de la auditoría, resuelto como efecto colateral de este change)**:
la auditoría original también encontró que `PendientesEndpoints.cs` inyecta el tipo
concreto de `Infrastructure/Sql/*` (`CalimacoIngresoBuscadorSql`, etc.) en vez de un
puerto — se había considerado un change B separado para eso. Con este refactor, el
Endpoint pasa a invocar `Manejador*` (Application), no el buscador SQL — la
dependencia Endpoint→Infrastructure-concreto desaparece. Endpoint→`Manejador*`
concreto no es una violación DIP: el `Manejador*` es el caso de uso mismo (núcleo),
no un adaptador intercambiable — no necesita una interfaz adicional solo para
existir. Se descarta el change B como innecesario; el hallazgo #3 queda cerrado por
las tareas 4.4/5.4 de este `tasks.md`.

## Decisions

**D1 — `Result<T>` propio, sin dependencia nueva.**
Tipo mínimo con `IsSuccess`, `Value` (solo si éxito) y `Error` (record con
`Status/Codigo/Detalle/Reintentable/Origen/SqlErrorNumber` — mismos campos que hoy
tiene `ErrorFuncionalException`, para que el mapeo a `ProblemaDetalle` no cambie).
Vive en `Application/Resultados/Result.cs`. Alternativa considerada: paquete NuGet
(FluentResults) — descartado, mismo criterio que el resto del proyecto: una necesidad
acotada (8 casos de uso, misma forma de error ya definida) no justifica una
dependencia nueva.

**D2 — Un `Manejador*` por caso de uso en `Application`, mismo patrón de clase base que
ya usa `Infrastructure/Sql` para los 6 de pendientes.**
`Infrastructure/Sql/CalimacoCmpBuscadorSqlBase<TItem>` y `SicaBuscadorSqlBase<TItem>`
ya factorizan lo común entre sus implementaciones (D4 del change original). El caso de
uso replica el mismo criterio: `ManejadorBuscarPendientesCalimacoCmpBase<TRequest,TItem>`
y `ManejadorBuscarPendientesSicaBase<TRequest,TItem>` validan y delegan al puerto
(`IBuscarPendientes<TRequest,TItem>`), devolviendo `Result<IReadOnlyList<TItem>>`; las
6 subclases finas solo proveen los códigos `GL-*` de su spec. `ManejadorCrearCorte` y
`ManejadorResolverInicioCorte` son casos de uso propios (sin jerarquía, no comparten
forma con nada más). Naming: sufijo `Manejador` = "handler" (§4 de la rúbrica,
convención ya establecida aunque no usada todavía en este proyecto).

**D3 — Los adaptadores SQL dejan de referenciar `ErrorFuncionalException` para
validación; solo el `Manejador*` la usa (o `Result<T>.Fallo(...)`, ver D1).**
`CorteCreatorSql`, `CorteResolverSql`, `CalimacoCmpBuscadorSqlBase`,
`SicaBuscadorSqlBase` pierden sus métodos `Validar`. Firma de los puertos
(`ICorteCreator`, `ICorteResolver`, `IBuscarPendientes<TRequest,TItem>`) no cambia —
siguen siendo el contrato entre `Application` e `Infrastructure`, solo que ahora se
asume que quien los invoca (el `Manejador*`) ya validó.

**D4 — Endpoints traducen `Result<T>` a `IResult` con un helper único, no un switch por
endpoint.**
`Api/Endpoints/ResultadoHttp.cs`: `static IResult Responder<T>(Result<T> resultado, Func<T,IResult> exito)` 
— si `IsSuccess`, ejecuta `exito(resultado.Value)` (ej. `Results.Json`, `Results.Created`);
si falla, arma `ProblemaDetalle` desde `resultado.Error` con el mismo `ProblemaDetalle.Crear`
que ya existe. Mismo criterio DRY que D3 del change original (mapeo de errores en un solo
lugar, no repetido por handler).

**D5 — Alcance de este change: las 8 operaciones de negocio, no la idempotencia de
`crearCorte`.**
`CorteEndpoints.CrearCorteAsync` ya tiene su propia lógica de idempotencia (D7 del
change original) con un `throw ErrorFuncionalException.DeReglaEspecifica` para el
conflicto 409 (`GL-IDEMP-001`). Ese `throw` está en el Endpoint, no en un adaptador de
Infrastructure — no es el hallazgo #2 de la auditoría (validación en el lugar
equivocado), así que puede quedar como excepción de corto-circuito en el Endpoint por
ahora, o convertirse también a `Result<T>` si al implementar `ManejadorCrearCorte` es
natural incluirlo (decisión de implementación, no bloqueante para este design).

## Risks / Trade-offs

- [Cambio toca las 8 operaciones a la vez — blast radius grande para un refactor de
  mecanismo interno] → Mitigación: implementar y verificar con test unitario cada
  `Manejador*` antes de tocar su Endpoint; correr la suite completa (`dotnet test`,
  hoy 76/76 verde) antes de considerar el change cerrado; verificar con `curl`/Postman
  contra al menos un caso de éxito y un caso de fallo por operación que la respuesta
  HTTP es byte-idéntica en forma a la de antes del refactor.
- [Convención `Manejador*` no existía todavía en este proyecto] → Mitigación: es la
  misma convención ya documentada en el estándar del usuario (`dotnet-clean-style` §4),
  usada en el proyecto hermano `reto_tecnico_backend_senior` — no es una invención
  nueva de este change.
- [`ErrorFuncionalException.DeReglaEspecifica`/`DeContratoGenerico` quedan sin uso en
  el camino de negocio, pero siguen usados por los middlewares de seguridad (fuera de
  alcance, ver Non-Goals) y por la idempotencia de `crearCorte` (D5)] → No se eliminan
  esos factory methods; se documenta en el código que su uso legítimo restante es
  middleware/idempotencia, no casos de uso.

## Open Questions

1. `Result<T>` como `readonly record struct` (sin allocation) vs `sealed class` — a
   decidir en implementación; no afecta el contrato observable ni las tasks.
2. ¿El `throw` de idempotencia en `CorteEndpoints.CrearCorteAsync` (D5) se convierte
   también a `Result<T>` al implementar `ManejadorCrearCorte`, o se deja como está? No
   bloqueante — se resuelve en la tarea correspondiente de `tasks.md`.
