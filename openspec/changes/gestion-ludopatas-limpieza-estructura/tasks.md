## 1. Mover capa de entrada bajo Api/

- [x] 1.1 Crear `src/GestionLudopatas.Api/Api/Endpoints/`, `Api/Middleware/`,
  `Api/Security/`.
- [x] 1.2 Mover `Endpoints/CorteEndpoints.cs` y `Endpoints/PendientesEndpoints.cs` a
  `Api/Endpoints/` — namespace sin cambios (`GestionLudopatas.Api.Endpoints`).
- [x] 1.3 Mover `Middleware/ManejadorExcepcionesGlobal.cs` y
  `Middleware/TrazabilidadMiddleware.cs` a `Api/Middleware/` — namespace sin cambios.
- [x] 1.4 Mover `Security/ApiKeyAuthenticationMiddleware.cs` y
  `Security/IpAllowlistMiddleware.cs` a `Api/Security/` — namespace sin cambios.
- [x] 1.5 Borrar las carpetas `Endpoints/`, `Middleware/`, `Security/` originales
  (deben quedar vacías tras 1.2-1.4).
- [x] 1.6 Mover `test/GestionLudopatas.Api.Tests/Endpoints/*` a
  `test/GestionLudopatas.Api.Tests/Api/Endpoints/` y
  `test/GestionLudopatas.Api.Tests/Security/*` a
  `test/GestionLudopatas.Api.Tests/Api/Security/` — namespace sin cambios.

## 2. Extraer bootstrap de Vault de Program.cs

- [x] 2.1 Crear `Infrastructure/Vault/VaultBootstrapExtensions.cs`: método de
  extensión `CargarSecretosSiHabilitadoAsync(this WebApplicationBuilder builder)` con
  el contenido hoy inline en `Program.cs` líneas 14-37 (chequeo `Vault:Habilitado`,
  fetch de `pathDb`/`pathApiKey`, escritura en `builder.Configuration`).
- [x] 2.2 Reemplazar el bloque inline en `Program.cs` por
  `await builder.CargarSecretosSiHabilitadoAsync();`.
- [x] 2.3 Confirmar que `Program.cs` queda solo con: creación del builder, la línea
  de Vault, registro de servicios (`AddPersistenciaSql`, `AddExceptionHandler`,
  `AddProblemDetails`), build, pipeline de middleware, mapeo de endpoints, `app.Run()`.

## 3. Eliminar carpetas vacías

- [x] 3.1 Borrar `Application/Seguridad/`, `Domain/Cortes/`, `Domain/Pendientes/`.

## 4. Verificación

- [x] 4.1 **Prueba determinística (requirement "No existen carpetas vacías")**:
  agregar `test/GestionLudopatas.Api.Tests/Infrastructure/EstructuraProyectoTests.cs`
  — test que recorre `src/GestionLudopatas.Api/` (resuelto vía ruta relativa desde el
  assembly de test) excluyendo `bin/`/`obj/` y falla si encuentra alguna carpeta sin
  archivos.
- [x] 4.2 **Prueba determinística (requirement "Program.cs no contiene lógica de
  bootstrap inline")**: mismo archivo de test — lee el contenido de `Program.cs` vía
  ruta relativa y falla si contiene el literal `"Vault:Address"` o `VaultSecretClient`.
- [x] 4.3 `dotnet build` sin errores ni warnings nuevos.
- [x] 4.4 `dotnet test` — línea base 76/76 + los 2 tests nuevos de 4.1/4.2 en verde.
- [x] 4.5 Actualizar `openspec/changes/gestion-ludopatas-casos-uso-result/design.md` y
  `tasks.md`: reemplazar referencias a `Endpoints/CorteEndpoints.cs`,
  `Endpoints/PendientesEndpoints.cs`, `Middleware/ManejadorExcepcionesGlobal.cs` por
  sus rutas nuevas bajo `Api/`.
