## ADDED Requirements

### Requirement: Obtención de secretos desde Vault en el arranque, en paths separados
El sistema SHALL obtener la connection string de SQL Server y la API Key de entrada (D2) desde HashiCorp Vault KV v2, mediante llamadas HTTP directas (`GET {VAULT_ADDRESS}/v1/secret/data/{path}` con header `X-Vault-Token`), sin SDK de Vault. **Ambos secretos SHALL vivir en paths distintos** (single responsibility — tienen dueño y ciclo de rotación distintos: el DBA rota la connection string, el equipo de la API rota la API Key): un path para `engine=sqlserver`/`host`/`port`/`dbname`/`username`/`password`, otro path para `api_key`. El sistema NUNCA SHALL leer estas credenciales de `appsettings.json` versionado ni de un `.env` con valores en texto plano. `VAULT_TOKEN` y `VAULT_ADDRESS` SHALL ser compartidos entre ambas llamadas; cada una usa su propio path (`VAULT_PATH_DB`, `VAULT_PATH_APIKEY`).

#### Scenario: Arranque exitoso
- **WHEN** `VAULT_TOKEN`, `VAULT_ADDRESS`, `VAULT_PATH_DB` y `VAULT_PATH_APIKEY` están presentes y Vault responde `200` en ambos paths con los campos esperados
- **THEN** el sistema resuelve la connection string (desde el path de BD) y la API Key (desde el path de API Key) antes de aceptar tráfico, y arranca normalmente

#### Scenario: Variable de arranque faltante
- **WHEN** falta `VAULT_TOKEN`, `VAULT_ADDRESS`, `VAULT_PATH_DB` o `VAULT_PATH_APIKEY`
- **THEN** el sistema no arranca; falla explícito antes de intentar cualquier llamada a Vault

#### Scenario: Vault rechaza el token en cualquiera de los dos paths
- **WHEN** Vault responde `403` a la solicitud de lectura de cualquiera de los dos secretos
- **THEN** el sistema no arranca y termina con código de salida distinto de cero

#### Scenario: Path de Vault inexistente
- **WHEN** Vault responde `404` para `VAULT_PATH_DB` o `VAULT_PATH_APIKEY`
- **THEN** el sistema no arranca y termina con código de salida distinto de cero

#### Scenario: Vault inaccesible por red
- **WHEN** la llamada HTTP a Vault falla por timeout o error de red, en cualquiera de los dos paths
- **THEN** el sistema no arranca; el error se propaga sin silenciarse (fail-fast, D8)

#### Scenario: Respuesta de Vault con campo faltante
- **WHEN** Vault responde `200` en el path de BD pero falta alguno de `engine`/`host`/`port`/`dbname`/`username`/`password`, `engine` no es `sqlserver`, o el path de API Key responde `200` sin el campo `api_key`
- **THEN** el sistema no arranca y reporta qué campo falta y en qué path

### Requirement: Cifrado de SQL Server y excepción transitoria de certificado DEV
La connection string construida desde Vault SHALL usar `Encrypt=True`. El valor no secreto
`SqlServer:TrustServerCertificate` SHALL ser `false` por defecto, de modo que el
certificado del servidor se valide. Puede configurarse explícitamente como `true` solo
en DEV mientras el certificado de SQL Server sea autofirmado o no coincida con el host;
esa excepción preserva cifrado, pero no autenticación del servidor, y SHALL retirarse
cuando el DBA entregue una CA y nombre de certificado válidos. Este ajuste no pertenece
al secreto de Vault ni cambia sus seis campos.

#### Scenario: Certificado SQL válido
- **WHEN** `SqlServer:TrustServerCertificate=false` y SQL Server presenta una cadena y nombre válidos
- **THEN** la conexión usa TLS cifrado y valida el certificado del servidor

#### Scenario: Certificado DEV autofirmado documentado
- **WHEN** DEV mantiene el certificado autofirmado/no coincidente y el despliegue configura explícitamente `SqlServer:TrustServerCertificate=true`
- **THEN** la conexión sigue usando `Encrypt=True`, arranca contra el servidor DEV y la excepción queda documentada para su retiro

### Requirement: Path de Vault dedicado, no compartido, y sin segmento de ambiente
Los paths de Vault de `GestionLudopatas.Api` SHALL ser propios por servicio (prefijo `api-` por convención del equipo: backend puro, no un sistema completo), siguiendo el mismo mount (`secret`, KV v2) usado por `api-sica`, pero SHALL NOT compartir path con `api-sica`, con otro servicio, ni entre sí (el path de BD y el de API Key son dos paths distintos) — cada uno de mínimo privilegio. **Ningún path SHALL incluir un segmento de ambiente** (`api-gestionludopatas/db`, `api-gestionludopatas/apikey`) — decisión explícita del usuario para replicar la estructura que tendrá producción: `dev` y `qa` comparten el mismo Vault (`https://dev-app-vault.acity.com.pe`) y el mismo path, sobreescribiendo el valor al pasar de un ambiente al otro (KV v2 versiona automáticamente); el Vault de producción es una instancia separada con los mismos paths sin segmento. Esto difiere deliberadamente del patrón de `api-sica`, que sí separa `/dev/`/`/qa/` dentro de un mismo Vault — no es un error, es una decisión propia de este proyecto.

#### Scenario: Mismo path en dev y qa, versionado por sobreescritura
- **WHEN** el servicio arranca en `dev` y luego, en otro momento, en `qa`
- **THEN** `VAULT_PATH_DB` y `VAULT_PATH_APIKEY` son idénticos en ambos casos; el valor vigente es el que se haya escrito más recientemente en Vault (versión más nueva de ese path), no una versión fijada por ambiente

#### Scenario: Mismos paths en producción, instancia de Vault distinta
- **WHEN** el servicio arranca en `prod`
- **THEN** `VAULT_ADDRESS` apunta al Vault de producción (instancia separada) y `VAULT_PATH_DB`/`VAULT_PATH_APIKEY` son los mismos strings de path que en dev/qa (`api-gestionludopatas/db`, `api-gestionludopatas/apikey`) — lo que cambia es la instancia de Vault, no el path

### Requirement: TLS sin bypass de validación de certificado
El sistema SHALL validar el certificado TLS del endpoint de Vault. El sistema NUNCA SHALL deshabilitar la validación de certificado (equivalente a `NODE_TLS_REJECT_UNAUTHORIZED=0` en el precedente Node) para resolver un problema de CA. Si Vault usa una CA interna, el sistema SHALL confiar en ella explícitamente vía configuración de confianza de certificado, no vía un bypass global.

#### Scenario: CA interna configurada
- **WHEN** el certificado de Vault es emitido por una CA interna ya provista por DevOps
- **THEN** el sistema confía en esa CA específica y la conexión TLS se valida normalmente

#### Scenario: Certificado no confiable sin configuración
- **WHEN** el certificado de Vault no es confiable y no se configuró la CA interna
- **THEN** la conexión falla (fail-fast) — el sistema no degrada a "aceptar cualquier certificado"
