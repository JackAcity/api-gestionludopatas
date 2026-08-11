## ADDED Requirements

### Requirement: Formato común de error
Toda respuesta de error de los 8 endpoints SHALL usar `Content-Type: application/problem+json` con los campos obligatorios `type` (URI), `title`, `status` (400..599), `code` (`GL-*`), `detail` (sin stack trace, SQL ni secretos), `traceId`, `timestamp` (RFC 3339) y `retryable` (boolean), más `source` (`api`|`sql`). El campo opcional `sqlErrorNumber` SHALL incluirse únicamente en errores controlados (510xx/511xx); el campo opcional `violations` SHALL usarse para detalle por campo en `400`/`422`.

#### Scenario: Error funcional expone código estable
- **WHEN** el sistema devuelve cualquier error `GL-*`
- **THEN** el cuerpo incluye `code`, `status`, `detail`, `traceId`, `timestamp`, `retryable` y `source`, y el mensaje en `detail` no contiene información sensible (cadena de conexión, SQL, stack trace)

#### Scenario: Toda respuesta incluye trazabilidad
- **WHEN** el sistema responde a cualquier solicitud (éxito o error)
- **THEN** la respuesta incluye el header `X-Trace-Id`; si el request trajo `X-Correlation-Id`, el sistema lo propaga sin alterarlo

### Requirement: Precedencia de validación API↔SQL
Cuando una condición puede detectarse tanto en la API como en el Stored Procedure, el sistema SHALL aplicar, en orden: (1) JSON ilegible o tipo incompatible → `400 GL-API-REQ-001`; (2) regla específica de la operación que replica un `THROW` SQL conocido → el código `GL-CORTE-*`/`GL-PEND-*` correspondiente, aunque la API la detecte antes de invocar el SP; (3) regla de contrato sin código funcional específico → `422 GL-API-REQ-002` como fallback; (4) si el SP devuelve el error controlado, el sistema SHALL aplicar exactamente el mismo código funcional que hubiera aplicado la prevalidación.

#### Scenario: Prevalidación replica un THROW SQL conocido
- **WHEN** la API detecta antes de invocar el SP una condición que corresponde a uno de los 23 `THROW` runtime catalogados en `endpoint/MATRIZ_MAPEO_ERRORES_SQL_HTTP.md`
- **THEN** el sistema responde con el mismo código `GL-*` que devolvería el SP para esa condición, con `source: "api"`

#### Scenario: Regla de contrato sin código específico
- **WHEN** el request incumple una regla del contrato que no tiene código funcional específico asignado
- **THEN** el sistema responde `422` con `code: GL-API-REQ-002`

### Requirement: Mapeo data-driven de errores SQL
El sistema SHALL mantener una única tabla de mapeo `número de error SQL → (status HTTP, code GL-*, mensaje público, retryable)`, consultada por un componente compartido, en vez de duplicar la lógica de clasificación en cada handler de endpoint. Los 6 códigos `50000–50012` (despliegue/esquema) SHALL tratarse como señal de healthcheck/arranque y NUNCA SHALL devolverse como respuesta runtime de un endpoint de negocio.

#### Scenario: Error SQL runtime mapeado
- **WHEN** un Stored Procedure lanza un `THROW` con un número de error catalogado en 510xx/511xx
- **THEN** el sistema traduce ese número al `code` y `status` HTTP correspondientes usando la tabla de mapeo, sin lógica condicional específica por endpoint

#### Scenario: Error de despliegue no se expone como respuesta runtime
- **WHEN** el esquema de `bd_autobot` no es compatible con lo que la API espera (errores `50000–50012`)
- **THEN** el sistema lo detecta en arranque o healthcheck, marca el servicio como no saludable, y ningún endpoint de negocio devuelve esos códigos como respuesta a una solicitud
