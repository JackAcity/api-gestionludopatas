## Why

Auditoría `dotnet-audit` sobre `GestionLudopatas.Api` (2026-08-10) encontró que los
fallos de negocio esperados (validación de campos, reglas del contrato SQL) se
propagan lanzando `ErrorFuncionalException`, capturada por un `catch` genérico a
nivel de framework (`ManejadorExcepcionesGlobal`, un `IExceptionHandler` global). La
rúbrica de este proyecto (`dotnet-clean-style` §7) nombra exactamente este patrón como
la señal de que se está usando el mecanismo equivocado: un fallo esperado (no una
falla de infraestructura ni un bug) debería devolverse como `Result<T>` desde el caso
de uso, no lanzarse. Como consecuencia directa, la validación de esas reglas terminó
viviendo dentro de los adaptadores SQL (`Infrastructure/Sql/*`) en vez del núcleo
(`Application`), porque no existe hoy una clase de caso de uso donde ponerla — mezcla
"hablar con SQL Server" y "validar regla de negocio" en la misma clase (violación SRP,
hallazgo #2 de la misma auditoría). Un adaptador nuevo detrás del mismo puerto (D4)
tendría que duplicar esa validación.

## What Changes

- Se introduce `Result<T>` (tipo interno del proyecto, sin dependencia nueva) para
  representar el resultado de un caso de uso: éxito con valor, o fallo con
  `(int Status, string Codigo, string Detalle, bool Reintentable, string Origen)` —
  mismos campos que hoy expone `ErrorFuncionalException`.
- Se crean clases de caso de uso en `Application` (una por operación: crear corte,
  resolver inicio de corte, y una por cada una de las 6 búsquedas de pendientes) que
  validan la regla de negocio y devuelven `Result<T>` de fallo sin invocar el puerto
  SQL, o invocan el puerto y devuelven `Result<T>` de éxito.
- Los adaptadores `Infrastructure/Sql/*` (`CorteCreatorSql`, `CorteResolverSql`,
  `CalimacoCmpBuscadorSqlBase` + sus 4 implementaciones, `SicaBuscadorSqlBase` + sus 2
  implementaciones) pierden toda validación de negocio — solo ejecutan el SP y mapean
  filas.
- Los Endpoints (`CorteEndpoints.cs`, `PendientesEndpoints.cs`) llaman al caso de uso y
  traducen `Result<T>` a `Results.Json`/`ProblemaDetalle` según éxito/fallo, sin
  relanzar excepción para el camino esperado.
- `ManejadorExcepcionesGlobal` deja de recibir `ErrorFuncionalException` como camino
  normal de negocio. Sigue existiendo para lo verdaderamente excepcional: `SqlException`
  no clasificada como validación (timeout, deadlock, conflicto de datos, permisos) y
  cualquier excepción no clasificada (bug/invariante rota). `ErrorFuncionalException.DeSql`
  (mapeo de un `SqlException` con número catalogado) se mantiene — ahí sigue siendo una
  excepción real lanzada por infraestructura, no un fallo de negocio anticipado por la API.
- El contrato HTTP expuesto **no cambia**: mismos códigos `GL-*`, mismo status HTTP,
  misma forma de `ProblemaDetalle` para cada regla — esto es un cambio de mecanismo
  interno, no de comportamiento observable.
- **BREAKING (interno, no de API)**: `Infrastructure/Sql/*` deja de lanzar
  `ErrorFuncionalException` de validación; cualquier test que hoy espere
  `Assert.ThrowsAsync<ErrorFuncionalException>()` contra un adaptador SQL directamente
  deja de ser válido y se reescribe contra el caso de uso.

## Capabilities

### New Capabilities
- `casos-uso-result-negocio`: contrato interno de los casos de uso de
  `GestionLudopatas.Api` — cómo se representa y propaga un fallo de negocio esperado
  (`Result<T>`, sin excepción) versus una falla verdaderamente excepcional (excepción,
  capturada por `ManejadorExcepcionesGlobal`), y la regla de que la validación de
  negocio vive en el caso de uso (Application), nunca en el adaptador SQL
  (Infrastructure). Aplica de forma transversal a las 8 operaciones existentes sin
  alterar su contrato HTTP.

### Modified Capabilities
(ninguna — el comportamiento observable por HTTP de `corte-crear`,
`corte-resolver-inicio`, `pendientes-*` y `modelo-error-comun` no cambia: mismos
códigos `GL-*`, mismo status, misma forma de respuesta. Este change es puramente de
implementación interna.)

## Impact

- **Código afectado**: `Infrastructure/Sql/CorteCreatorSql.cs`, `CorteResolverSql.cs`,
  `CalimacoCmpBuscadorSqlBase.cs` (+ 4 implementaciones), `SicaBuscadorSqlBase.cs` (+ 2
  implementaciones), `Api/Endpoints/CorteEndpoints.cs`, `Api/Endpoints/PendientesEndpoints.cs`,
  `Api/Middleware/ManejadorExcepcionesGlobal.cs` (rutas post
  `gestion-ludopatas-limpieza-estructura`, aplicado antes que este change). Nuevo:
  clases de caso de uso en
  `Application/Cortes/` y `Application/Pendientes/`, y el tipo `Result<T>` en
  `Application` (o `Domain`, a decidir en design.md).
- **Tests**: `test/GestionLudopatas.Api.Tests/Infrastructure/Sql/ValidacionAdaptadoresTests.cs`
  se reescribe contra los nuevos casos de uso (el adaptador deja de lanzar); se agregan
  tests unitarios por caso de uso verificando `Result<T>` de fallo con el código `GL-*`
  correcto y que el puerto SQL (mock/canary) nunca se invoca cuando la validación falla.
- **Sin impacto en**: contrato HTTP/JSON (Postman collection, OpenAPI de referencia),
  Vault, idempotencia, seguridad (API Key/IP allowlist), despliegue Docker.
