## 1. Scaffolding de la solution

- [x] 1.1 Crear proyecto Web API .NET 10 (minimal API) en `api-gestionludopatas/src/GestionLudopatas.Api/`. Carpeta raíz y `.slnx` renombrados a `api-gestionludopatas` (convención del equipo: prefijo `api-` para backend puro) — csproj/namespace C# quedan `GestionLudopatas.Api` porque un namespace no admite guiones
- [x] 1.2 Crear carpetas por capa dentro del proyecto único (D1): `Domain/`, `Application/`, `Infrastructure/`, `Endpoints/`, `Security/`, `Middleware/`
- [x] 1.3 Agregar paquete `Microsoft.Data.SqlClient` (sin ORM — llamadas directas a SP, igual patrón que `ReglasCargaSql` del proyecto hermano)
- [x] 1.4 Configurar `appsettings.json`/`appsettings.Development.json` con placeholders (sin secretos) y variables de entorno para connection string y API Keys

## 2. Modelo de errores común (spec `modelo-error-comun`)

- [x] 2.1 Definir `Domain/Errores/CodigoError.cs` con las constantes `GL-*` (23 runtime + 6 deploy + transversales)
- [x] 2.2 Definir `Domain/Errores/ErrorMapeoSql.cs`: diccionario `int sqlErrorNumber -> (HttpStatus, Code, MensajePublico, Retryable)` data-driven (D3), poblado 1:1 desde `endpoint/MATRIZ_MAPEO_ERRORES_SQL_HTTP.md` secciones 3 y 4
- [x] 2.3 Definir `Application/Errores/ProblemaDetalle.cs` (record con `type/title/status/code/detail/traceId/timestamp/retryable/source/sqlErrorNumber?/violations?`)
- [x] 2.4 Implementar `Middleware/ManejadorExcepcionesGlobal.cs` (`IExceptionHandler`) que capture `SqlException`, use `ErrorMapeoSql` y nunca filtre stack trace/connection string
- [x] 2.5 Implementar `Middleware/TrazabilidadMiddleware.cs`: `X-Trace-Id` obligatorio de salida, `X-Correlation-Id` de entrada propagado si viene
- [x] 2.6 Implementar precedencia de validación API↔SQL (sección 5.1 del contrato) como helper reusable por los 8 handlers — `ErrorFuncionalException` con factories `DeReglaEspecifica`/`DeContratoGenerico`/`DeSql`

## 3. Seguridad (spec `seguridad-acceso-api`)

- [x] 3.1 Implementar `Security/ApiKeyAuthenticationMiddleware.cs`: valida header `X-Api-Key` con comparación de tiempo constante, produce `401 GL-AUTH-001` si falta/inválida. Desviación de nombre: middleware plano en vez de `AuthenticationHandler<TOptions>` — no necesitamos claims/esquemas múltiples, un `IAuthenticationHandler` completo hubiera sido ceremonia sin uso real
- [x] 3.2 Implementar `Security/IpAllowlistMiddleware.cs`: rechaza con `403 GL-AUTH-002` si la IP de origen no está en la lista configurada (soporta IP exacta y CIDR vía `IPNetwork` nativo)
- [x] 3.3 Registrar ambos en el pipeline (`Program.cs`, antes de mapear endpoints); `/health` excluido explícitamente de ambos (lo pega el healthcheck de Docker sin credenciales, igual criterio que `api-sica`)
- [x] 3.4 Documentar en `README.md` cómo rotar la API Key y actualizar la allowlist on-premise

## 4. Persistencia SQL

- [x] 4.1 Implementar `Infrastructure/Sql/ISqlConnectionFactory.cs` / `SqlConnectionFactory.cs` (connection string desde `IConfiguration`, nunca hardcodeada)
- [x] 4.2 Documentar en `README.md` el script de aprovisionamiento del login SQL con `GRANT EXECUTE` acotado a los 8 SP (D5)
- [x] 4.3 Implementar healthcheck `Infrastructure/Sql/EsquemaHealthCheck.cs`: verifica que los 8 SP y las 2 tablas existan en `bd_autobot` antes de servir tráfico (D6)

## 5. Casos de uso — Cortes

- [x] 5.1 Puerto `Application/Cortes/ICorteResolver.cs` + adaptador `Infrastructure/Sql/CorteResolverSql.cs` (`SP_CORTE_ResolverInicio`, transacción propia)
- [x] 5.2 Endpoint `Endpoints/CorteEndpoints.cs`: `POST /api/v1/cortes/resoluciones-inicio` según spec `corte-resolver-inicio`
- [x] 5.3 Puerto `Application/Cortes/ICorteCreator.cs` + adaptador `Infrastructure/Sql/CorteCreatorSql.cs` (`SP_CORTE_Crear`)
- [x] 5.4 Endpoint `POST /api/v1/cortes` según spec `corte-crear`
- [x] 5.5 Implementar almacén de idempotencia `Infrastructure/Idempotencia/IdempotencyStore.cs` (D7): fingerprint de payload, replay, `409 GL-IDEMP-001` en conflicto. Desviación: en memoria (`ConcurrentDictionary`), no SQLite — se evaluó Microsoft.Data.Sqlite y se descartó por traer SQLitePCLRaw.lib.e_sqlite3 con vulnerabilidad alta conocida (GHSA-2m69-gcr7-jv3q) sin versión parcheada limpia; ver comentario `ponytail:` en el archivo
- [x] 5.6 Cablear idempotencia en el endpoint de crear corte (header `Idempotency-Key`, 16–128 caracteres)

## 6. Casos de uso — Pendientes

- [x] 6.1 Puerto común `Application/Pendientes/IBuscarPendientes.cs` (`Task<IReadOnlyList<TItem>> BuscarAsync(TRequest, CancellationToken)`) para uniformar los 6 adaptadores (D4)
- [x] 6.2 Adaptador + endpoint CALIMACO ingreso (`SP_Pendientes_CALIMACO_Ingreso`) según spec `pendientes-calimaco-ingreso`
- [x] 6.3 Adaptador + endpoint CALIMACO salida (`SP_Pendientes_CALIMACO_Salida`) según spec `pendientes-calimaco-salida`
- [x] 6.4 Adaptador + endpoint CMP ingreso (`SP_Pendientes_CMP_Ingreso`) según spec `pendientes-cmp-ingreso`
- [x] 6.5 Adaptador + endpoint CMP salida (`SP_Pendientes_CMP_Salida`) según spec `pendientes-cmp-salida`
- [x] 6.6 Adaptador + endpoint SICA ingreso (`SP_Pendientes_SICA_Ingreso`, sin `corteIdActual`/`esReintentoForzado`, `nombresApellidos` no nulo) según spec `pendientes-sica-ingreso`
- [x] 6.7 Adaptador + endpoint SICA salida (`SP_Pendientes_SICA_Salida`, `nombresApellidos`/`fechaInscripcion` nullable) según spec `pendientes-sica-salida`

## 7. Despliegue Docker — Fase 1, dev con config plana (spec `despliegue-docker`)

Objetivo de esta fase: tener los 8 endpoints corriendo en el ambiente dev real, contra
`bd_autobot` real, verificables end-to-end — **sin Vault todavía**. El secreto (connection
string + API Key) viaja como variable de entorno normal en el `.env`/compose de dev, igual
que hacía `api-sica` antes de su propia migración a Vault. La sección 8 hace el cutover.

- [x] 7.1 `Dockerfile` multi-stage (.NET 10 SDK build → runtime), usuario no root
- [x] 7.2 `docker-compose.yml` de dev con `ConnectionStrings__BdAutobot`/`Seguridad__ApiKey`/`Seguridad__IpsPermitidas` como variables de entorno explícitas (sin Vault). Puerto `9012` confirmado libre contra `ss -tlnp` real del server DEV01 (2026-08-08) — `8092` estaba ocupado
- [x] 7.3 **OpenAPI segura desplegada en DEV (2026-08-11)** — GHSA-v5pm-xwqc-g5wc fue corregida en `Microsoft.OpenApi` 2.7.5; `Microsoft.AspNetCore.OpenApi` 10.0.11 exige esa versión mínima. Se añadió Scalar 2.16.18 sin dependencias transitivas para la UI. Solo `Development` mapea `/openapi/v1.json` y `/docs`; ambas rutas omiten API key únicamente para cargar la UI, pero permanecen tras la allowlist de IP. El documento declara `X-Api-Key` en sus 8 operaciones, Scalar no persiste autenticación y Agent/fuentes externas están deshabilitados. Verificado en DEV: health/OpenAPI/Scalar `200` y endpoint de negocio sin clave `401`; `dotnet list package --vulnerable --include-transitive` no reporta paquetes vulnerables.
- [x] 7.4 Desplegado en `SRV VXRDEVAPP01` (`10.99.200.100:32451`, mismo server que `api-sica`), directorio propio `/acity/gestionludopatas/api-gestionludopatas/` (no toca `/acity/sica/`). Build + `docker compose up -d` OK, contenedor `gestion_ludopatas_api` corriendo en `9012:8080`. `.env` con connection string de password **fake** (`bd_autobot` real, login `app_api_rw_gestionludopatas` — el DBA todavía no lo creó) y API Key real generada
- [x] 7.5 Smoke test manual completo (2026-08-08) contra el deploy real: `/health` → `503` (esperado, password fake); sin `X-Api-Key` → `401 GL-AUTH-001`; API Key válida pero IP fuera de allowlist → `403 GL-AUTH-002` (allowlist ajustada al subnet real del bridge Docker, `192.168.144.0/20`, distinto de lo asumido); API Key inválida → `401`; los 5 endpoints que requieren SQL → `500 GL-API-UNEXPECTED-001` limpio (login failed 18456, sin filtrar detalle); 1 caso de validación pura (`calimaco/ingresos` sin `corteIdActual`) → `422 GL-PEND-CAL-ING-001` **sin tocar SQL**, confirma que el canary pattern de los tests unitarios se sostiene en real. **Bug real encontrado y corregido en esta corrida**: `ManejadorExcepcionesGlobal` filtraba `sqlErrorNumber` en el fallback no-catalogado (ej. 18456) — la spec dice que ese campo es solo para errores catalogados (510xx/511xx). Fix: `ManejarSqlException` ahora usa `ErrorMapeoSql.PorNumero.TryGetValue` en vez de `Resolver`, cae a `ManejarNoClasificado` (sin número, `source:"api"`) si no está catalogado — verificado de nuevo en vivo tras el fix, `sqlErrorNumber:null` confirmado. 76/76 tests siguen verdes tras el fix

## 8. Secretos vía Vault — Fase 2, cutover (spec `secretos-vault`)

Se hace **después** de validar que el servicio funciona en dev con config plana (sección 7).
Reemplaza la fuente del secreto sin tocar `ISqlConnectionFactory` ni los endpoints.

- [x] 8.1 Implementar `Infrastructure/Vault/VaultSecretClient.cs`: `HttpClient` mínimo, `GET {VAULT_ADDRESS}/v1/secret/data/{path}`, header `X-Vault-Token`, parseo `data.data` — sin SDK, genérico a propósito (no sabe qué es una connection string ni una API Key, single responsibility), mismo patrón que `api-sica` (D8)
- [x] 8.2 Resolver **dos** secretos en `Program.cs` **antes** de `builder.Build()` (paths separados por SRP, corrección explícita del usuario): connection string desde `Vault:PathDb`, API Key desde `Vault:PathApiKey`; ambos inyectados en `builder.Configuration` antes de registrar `AddPersistenciaSql`
- [x] 8.3 Fail-fast: `Vault:Address`/`Vault:Token`/`Vault:PathDb`/`Vault:PathApiKey` ausentes, Vault no-2xx en cualquiera de los dos paths, red inaccesible, campo requerido faltante o `engine` distinto de `sqlserver` en el secreto de BD → la app no arranca (cubierto por tests, tarea 9.6)
- [x] 8.4 `Infrastructure/Vault/VaultHttpClientFactory.cs`: si se configura `Vault:RutaCaInterna`, confía explícitamente en esa CA (custom trust store); si no, usa la cadena de confianza del sistema — nunca deshabilita validación TLS
- [x] 8.5 El usuario confirmó la creación de `api-gestionludopatas/db` y `api-gestionludopatas/apikey` — **sin segmento de ambiente**, mismo path para dev y qa (decisión explícita: replicar estructura de prod). El secreto `db` contiene `engine=sqlserver` más `host`/`port`/`dbname`/`username`/`password`; `apikey` contiene únicamente `api_key`. Los valores reales no se registran en este repositorio.
- [x] 8.6 **Cutover DEV completado 2026-08-11** — ambos paths de Vault devolvieron `200` desde `VXRDEVAPP01`; el contenedor arrancó y `/health` respondió `200` con secretos de Vault. Tras retirar `DB_CONNECTION_STRING` y `API_KEY` del `.env` remoto y recrear el contenedor, `/health` volvió a responder `200`, demostrando que no existe fallback plano. Una llamada autenticada con la API Key obtenida de Vault solo en memoria devolvió `422 GL-CORTE-RES-001`. DEV configura transitoriamente `SQLSERVER_TRUST_SERVER_CERTIFICATE=true` porque SQL Server usa certificado autofirmado/no coincidente; retirar la excepción cuando el DBA entregue certificado/CA válidos.

## 9. Tests unitarios (nuestro lado — sin depender de `bd_autobot` real)

Alcance del deliverable: **unitarios**, con los 8 puertos (`ICorteResolver`, `ICorteCreator`,
`IBuscarPendientes*`) mockeados/fake — nunca contra `bd_autobot` real. `bd_autobot` es una
instancia on-prem compartida, no algo que se levante por test run (a diferencia del Postgres
dockerizado del reto). La verificación contra la base y el Vault reales queda cubierta por
la colección Postman + `newman` corriendo contra el deploy real (spec `coleccion-postman`,
tareas 7.5/8.6) — eso corre por fuera de `dotnet test`.

- [x] 9.1 `ErrorMapeoSql`: los 23 números runtime + 6 deploy + 8 nativos SQL Server cubiertos por `Theory`, cada uno resuelve al `code`/`status`/`retryable` exacto de la matriz — falla si algún código cambia sin querer
- [x] 9.2 Precedencia API↔SQL: prevalidación (`DeReglaEspecifica`) y error simulado del SP (`DeSql` sobre `ErrorMapeoSql.Resolver`) para la misma condición producen el mismo `code`
- [x] 9.3 `crearCorte` con `ICorteCreator` fake + `IdempotencyStore` en memoria: misma `Idempotency-Key` + mismo payload → replay sin invocar el puerto una segunda vez; mismo key + payload distinto → `409 GL-IDEMP-001`; key fuera de rango → `422 GL-API-REQ-002`. Extendido post-auditoría: dos solicitudes simultáneas con misma clave/payload invocan el puerto una sola vez y la segunda recibe replay; con payload distinto, el conflicto es inmediato sin esperar ni invocar el SP.
- [x] 9.4 Validación de cada adaptador con una fábrica de conexión canary (lanza si se llega a intentar abrir SQL): cobertura completa en `CorteResolverSql`/`CorteCreatorSql`/`CalimacoIngresoBuscadorSql`/`SicaIngresoBuscadorSql`; **alcance reducido** — no se replicó 1:1 para los 4 adaptadores restantes (`CalimacoSalida`/`CmpIngreso`/`CmpSalida`/`SicaSalida`), siguen exactamente el mismo patrón ya probado (misma clase base, mismos códigos por sistema) y no se agregó el "caso feliz" con SP real (no mockeable sin infra, ver nota de alcance arriba) — pendiente si se quiere cobertura exhaustiva
- [x] 9.5 `ApiKeyAuthenticationMiddleware`/`IpAllowlistMiddleware` en aislado: sin `X-Api-Key` → `401`; API Key válida pero IP fuera de allowlist (exacta y CIDR) → `403`; `/health` exento en ambos. La prueba cubre también una segunda entrada independiente de `Seguridad:IpsPermitidas`, para conservar UiPath y autorizar un operador sin concatenar IPs.
- [x] 9.6 `VaultSecretClient` con `HttpMessageHandler` fake (mismo formato que `vault.util.spec.ts` de `api-sica`, V-01 a V-06): `200` con los seis campos de BD → objeto resuelto; `403`/`404`/campo faltante/red inaccesible → excepción. `VaultCampos` exige además `engine=sqlserver`, falla ante motor ausente o incompatible y construye la cadena SQL con `Encrypt=True` preservando la política explícita de certificado.

**Resultado**: `dotnet test` → 125/125 verdes, 0 errores (verificado tras exigir
`engine=sqlserver` y cubrir la política TLS de SQL Server el 2026-08-11).

## 10. Colección Postman (spec `coleccion-postman`)

- [x] 10.1 `postman/GestionLudopatas.postman_collection.json`: 8 requests (uno por operación), auth de colección `X-Api-Key` (nada de OAuth2), 1 ejemplo de error guardado por request tomado literal de su spec. Escrita desde cero en vez de editar `endpoint/GestionLudopatas_Postman_Collection_v0.2.0-propuesta.json` (387KB, construida para el flujo OAuth2 descartado) — más simple que cirugía sobre ese archivo
- [x] 10.2 `postman/GestionLudopatas.postman_environment.json`: variables `baseUrl`/`apiKey`/`correlationId`/`idempotencyKey`, sin variables de OAuth2
- [x] 10.3 **Ejecutado nuevamente en DEV (2026-08-11)** — colección de contrato Newman: 8 requests, 8 prerequest scripts, 8 test scripts y 8 assertions, todos en verde. Corrió en un contenedor temporal `postman/newman:6-alpine` con red host de `VXRDEVAPP01` (el origen externo recibe correctamente `403 GL-AUTH-002` por la allowlist). Validó por operación `422`, `GL-*`, `application/problem+json`, `X-Trace-Id` y propagación de `X-Correlation-Id`, sin SQL ni escrituras. Las respuestas 200 de `GestionLudopatas.postman_collection.json` siguen fuera de la automatización: crean datos y requieren login real, datos aprobados y autorización operacional.
- [x] 10.4 **Lectura funcional segura ejecutada en DEV (2026-08-11)** — `SP_Pendientes_SICA_Ingreso` y `SP_Pendientes_SICA_Salida` devolvieron `200 []` con API Key resuelta desde Vault, esquema de arreglo válido y sin reserva/escritura de filas. `GestionLudopatas.readonly.postman_collection.json` permite a QA repetir estas dos verificaciones con Newman sin ejecutar `POST /cortes`.

## 11. Documentación y cierre

- [x] 11.1 `README.md` del nuevo proyecto: cómo levantar local, cómo desplegar on-premise, cómo rotar API Key, cómo crear el path de Vault, trazabilidad endpoint↔SP↔spec, y las 3 desviaciones de seguridad documentadas (idempotencia en memoria, OpenAPI bloqueado, auth D2)
- [ ] 11.2 **PARCIAL** — `dotnet test` confirmado en 131/131 y `dotnet build -warnaserror` en 0 warnings/0 errors. El refuerzo Result/JSON/idempotencia/DIP fue desplegado en DEV el 2026-08-10; build Docker correcto tras añadir `.dockerignore` (excluye `obj`/`bin`/`.env`) y contenedor `gestion_ludopatas_api` recreado. Newman DEV pasó 8/8 (10.3) y el `curl` antes/después mantuvo los 8 `status+code` (change Result 6.3b). El cutover a Vault (8.6) quedó verificado el 2026-08-11 con ambos paths `200`, salud `200` sin fallback plano y una llamada autenticada correcta. La allowlist de DEV conserva UiPath y expone una segunda entrada de operador; tras el redeploy, salud `200`. La primera respuesta funcional `200 []` de los dos SP SICA de solo lectura quedó comprobada (10.4). OpenAPI/Scalar seguro quedó desplegado y verificado (7.3). Falta: certificado SQL Server válido para retirar `TrustServerCertificate=true` y aplicar/verificar la idempotencia compartida definida en `gestion-ludopatas-idempotencia-persistente` antes de escalar a más de una instancia. El change **no** está listo para archivar todavía — depende de esas condiciones externas.
