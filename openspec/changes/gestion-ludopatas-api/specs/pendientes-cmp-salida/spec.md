## ADDED Requirements

### Requirement: Búsqueda de pendientes CMP salida
El sistema SHALL exponer `POST /api/v1/pendientes/cmp/salidas/busqueda`, que recibe `{corteIdActual: integer, maxReintentosPorSistema: integer, esReintentoForzado: boolean = false}` y ejecuta `dbo.SP_Pendientes_CMP_Salida` (solo lectura, sin `ORDER BY`, sin reserva de filas). La respuesta SHALL ser un arreglo de 0..N objetos `{id, numeroDocumento, tipoDocumento, corteId, ultimoCorteId, cmpUltimoCorteId, cmpReintentos}`. Un arreglo vacío SHALL considerarse una respuesta `200` válida, no un error.

#### Scenario: Búsqueda con resultados
- **WHEN** `corteIdActual > 0` y `maxReintentosPorSistema > 0` son válidos y existen filas elegibles
- **THEN** el sistema responde `200` con el arreglo de pendientes

#### Scenario: Búsqueda sin resultados
- **WHEN** los parámetros son válidos pero ninguna fila cumple los filtros AS-BUILT del SP
- **THEN** el sistema responde `200` con `[]`

#### Scenario: corteIdActual inválido
- **WHEN** `corteIdActual` es `null` o menor o igual a cero
- **THEN** el sistema responde `422` con `code: GL-PEND-CMP-SAL-001`

#### Scenario: maxReintentosPorSistema inválido
- **WHEN** `maxReintentosPorSistema` es `null` o menor o igual a cero
- **THEN** el sistema responde `422` con `code: GL-PEND-CMP-SAL-002`

#### Scenario: esReintentoForzado nulo explícito
- **WHEN** el cliente envía `esReintentoForzado: null` explícitamente
- **THEN** el sistema responde `422` con `code: GL-PEND-CMP-SAL-003`

#### Scenario: esReintentoForzado omitido
- **WHEN** el cliente no incluye `esReintentoForzado` en el request
- **THEN** el sistema SHALL usar `false` como valor por defecto, igual al default SQL `BIT = 0`, y filtrar por el último corte
