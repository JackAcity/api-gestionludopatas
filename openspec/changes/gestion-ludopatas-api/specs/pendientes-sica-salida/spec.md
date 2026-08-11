## ADDED Requirements

### Requirement: Búsqueda de pendientes SICA salida
El sistema SHALL exponer `POST /api/v1/pendientes/sica/salidas/busqueda`, que recibe `{maxReintentosPorSistema: integer}` (sin `corteIdActual` ni `esReintentoForzado` — SICA no los usa) y ejecuta `dbo.SP_Pendientes_SICA_Salida` (solo lectura, sin `ORDER BY`, sin reserva de filas). La respuesta SHALL ser un arreglo de 0..N objetos `{id, numeroDocumento, tipoDocumento, nombresApellidos, fechaInscripcion, corteId, ultimoCorteId, sicaReintentos}`. A diferencia de SICA ingreso, en salida `nombresApellidos` y `fechaInscripcion` SHALL poder ser `null` — se incluyen como propiedades del ResultSet pero el SP no filtra su nulidad. Un arreglo vacío SHALL considerarse una respuesta `200` válida, no un error.

#### Scenario: Búsqueda con resultados
- **WHEN** `maxReintentosPorSistema > 0` es válido y existen filas elegibles
- **THEN** el sistema responde `200` con el arreglo de pendientes, permitiendo `nombresApellidos` y `fechaInscripcion` nulos

#### Scenario: Búsqueda sin resultados
- **WHEN** el parámetro es válido pero ninguna fila cumple los filtros AS-BUILT del SP
- **THEN** el sistema responde `200` con `[]`

#### Scenario: maxReintentosPorSistema inválido
- **WHEN** `maxReintentosPorSistema` es `null` o menor o igual a cero
- **THEN** el sistema responde `422` con `code: GL-PEND-SICA-SAL-001`
