## Why

UiPath (el automatizador) necesita ejecutar 8 Stored Procedures de `bd_autobot` (SQL Server 2019) para el proceso de gestión de ludópatas (resolución/creación de "cortes" y búsqueda de pendientes por sistema CALIMACO/CMP/SICA). Por criterios de seguridad, UiPath no puede conectarse directo a la base de datos — eso abriría una brecha (credenciales SQL en el robot, superficie de ataque directa a `bd_autobot`). Se necesita un backend HTTP que actúe como único punto de entrada seguro, ejecute los SP y normalice sus resultados y errores.

El contrato de estos 8 SP (rutas, request/response, 29 errores `THROW` SQL mapeados a HTTP) ya fue analizado y documentado en `endpoint/PROPUESTA_CONTRATO_API_SQL_GestionLudopatas.md` y `endpoint/MATRIZ_MAPEO_ERRORES_SQL_HTTP.md`. Ese análisis es la fuente de verdad técnica para firmas y códigos de error; este change lo convierte en specs ejecutables e implementación, con una corrección deliberada sobre la seguridad propuesta ahí (ver Impact).

## What Changes

- Nuevo proyecto independiente, repo/carpeta `api-gestionludopatas` (convención del equipo: prefijo `api-` para backend puro, `app_` es para un sistema completo), solution y namespace C# `GestionLudopatas.Api` (un namespace no admite guiones) sobre .NET 10, fuera de `reto_tecnico_backend_senior`, para despliegue on-premise.
- 8 endpoints HTTP `POST`, uno por Stored Procedure, siguiendo Opción A del documento fuente (recursos explícitos, sin nombres físicos de SP en la URL).
- Modelo de error único `application/problem+json` con los 23 códigos `GL-*` runtime (510xx/511xx) ya catalogados, más los códigos transversales (`GL-API-*`, `GL-AUTH-*`, `GL-IDEMP-001`, `GL-DATA-CONFLICT-*`, `GL-SQL-*`).
- Idempotencia vía header `Idempotency-Key` solo en `crearCorte` (único SP de inserción sin clave natural).
- **Corrección de seguridad respecto al documento fuente**: se descarta OAuth2 Client Credentials (recomendado en `PROPUESTA_CONTRATO_API_SQL_GestionLudopatas.md` §12) y se adopta **API Key propia + IP allowlist**. Motivo: despliegue on-premise de un solo consumidor conocido (el host UiPath) — OAuth2 exige un Identity Provider adicional que no aporta valor a ese escenario y añade operación innecesaria. El documento fuente ya deja `ApiKeyAuth` definido como alternativa; este change la promueve a esquema activo.
- Los 6 errores 500xx de despliegue/esquema (`GL-SQL-DEPLOY-*`, `GL-SQL-SCHEMA-*`) se tratan como fallas de arranque/healthcheck, no como respuestas runtime de un endpoint de negocio.
- Secretos (connection string SQL Server, API Key) SHALL obtenerse en runtime desde HashiCorp Vault (KV v2) — sin `.env` con credenciales en texto plano. Sigue el patrón ya implementado y verificado en producción por `api-sica` (`openspec/changes/archive/vault-integration/` en ese repo): mismo Vault, path dedicado propio, mismo formato de campos, cliente HTTP sin SDK, fail-fast si Vault no responde.
- Despliegue en Docker, en el **mismo ambiente on-premise** donde ya corre `api-sica` (app servers Linux `SRV VXRDEVAPP01`/`SRV VXRQAAPP01`, `docker compose`, un puerto libre por servicio). No es infraestructura nueva — se suma un contenedor más al mismo host/entorno ya operado.

## Capabilities

### New Capabilities
- `corte-resolver-inicio`: `POST /api/v1/cortes/resoluciones-inicio` — ejecuta `SP_CORTE_ResolverInicio`, resuelve inicio de corte oficial/manual.
- `corte-crear`: `POST /api/v1/cortes` — ejecuta `SP_CORTE_Crear`, inserta un corte nuevo, con idempotencia.
- `pendientes-calimaco-ingreso`: `POST /api/v1/pendientes/calimaco/ingresos/busqueda` — ejecuta `SP_Pendientes_CALIMACO_Ingreso`.
- `pendientes-calimaco-salida`: `POST /api/v1/pendientes/calimaco/salidas/busqueda` — ejecuta `SP_Pendientes_CALIMACO_Salida`.
- `pendientes-cmp-ingreso`: `POST /api/v1/pendientes/cmp/ingresos/busqueda` — ejecuta `SP_Pendientes_CMP_Ingreso`.
- `pendientes-cmp-salida`: `POST /api/v1/pendientes/cmp/salidas/busqueda` — ejecuta `SP_Pendientes_CMP_Salida`.
- `pendientes-sica-ingreso`: `POST /api/v1/pendientes/sica/ingresos/busqueda` — ejecuta `SP_Pendientes_SICA_Ingreso`.
- `pendientes-sica-salida`: `POST /api/v1/pendientes/sica/salidas/busqueda` — ejecuta `SP_Pendientes_SICA_Salida`.
- `seguridad-acceso-api`: autenticación por API Key + IP allowlist para las 8 operaciones; respuestas `401`/`403` normalizadas.
- `modelo-error-comun`: formato `problem+json` compartido, catálogo de códigos `GL-*`, precedencia de validación API↔SQL, mapeo `SqlException.Number` → código.
- `secretos-vault`: obtención de credenciales SQL Server y API Key desde HashiCorp Vault KV v2 en el arranque, fail-fast si Vault no responde o el token es inválido.
- `despliegue-docker`: imagen y `docker-compose` para desplegar `GestionLudopatas.Api` on-premise en el mismo ambiente ya operado para `api-sica`.
- `coleccion-postman`: colección Postman de los 8 endpoints con auth API Key, validada con `newman` contra el deploy real de dev antes de cerrar el change (mismo criterio que `reto_tecnico_backend_senior`).

### Modified Capabilities
(ninguna — proyecto nuevo, sin specs previas en `openspec/specs/`)

## Impact

- **Código nuevo**: solution `GestionLudopatas.Api` completa (Domain/Application/Infrastructure/Api), fuera del árbol de `reto_tecnico_backend_senior`.
- **Base de datos**: solo lectura/ejecución de los 8 SP existentes vía un login SQL con permiso `EXECUTE` acotado a esos SP (sin acceso a tablas). No se modifica DDL ni SP.
- **Consumidor**: UiPath, vía HTTPS + API Key, desde una IP/subnet fija del host on-premise.
- **Fuera de alcance** (heredado del documento fuente): transacción atómica resolver+crear, CRUD de tablas, endpoints de actualización de bitácora, cambios al SQL.
