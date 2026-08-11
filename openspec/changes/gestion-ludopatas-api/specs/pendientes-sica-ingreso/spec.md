## ADDED Requirements

### Requirement: Búsqueda de pendientes SICA ingreso
El sistema SHALL exponer `POST /api/v1/pendientes/sica/ingresos/busqueda`, que recibe `{maxReintentosPorSistema: integer}` (sin `corteIdActual` ni `esReintentoForzado` — SICA no los usa) y ejecuta `dbo.SP_Pendientes_SICA_Ingreso` (solo lectura, sin `ORDER BY`, sin reserva de filas). La respuesta SHALL ser un arreglo de 0..N objetos `{id, numeroDocumento, tipoDocumento, nombresApellidos, fechaInscripcion, corteId, ultimoCorteId, sicaReintentos}`. `nombresApellidos` SHALL ser obligatorio y no nulo en cada item porque el SP filtra `nombres_apellidos IS NOT NULL`; `fechaInscripcion` puede ser `null`. Un arreglo vacío SHALL considerarse una respuesta `200` válida, no un error.

#### Scenario: Búsqueda con resultados
- **WHEN** `maxReintentosPorSistema > 0` es válido y existen filas elegibles
- **THEN** el sistema responde `200` con el arreglo de pendientes, cada item con `nombresApellidos` no nulo

#### Scenario: Búsqueda sin resultados
- **WHEN** el parámetro es válido pero ninguna fila cumple los filtros AS-BUILT del SP
- **THEN** el sistema responde `200` con `[]`

#### Scenario: maxReintentosPorSistema inválido
- **WHEN** `maxReintentosPorSistema` es `null` o menor o igual a cero
- **THEN** el sistema responde `422` con `code: GL-PEND-SICA-ING-001`
