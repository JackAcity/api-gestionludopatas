## ADDED Requirements

### Requirement: Creación de corte
El sistema SHALL exponer `POST /api/v1/cortes`, que recibe `{tipoCorte: "oficial"|"manual", fechaHoraCorte: date-time|null, fechaHoraEjecucion: date-time}` y ejecuta `dbo.SP_CORTE_Crear`. La respuesta exitosa SHALL ser `201 Created` con `{corteId: integer}`, donde `corteId` nunca es nulo.

#### Scenario: Corte oficial válido
- **WHEN** `tipoCorte = "oficial"` y `fechaHoraCorte` y `fechaHoraEjecucion` están presentes
- **THEN** el sistema responde `201` con `corteId` no nulo

#### Scenario: Corte manual válido
- **WHEN** `tipoCorte = "manual"`, `fechaHoraCorte` es `null` (u omitido) y `fechaHoraEjecucion` está presente
- **THEN** el sistema responde `201` con `corteId` no nulo

#### Scenario: tipoCorte inválido
- **WHEN** `tipoCorte` es `null` o distinto de `"oficial"`/`"manual"`
- **THEN** el sistema responde `422` con `code: GL-CORTE-CRE-001`

#### Scenario: Oficial sin fechaHoraCorte
- **WHEN** `tipoCorte = "oficial"` y `fechaHoraCorte` es `null`
- **THEN** el sistema responde `422` con `code: GL-CORTE-CRE-002`

#### Scenario: Manual con fechaHoraCorte presente
- **WHEN** `tipoCorte = "manual"` y `fechaHoraCorte` no es `null`
- **THEN** el sistema responde `422` con `code: GL-CORTE-CRE-003`

#### Scenario: fechaHoraEjecucion ausente
- **WHEN** `fechaHoraEjecucion` es `null` u omitida
- **THEN** el sistema responde `422` con `code: GL-CORTE-CRE-004`

### Requirement: Idempotencia de creación de corte
`SP_CORTE_Crear` no tiene clave natural ni soporte de idempotencia propio, por lo que la garantía SHALL implementarse en la API. Toda solicitud a `POST /api/v1/cortes` SHALL incluir el header `Idempotency-Key` (16 a 128 caracteres). El sistema SHALL persistir, junto al resultado de la primera ejecución exitosa, un fingerprint del payload asociado a esa clave, con vigencia de 24 horas.

#### Scenario: Repetición con misma clave y mismo payload
- **WHEN** llega un `POST /api/v1/cortes` con un `Idempotency-Key` ya usado y el mismo payload que la solicitud original
- **THEN** el sistema responde con el mismo `status` y cuerpo de la respuesta original, sin ejecutar `SP_CORTE_Crear` de nuevo, e incluye el header `Idempotency-Replayed: true`

#### Scenario: Repetición con misma clave y payload distinto
- **WHEN** llega un `POST /api/v1/cortes` con un `Idempotency-Key` ya usado pero un payload distinto al original
- **THEN** el sistema responde `409` con `code: GL-IDEMP-001` y no ejecuta el SP

#### Scenario: Idempotency-Key ausente o fuera de rango
- **WHEN** el header `Idempotency-Key` falta, o su longitud no está entre 16 y 128 caracteres
- **THEN** el sistema responde `422` con `code: GL-API-REQ-002`
