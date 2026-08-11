## Why

La estructura de carpetas de `GestionLudopatas.Api` tiene 3 carpetas creadas por
scaffolding inicial que nunca se poblaron (`Application/Seguridad`, `Domain/Cortes`,
`Domain/Pendientes`) — ruido que hace parecer que existen conceptos de dominio que en
realidad viven en otro lado. Además, `Endpoints/`, `Middleware/` y `Security/` están
sueltas al mismo nivel que `Domain/`, `Application/`, `Infrastructure/`, sin agrupar
bajo una sola carpeta que represente la capa de entrada (adaptador driving/`*.Api` del
vocabulario de `dotnet-clean-style` §1) — a diferencia del proyecto de referencia
(`reto_tecnico_backend_senior`), que sí tiene esa capa claramente delimitada (ahí como
proyecto `.csproj` separado; acá, por ser escala minimal de un solo proyecto, el
equivalente correcto es una sola carpeta física). `Program.cs` además mezcla el
bootstrap de secretos de Vault (24 de sus 60 líneas) con el composition root — dificulta
ver de un vistazo qué arma el arranque de la aplicación.

## What Changes

- Se eliminan las 3 carpetas vacías: `Application/Seguridad`, `Domain/Cortes`,
  `Domain/Pendientes`.
- `Endpoints/`, `Middleware/` y `Security/` se mueven a `Api/Endpoints/`,
  `Api/Middleware/`, `Api/Security/` — movimiento físico únicamente, el namespace de
  cada tipo **no cambia** (`GestionLudopatas.Api.Endpoints`, `.Middleware`,
  `.Security`) para no arrastrar "Api.Api" en el nombre (el namespace raíz del proyecto
  ya es `GestionLudopatas.Api`).
- El bloque de bootstrap de Vault en `Program.cs` (resolución de secretos antes de
  `builder.Build()`) se extrae a
  `Infrastructure/Vault/VaultBootstrapExtensions.cs` como un método de extensión sobre
  `WebApplicationBuilder`, invocado con una sola línea desde `Program.cs`.
- `Program.cs` queda como composition root puro: registro de servicios, construcción
  de la app, pipeline de middleware, mapeo de endpoints — sin lógica de bootstrap
  inline.
- Los archivos de test correspondientes (`test/.../Endpoints/*`,
  `test/.../Security/*`) se mueven a `test/.../Api/Endpoints/`, `test/.../Api/Security/`
  para mantener el mismo espejo de estructura que el proyecto de producción.
- Sin cambio de comportamiento observable: mismo contrato HTTP, misma configuración,
  mismo pipeline de middleware en el mismo orden.

## Capabilities

### New Capabilities
- `estructura-proyecto`: reglas verificables sobre la organización física del código
  de `GestionLudopatas.Api` — sin carpetas vacías bajo `src/`, la capa de entrada
  (Endpoints/Middleware/Security) vive bajo una única carpeta `Api/`, y `Program.cs`
  no contiene lógica de bootstrap de secretos inline.

### Modified Capabilities
(ninguna — no cambia comportamiento HTTP ni de negocio, es reorganización física y
extracción de un bloque de arranque a su propio archivo.)

## Impact

- **Código movido** (sin cambio de namespace): `Endpoints/CorteEndpoints.cs`,
  `Endpoints/PendientesEndpoints.cs` → `Api/Endpoints/`; `Middleware/ManejadorExcepcionesGlobal.cs`,
  `Middleware/TrazabilidadMiddleware.cs` → `Api/Middleware/`;
  `Security/ApiKeyAuthenticationMiddleware.cs`, `Security/IpAllowlistMiddleware.cs` →
  `Api/Security/`.
- **Código nuevo**: `Infrastructure/Vault/VaultBootstrapExtensions.cs`.
- **Código editado**: `Program.cs` (reemplaza el bloque inline por una llamada al
  método de extensión).
- **Carpetas eliminadas**: `Application/Seguridad/`, `Domain/Cortes/`,
  `Domain/Pendientes/`.
- **Tests movidos**: `test/GestionLudopatas.Api.Tests/Endpoints/*` →
  `test/GestionLudopatas.Api.Tests/Api/Endpoints/*`;
  `test/GestionLudopatas.Api.Tests/Security/*` →
  `test/GestionLudopatas.Api.Tests/Api/Security/*`.
- **Otros changes afectados**: `openspec/changes/gestion-ludopatas-casos-uso-result/`
  referencia rutas bajo `Endpoints/`/`Middleware/` — se actualiza para apuntar a
  `Api/Endpoints/`/`Api/Middleware/` una vez este change se aplique primero (ver
  design.md, orden de aplicación).
