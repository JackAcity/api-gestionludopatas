## 1. `Result<T>` base

- [x] 1.1 Crear `Application/Resultados/Result.cs`: tipo `Result<T>` con `IsSuccess`,
  `Value` (solo si éxito), `Error` (record `ResultadoError(int Status, string Codigo,
  string Detalle, bool Reintentable, string Origen, int? SqlErrorNumber)`), factory
  `Result<T>.Ok(value)` / `Result<T>.Fallo(error)`.
- [x] 1.2 Crear `Api/Endpoints/ResultadoHttp.cs` (ver
  `gestion-ludopatas-limpieza-estructura`, aplicado antes que este change):
  `static IResult Responder<T>(Result<T>
  resultado, Func<T, IResult> exito)` — si falla, arma `ProblemaDetalle` desde
  `resultado.Error` reusando `ProblemaDetalle.Crear` existente.
- [x] 1.3 Test unitario de `Result<T>`: `Ok` expone `Value` y `IsSuccess=true`;
  `Fallo` expone `Error` y `IsSuccess=false`; acceder a `Value` en un `Result` fallido
  lanza (o el diseño elegido para ese caso) — cubrir el comportamiento elegido con un
  test explícito.

## 2. Caso de uso piloto: crear corte

- [x] 2.1 Crear `Application/Cortes/ManejadorCrearCorte.cs`: valida
  `TipoCorte`/`FechaHoraCorte`/`FechaHoraEjecucion` (misma lógica hoy en
  `CorteCreatorSql.Validar`), devuelve `Result<CrearCorteResponse>.Fallo(...)` con el
  código `GL-CORTE-CRE-00X` correspondiente sin invocar `ICorteCreator`; si válido,
  invoca `ICorteCreator.CrearAsync` y envuelve la respuesta en `Result.Ok`.
- [x] 2.2 Quitar `Validar` de `CorteCreatorSql` — el adaptador solo ejecuta
  `SP_CORTE_Crear` y mapea `@corte_id`.
- [x] 2.3 `Api/Endpoints/CorteEndpoints.cs`: `POST /api/v1/cortes` invoca
  `ManejadorCrearCorte` en vez de `ICorteCreator` directo, traduce con
  `ResultadoHttp.Responder`.
- [x] 2.4 **Prueba determinística (hallazgo #1 y #2, requirement "El adaptador SQL
  nunca se invoca cuando la validación de negocio falla")**: en
  `test/GestionLudopatas.Api.Tests/Application/Cortes/ManejadorCrearCorteTests.cs`,
  para cada regla de `corte-crear` (tipoCorte inválido, fechaHoraCorte
  requerida/debe-ser-nula, fechaHoraEjecucion requerida): `Result<T>.IsSuccess ==
  false` con el `Codigo` `GL-*` correcto, usando un `ICorteCreator` canario que lanza
  si se invoca. Reemplaza el equivalente hoy en `ValidacionAdaptadoresTests.cs` para
  `CorteCreatorSql`.

## 3. Caso de uso: resolver inicio de corte

- [x] 3.1 Crear `Application/Cortes/ManejadorResolverInicioCorte.cs`: valida
  `FechaHoraEvaluacion`/`TimeoutMinutos` (misma lógica hoy inline en
  `CorteResolverSql`), devuelve `Result<ResolverInicioResponse>.Fallo(...)` sin
  invocar `ICorteResolver` si inválido.
- [x] 3.2 Quitar la validación inline de `CorteResolverSql` — el adaptador solo
  ejecuta `SP_CORTE_ResolverInicio` dentro de su transacción propia.
- [x] 3.3 `Api/Endpoints/CorteEndpoints.cs`: `POST /api/v1/cortes/resoluciones-inicio`
  invoca `ManejadorResolverInicioCorte`, traduce con `ResultadoHttp.Responder`.
- [x] 3.4 **Prueba determinística**: `ManejadorResolverInicioCorteTests.cs` — para
  `fechaHoraEvaluacion` ausente y `timeoutMinutos` negativo, `Result<T>` de fallo con
  `GL-CORTE-RES-001`/`GL-CORTE-RES-002`, `ICorteResolver` canario nunca invocado.

## 4. Casos de uso: pendientes CALIMACO/CMP (4 operaciones)

- [x] 4.1 Crear `Application/Pendientes/ManejadorBuscarPendientesCalimacoCmpBase.cs`
  (clase base, mismo criterio que `CalimacoCmpBuscadorSqlBase` en Infrastructure):
  valida `CorteIdActual`/`MaxReintentosPorSistema`/`EsReintentoForzado` (misma lógica
  hoy en `CalimacoCmpBuscadorSqlBase.Validar`), delega a
  `IBuscarPendientes<PendientesCalimacoCmpRequest,TItem>` si válido.
  **Nota de implementación**: CALIMACO y CMP ingreso/salida comparten el mismo
  `TRequest`/`TItem` — se agregaron 4 interfaces marcador
  (`IBuscarPendientesCalimacoIngreso`/`...Salida`/`...CmpIngreso`/`...CmpSalida`, ver
  `Application/Pendientes/IBuscarPendientes.cs`) porque el genérico solo era ambiguo
  para DI; la base quedó parametrizada en `TItem, TPuerto`.
- [x] 4.2 Crear las 4 subclases finas (`ManejadorBuscarPendientesCalimacoIngreso`,
  `...CalimacoSalida`, `...CmpIngreso`, `...CmpSalida`) proveyendo solo sus códigos
  `GL-*`.
- [x] 4.3 Quitar `Validar` de `CalimacoCmpBuscadorSqlBase` — los 4 adaptadores
  concretos (`CalimacoIngresoBuscadorSql`, etc.) solo ejecutan su SP y mapean fila.
- [x] 4.4 `Api/Endpoints/PendientesEndpoints.cs`: las 4 rutas CALIMACO/CMP invocan el
  `Manejador*` correspondiente (inyectado por tipo concreto de la subclase fina) en
  vez del buscador SQL directo — esto cierra también el hallazgo #3 de la auditoría
  (endpoint dependía de `Infrastructure/Sql/*` concreto); no hace falta una interfaz
  adicional sobre `Manejador*`, ver design.md.
- [x] 4.5 **Prueba determinística**: un test por cada una de las 4 operaciones —
  `corteIdActual<=0`, `maxReintentosPorSistema<=0`, `esReintentoForzado` enviado-null
  → `Result<T>` de fallo con el código `GL-PEND-*` correcto, puerto SQL canario nunca
  invocado.

## 5. Casos de uso: pendientes SICA (2 operaciones)

- [x] 5.1 Crear `Application/Pendientes/ManejadorBuscarPendientesSicaBase.cs`: valida
  `MaxReintentosPorSistema` (misma lógica hoy en `SicaBuscadorSqlBase`), delega a
  `IBuscarPendientes<PendientesSicaRequest,TItem>` si válido.
- [x] 5.2 Crear `ManejadorBuscarPendientesSicaIngreso` y `...SicaSalida`.
- [x] 5.3 Quitar la validación inline de `SicaBuscadorSqlBase`.
- [x] 5.4 `Api/Endpoints/PendientesEndpoints.cs`: las 2 rutas SICA invocan el
  `Manejador*` correspondiente.
- [x] 5.5 **Prueba determinística**: `maxReintentosPorSistema<=0`/`null` → `Result<T>`
  de fallo con `GL-PEND-SICA-ING-001`/`GL-PEND-SICA-SAL-001`, puerto SQL canario
  nunca invocado.

## 6. Limpieza y verificación de contrato

- [x] 6.1 Revisar `ErrorFuncionalException`: confirmar que `DeReglaEspecifica` y
  `DeContratoGenerico` solo quedan referenciados desde
  `ApiKeyAuthenticationMiddleware`, `IpAllowlistMiddleware` y la idempotencia de
  `CorteEndpoints.CrearCorteAsync` (fuera de alcance, ver design.md Non-Goals/D5) —
  ningún `Manejador*` ni adaptador `Infrastructure/Sql/*` los usa más. Confirmado por
  grep: son los únicos 4 call-sites en producción.
- [x] 6.2 Reescribir/eliminar en `ValidacionAdaptadoresTests.cs` los casos que hoy
  esperan `Assert.ThrowsAsync<ErrorFuncionalException>()` contra un adaptador SQL de
  validación — ese comportamiento se movió a los tests de los `Manejador*` (secciones
  2-5). No quedó ninguna responsabilidad de validación en el adaptador — el archivo
  se eliminó entero (y la carpeta `test/.../Infrastructure/Sql/`, que quedó vacía).
- [x] 6.3a `dotnet test` completo: 98/98 en verde (línea base 78/78 tras
  `gestion-ludopatas-limpieza-estructura` + 20 tests nuevos de este change).
- [x] 6.3b **Verificado en DEV (2026-08-10)** — comparación `curl` de una regla
  por operación, antes/después del refactor. Antes: imagen anterior
  `gestion-ludopatas-api:rollback-20260810-185101` en contenedor temporal aislado;
  después: contenedor DEV `gestion_ludopatas_api` en `9012`. Los 8 pares
  `status=422` + `code` fueron idénticos: `GL-CORTE-RES-001`, `GL-CORTE-CRE-001`,
  `GL-PEND-CAL-ING-001`, `GL-PEND-CAL-SAL-001`, `GL-PEND-CMP-ING-001`,
  `GL-PEND-CMP-SAL-001`, `GL-PEND-SICA-ING-001` y `GL-PEND-SICA-SAL-001`.
  La versión posterior además corrigió el media type a `application/problem+json`.
- [x] 6.4 Actualizar `openspec/changes/gestion-ludopatas-api/design.md` o el README
  del proyecto para referenciar esta decisión (Result<T> para casos de uso) como
  parte de la arquitectura vigente, si aplica según convención del repo. Agregado
  como Decisión D10 en `openspec/changes/gestion-ludopatas-api/design.md`.
