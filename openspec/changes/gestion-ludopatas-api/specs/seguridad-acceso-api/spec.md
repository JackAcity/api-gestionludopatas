## ADDED Requirements

### Requirement: Autenticación por API Key
Las 8 operaciones SHALL requerir un header `X-Api-Key` con una clave válida, comparada mediante un algoritmo de tiempo constante para evitar ataques de temporización. Esta decisión reemplaza deliberadamente la propuesta de OAuth2 Client Credentials del documento fuente (`endpoint/PROPUESTA_CONTRATO_API_SQL_GestionLudopatas.md` §12): el despliegue es on-premise con un único consumidor conocido (UiPath), y un Identity Provider adicional no aporta valor a ese escenario.

#### Scenario: API Key ausente
- **WHEN** una solicitud a cualquiera de los 8 endpoints no incluye el header `X-Api-Key`
- **THEN** el sistema responde `401` con `code: GL-AUTH-001`, sin ejecutar el Stored Procedure

#### Scenario: API Key inválida
- **WHEN** el header `X-Api-Key` está presente pero no coincide con ninguna clave activa
- **THEN** el sistema responde `401` con `code: GL-AUTH-001`, sin ejecutar el Stored Procedure

#### Scenario: API Key válida
- **WHEN** el header `X-Api-Key` coincide con una clave activa
- **THEN** el sistema continúa con la validación de IP allowlist

### Requirement: Restricción por IP allowlist
El sistema SHALL mantener una lista configurable de IPs/subredes autorizadas (el host UiPath) y SHALL rechazar cualquier solicitud cuya IP de origen no esté en esa lista, incluso si la API Key es válida.

#### Scenario: IP fuera de la allowlist
- **WHEN** una solicitud con API Key válida llega desde una IP que no está en la allowlist configurada
- **THEN** el sistema responde `403` con `code: GL-AUTH-002`, sin ejecutar el Stored Procedure

#### Scenario: IP dentro de la allowlist
- **WHEN** una solicitud con API Key válida llega desde una IP incluida en la allowlist
- **THEN** el sistema procede a ejecutar la operación solicitada
