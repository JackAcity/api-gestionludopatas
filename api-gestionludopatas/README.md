# GestionLudopatas.Api

Backend puente entre UiPath y `bd_autobot` (SQL Server 2019). Expone 8 Stored Procedures
como HTTP para que el bot nunca se conecte directo a la base — ver
[`openspec/changes/gestion-ludopatas-api/`](../openspec/changes/gestion-ludopatas-api/)
(proposal, design, specs, tasks) para el detalle completo de decisiones y contrato.

Contrato fuente de los 8 SP: [`endpoint/`](../endpoint/) en la raíz de `project_autobot`
(`PROPUESTA_CONTRATO_API_SQL_GestionLudopatas.md`, `MATRIZ_MAPEO_ERRORES_SQL_HTTP.md`).

## Estado actual

Implementado localmente, `dotnet test` en verde (131/131), `dotnet build -warnaserror`
sin warnings ni errores. La Fase 1 fue **desplegada y verificada en dev real**
(2026-08-08): `SRV VXRDEVAPP01` (`10.99.200.100:32451`), mismo server que `api-sica`,
directorio propio `/acity/gestionludopatas/api-gestionludopatas/`, contenedor
`gestion_ludopatas_api` en el puerto `9012`. El 2026-08-10 se desplegaron los refuerzos
Result, contrato JSON, idempotencia y DIP: Newman ejecutó las 8 prevalidaciones en DEV
con 8/8 assertions verdes y la comparación `curl` antes/después preservó los 8 pares
`422` + `GL-*`. El contrato de errores quedó además corregido a
`application/problem+json`.

La Fase 2 (Vault) fue **desplegada y verificada en DEV el 2026-08-11**: ambos paths de
Vault devolvieron `200`, la API arrancó con `/health` en `200`, y volvió a arrancar en
`200` después de retirar `DB_CONNECTION_STRING` y `API_KEY` del `.env` remoto. Una
operación autenticada con la API Key recuperada de Vault únicamente en memoria devolvió
el contrato esperado `422 GL-CORTE-RES-001`. Las búsquedas SICA de solo lectura también
fueron verificadas funcionalmente con `200 []` en DEV el 2026-08-11. DEV mantiene transitoriamente
`SQLSERVER_TRUST_SERVER_CERTIFICATE=true` porque SQL Server presenta un certificado
autofirmado/no coincidente; el tráfico queda cifrado, pero el certificado no se autentica
hasta que el DBA provea una CA y nombre válidos.

**Bug real encontrado y corregido durante esta verificación**: el manejador de errores
filtraba `sqlErrorNumber` en el fallback no catalogado (ej. `18456` login failed) —
la spec dice que ese campo es solo para errores catalogados (510xx/511xx). Corregido y
reverificado en vivo.

**Falta para cerrar el change** (`openspec/changes/gestion-ludopatas-api/tasks.md`):
reemplazar el certificado SQL Server DEV por uno válido y volver a
`SQLSERVER_TRUST_SERVER_CERTIFICATE=false`,
persistencia compartida de idempotencia antes de escalar a más de una instancia.

## Referencia API en DEV

En `Development`, la referencia interactiva está disponible en
`http://10.99.200.100:9012/docs` y el documento en
`http://10.99.200.100:9012/openapi/v1.json`. Ambos omiten la API key solo para que el
navegador pueda cargar la documentación, pero siguen detrás de la allowlist de IP; en
QA y producción no se mapean. Las operaciones descritas declaran `X-Api-Key`: ingrésela
manualmente en Scalar para probarlas. La UI no persiste la clave y tiene Agent/fuentes
externas deshabilitados.

## Cómo correr local

```bash
dotnet build
dotnet test
```

Para levantarlo necesitás una connection string real a `bd_autobot` (o una copia con los
8 SP y las 2 tablas — ver Provisionamiento SQL abajo) y una API Key propia:

```bash
export ConnectionStrings__BdAutobot="Server=...;Database=bd_autobot;User Id=...;Password=...;Encrypt=True;"
export Seguridad__ApiKey="una-clave-larga-propia"
export Seguridad__IpsPermitidas__0="127.0.0.1"
dotnet run --project src/GestionLudopatas.Api
```

`GET /health` no requiere API Key (lo pega el healthcheck de Docker/infra sin credenciales).

## Provisionamiento SQL (D5 — mínimo privilegio)

El login de la API **solo** necesita `EXECUTE` en los 8 SP — nunca acceso directo a
`Corte`/`bitacora_transacciones`:

```sql
USE bd_autobot;
CREATE LOGIN app_gestion_ludopatas WITH PASSWORD = '<definir>';
CREATE USER app_gestion_ludopatas FOR LOGIN app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_CORTE_ResolverInicio          TO app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_CORTE_Crear                    TO app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_Pendientes_CALIMACO_Ingreso    TO app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_Pendientes_CALIMACO_Salida     TO app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_Pendientes_CMP_Ingreso         TO app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_Pendientes_CMP_Salida          TO app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_Pendientes_SICA_Ingreso        TO app_gestion_ludopatas;
GRANT EXECUTE ON dbo.SP_Pendientes_SICA_Salida         TO app_gestion_ludopatas;
```

`EsquemaHealthCheck` (`/health`) verifica que los 8 SP y ambas tablas existan antes de
aceptar tráfico — si falta algo, el healthcheck reporta no saludable en vez de fallar en
medio de una solicitud de negocio (D6).

### Idempotencia persistente — siguiente etapa DBA

La idempotencia actual está protegida contra duplicados simultáneos en una instancia,
pero aún es memoria local. El paquete para hacerla durable sin otorgar DML directo sobre
`bd_autobot` está en [database/idempotencia-persistente](database/idempotencia-persistente):
debe revisarlo y ejecutarlo el DBA en una base propia de la API, ubicada en la misma
instancia SQL Server. No ejecutar el script desde el contenedor ni desde la API.

El documento de lectura inicial para DBA, arquitectura, QA y quien apruebe el pase es
[PASE_DBA_Y_ARQUITECTURA.md](database/idempotencia-persistente/PASE_DBA_Y_ARQUITECTURA.md):
explica el problema, las alternativas descartadas, cada paso/permiso/transacción, sus
riesgos, pruebas, rollback y responsabilidades.

El preflight DEV confirmó que el login actual tiene `EXECUTE` sobre `SP_CORTE_Crear`,
pero no `CREATE TABLE` ni `VIEW DEFINITION`, como exige D5. Ver
[VALIDACION_DBA.md](database/idempotencia-persistente/VALIDACION_DBA.md) antes de
cambiar la implementación .NET.

## Despliegue on-premise (dos fases, spec `despliegue-docker` / `secretos-vault`)

**Fase 1 — dev con config plana** (`Dockerfile`, `docker-compose.yml`): mismo app server
Linux donde ya corre `api-sica`, contenedor y puerto propios (`9012`, confirmado libre
contra `ss -tlnp` real del server DEV01). Secretos como variables de entorno normales,
`Vault__Habilitado=false`.

**Fase 2 — cutover a Vault** (D8): mismo Vault que ya usa `api-sica`
(`openspec/changes/archive/vault-integration/` en ese repo tiene el runbook completo).
**Dos paths separados, no uno** (single responsibility — la connection string la rota el
DBA, la API Key la rotamos nosotros, no comparten ciclo de vida): prefijo `api-` porque
es un backend, no un sistema completo (`app_` queda para eso, ej. `app_reclamos`) —
ninguno de los dos se comparte con `api-sica` ni entre sí.

**Sin segmento de ambiente en el path, ni en dev/qa** (a propósito — se busca replicar
la estructura que tendrá producción, donde no hace falta desambiguar). `dev` y `qa`
comparten Vault y comparten path; pasar de uno a otro es sobreescribir el valor (KV v2
versiona, no se pierde el anterior). Esto es distinto del patrón de `api-sica` (que sí
separa `/dev/`/`/qa/`) — decisión propia de este proyecto.

| Ambiente | `Vault__Address` | `Vault__PathDb` | `Vault__PathApiKey` |
|---|---|---|---|
| dev | `https://dev-app-vault.acity.com.pe` | `api-gestionludopatas/db` | `api-gestionludopatas/apikey` |
| qa | `https://dev-app-vault.acity.com.pe` (mismo Vault y mismo path que dev — sobreescribir valor) | `api-gestionludopatas/db` | `api-gestionludopatas/apikey` |
| prod | Vault de producción (instancia separada) | `api-gestionludopatas/db` | `api-gestionludopatas/apikey` |

Campos esperados en Vault: path `db` → `engine` (`sqlserver`), `host`, `port`, `dbname`, `username`, `password`;
path `apikey` → solo `api_key`. Activar con `Vault__Habilitado=true` +
`Vault__Address`/`Vault__Token`/`Vault__PathDb`/`Vault__PathApiKey` (y
`Vault__RutaCaInterna` si Vault usa una CA interna — nunca se deshabilita validación TLS).
En el `.env` remoto, `docker-compose.yml` recibe esas opciones como
`VAULT_HABILITADO`/`VAULT_ADDRESS`/`VAULT_TOKEN`/`VAULT_PATH_DB`/
`VAULT_PATH_APIKEY` (y opcionalmente `VAULT_CA_FILE`); usar
[.env.example](.env.example) como plantilla. Una vez comprobado el arranque Fase 2,
eliminar `DB_CONNECTION_STRING` y `API_KEY` del `.env` remoto: no deben quedar como
fallback de secretos.

La conexión a SQL Server siempre usa `Encrypt=True`. Por defecto
`SQLSERVER_TRUST_SERVER_CERTIFICATE=false`; el valor `true` solo reproduce de forma
temporal la configuración DEV existente mientras el DBA mantiene un certificado
autofirmado/no coincidente. Conserva cifrado, pero no autentica el certificado del
servidor: volver a `false` al instalar una CA y un nombre de certificado válidos.

## Rotar la API Key / actualizar la allowlist

1. Generar una clave nueva (larga, aleatoria).
2. Fase 1: actualizar `Seguridad__ApiKey` en el `.env`/compose y reiniciar el contenedor.
   Fase 2 (Vault): actualizar el campo `api_key` en el path de Vault y redesplegar
   (`docker compose up -d --force-recreate` — re-lee Vault al arrancar, mismo patrón que
   `api-sica`).
3. En Docker, conservar `UIPATH_IP_ALLOWLIST` para UiPath y usar
   `OPERADOR_IP_ALLOWLIST` para una segunda IP/CIDR exacta del operador autorizado.
   Ambas se enlazan a `Seguridad__IpsPermitidas` y se evalúan como entradas independientes;
   no reemplazar ni concatenar la IP de UiPath. Antes de habilitar un ambiente nuevo,
   confirmar la IP de salida real del cliente.
4. Avisar al equipo de UiPath del cambio — no hay periodo de gracia con dos claves
   válidas simultáneas en esta versión.

## Trazabilidad endpoint ↔ SP ↔ spec

| Endpoint | Stored Procedure | Spec |
|---|---|---|
| `POST /api/v1/cortes/resoluciones-inicio` | `SP_CORTE_ResolverInicio` | `corte-resolver-inicio` |
| `POST /api/v1/cortes` | `SP_CORTE_Crear` | `corte-crear` |
| `POST /api/v1/pendientes/calimaco/ingresos/busqueda` | `SP_Pendientes_CALIMACO_Ingreso` | `pendientes-calimaco-ingreso` |
| `POST /api/v1/pendientes/calimaco/salidas/busqueda` | `SP_Pendientes_CALIMACO_Salida` | `pendientes-calimaco-salida` |
| `POST /api/v1/pendientes/cmp/ingresos/busqueda` | `SP_Pendientes_CMP_Ingreso` | `pendientes-cmp-ingreso` |
| `POST /api/v1/pendientes/cmp/salidas/busqueda` | `SP_Pendientes_CMP_Salida` | `pendientes-cmp-salida` |
| `POST /api/v1/pendientes/sica/ingresos/busqueda` | `SP_Pendientes_SICA_Ingreso` | `pendientes-sica-ingreso` |
| `POST /api/v1/pendientes/sica/salidas/busqueda` | `SP_Pendientes_SICA_Salida` | `pendientes-sica-salida` |

Transversales: `seguridad-acceso-api`, `modelo-error-comun`, `secretos-vault`, `despliegue-docker`, `coleccion-postman`.

## Postman

Hay dos colecciones, ambas con auth `X-Api-Key` y el environment
`postman/GestionLudopatas.postman_environment.json` (plantilla):

- `GestionLudopatas.postman_collection.json`: 8 operaciones exitosas y sus ejemplos.
  No se automatiza aún porque crea datos y depende del login real de `bd_autobot`.
- `GestionLudopatas.contract.postman_collection.json`: 8 reglas negativas, una por
  operación. Es segura, no toca SQL y contiene asserts Newman de status, código,
  Problem Details y trazabilidad. Fue ejecutada nuevamente en DEV el 2026-08-11: 8/8 verde.
  Desde una red externa no permitida el resultado esperado es `403 GL-AUTH-002`;
  ejecútala desde un origen incluido en la allowlist (`UIPATH_IP_ALLOWLIST` u
  `OPERADOR_IP_ALLOWLIST`).

Ver [postman/README.md](postman/README.md) para ejecutar la suite de contrato en DEV.

## Decisiones que se apartan del documento fuente (`endpoint/`)

- **Seguridad**: API Key + IP allowlist en vez de OAuth2 Client Credentials — un solo
  consumidor conocido on-premise no justifica un Identity Provider (Decisión D2, `design.md`).
- **Idempotencia**: almacén en memoria, no una tabla SQL persistida — se evaluó
  `Microsoft.Data.Sqlite` y se descartó por una vulnerabilidad alta conocida sin fix
  limpio en su dependencia transitiva (`SQLitePCLRaw.lib.e_sqlite3`, GHSA-2m69-gcr7-jv3q).
  Riesgo aceptado: un reinicio del proceso pierde el registro dentro de la ventana de 24h
  — ver comentario `ponytail:` en `Infrastructure/Idempotencia/IdempotencyStore.cs`.
- **Documentación OpenAPI/Scalar**: se habilitó exclusivamente en Development después de
  actualizar a `Microsoft.AspNetCore.OpenApi` 10.0.11, que incorpora la versión corregida
  de `Microsoft.OpenApi`. Permanece protegida por allowlist; fuera de Development no se
  mapea. Scalar no persiste la API Key ni habilita Agent/default fonts.
