## ADDED Requirements

### Requirement: Resolución de inicio de corte
El sistema SHALL exponer `POST /api/v1/cortes/resoluciones-inicio`, que recibe `{fechaHoraEvaluacion: date-time, timeoutMinutos: integer}` y ejecuta `dbo.SP_CORTE_ResolverInicio` dentro de una transacción con `UPDLOCK, HOLDLOCK`. La respuesta SHALL ser exactamente un objeto `{accion: string, corteId: integer|null, corteColgadoOficialId: integer|null, corteColgadoManualId: integer|null}`, donde `accion` nunca es nula. La API no SHALL alterar las reglas de frontera que ya aplica el SP.

#### Scenario: Resolución válida
- **WHEN** el cliente envía `fechaHoraEvaluacion` y `timeoutMinutos >= 0` válidos
- **THEN** el sistema responde `200` con un objeto que incluye `accion` no nula y los tres identificadores según lo que devuelva el SP

#### Scenario: fechaHoraEvaluacion ausente
- **WHEN** el request no incluye `fechaHoraEvaluacion` o es `null`
- **THEN** el sistema responde `422` con `code: GL-CORTE-RES-001`

#### Scenario: timeoutMinutos inválido
- **WHEN** `timeoutMinutos` es `null` o menor que cero
- **THEN** el sistema responde `422` con `code: GL-CORTE-RES-002`

#### Scenario: Conflicto de cortes oficiales
- **WHEN** existen dos o más cortes oficiales en estado `en_proceso` al momento de resolver
- **THEN** el sistema responde `409` con `code: GL-CORTE-RES-003`

#### Scenario: Conflicto de cortes manuales vencidos
- **WHEN** existen dos o más cortes manuales vencidos en estado `en_proceso`
- **THEN** el sistema responde `409` con `code: GL-CORTE-RES-004`

#### Scenario: Timeout fuera de rango DATETIME
- **WHEN** `timeoutMinutos` combinado con `fechaHoraEvaluacion` excede el rango representable por `DATETIME` de SQL Server al restarlo
- **THEN** el sistema responde `422` con `code: GL-CORTE-RES-005`
