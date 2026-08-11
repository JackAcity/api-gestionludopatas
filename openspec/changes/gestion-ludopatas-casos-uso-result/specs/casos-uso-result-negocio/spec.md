## ADDED Requirements

### Requirement: Un fallo de negocio esperado se representa como `Result<T>` de fallo, nunca se lanza como excepción
Cuando una regla de negocio de una de las 8 operaciones de `GestionLudopatas.Api` no se cumple (validación de campo, condición del contrato SQL replicada en la API), el caso de uso (`Manejador*`) SHALL devolver un `Result<T>` de fallo con `Status/Codigo/Detalle/Reintentable/Origen` correspondientes al código `GL-*` de la regla, y SHALL NOT lanzar una excepción para este camino.

#### Scenario: `tipoCorte` inválido en crear corte
- **WHEN** se invoca `ManejadorCrearCorte` con `TipoCorte` distinto de `"oficial"` u `"manual"`
- **THEN** el resultado es `Result<CrearCorteResponse>` de fallo con `Codigo = "GL-CORTE-CRE-001"` y `Status = 422`
- **AND** no se lanza ninguna excepción

#### Scenario: `corteIdActual` inválido en búsqueda de pendientes CALIMACO ingreso
- **WHEN** se invoca `ManejadorBuscarPendientesCalimacoIngreso` con `CorteIdActual <= 0` o `null`
- **THEN** el resultado es `Result<IReadOnlyList<PendienteCalimacoItem>>` de fallo con `Codigo = "GL-PEND-CAL-ING-001"` y `Status = 422`
- **AND** no se lanza ninguna excepción

### Requirement: El adaptador SQL nunca se invoca cuando la validación de negocio falla
El caso de uso SHALL validar antes de invocar el puerto de infraestructura (`ICorteCreator`, `ICorteResolver`, `IBuscarPendientes<TRequest,TItem>`); si la validación falla, el puerto SHALL NOT ser invocado.

#### Scenario: Validación corta antes de abrir conexión SQL
- **WHEN** se invoca cualquier `Manejador*` con datos que violan una regla de validación de su spec
- **THEN** el adaptador SQL subyacente (verificado con una fábrica de conexión canario que lanza si se invoca) nunca es invocado

### Requirement: La validación de reglas de negocio vive en el caso de uso, no en el adaptador SQL
Los adaptadores en `Infrastructure/Sql/*` (`CorteCreatorSql`, `CorteResolverSql`, `CalimacoCmpBuscadorSqlBase` y sus implementaciones, `SicaBuscadorSqlBase` y sus implementaciones) SHALL limitarse a ejecutar el Stored Procedure y mapear el resultado, y SHALL NOT contener lógica de validación de reglas de negocio.

#### Scenario: Adaptador SQL recibe una solicitud ya validada
- **WHEN** el caso de uso invoca el puerto de infraestructura
- **THEN** el adaptador ejecuta el Stored Procedure directamente, sin repetir ninguna validación de campo ya evaluada por el caso de uso

### Requirement: El contrato HTTP observable no cambia respecto al mecanismo anterior
Para cada regla de negocio, la respuesta HTTP (status, `code`, `detail`, `retryable`, `source` del `ProblemaDetalle`) SHALL ser idéntica a la que producía la versión anterior basada en `ErrorFuncionalException`.

#### Scenario: Respuesta 422 de `tipoCorte` inválido antes y después del refactor
- **WHEN** un cliente envía `POST /api/v1/cortes` con `tipoCorte` inválido
- **THEN** la respuesta es `422` con cuerpo `application/problem+json`, `code = "GL-CORTE-CRE-001"`, igual que antes de introducir `Result<T>`

### Requirement: Solo una falla verdaderamente excepcional se propaga como excepción
Una falla de infraestructura (`SqlException` con número no catalogado como validación de negocio: timeout, deadlock, conflicto de datos, permisos, no disponible) o una excepción no clasificada (bug, invariante rota) SHALL seguir propagándose como excepción real, capturada por `ManejadorExcepcionesGlobal`.

#### Scenario: Timeout de SQL Server sigue siendo excepción
- **WHEN** `SqlConnectionFactory` o un comando SQL lanza `SqlException` con número `-2` (timeout) durante la ejecución de un Stored Procedure
- **THEN** la excepción se propaga sin ser convertida a `Result<T>` en el caso de uso
- **AND** `ManejadorExcepcionesGlobal` la traduce a `504` con `Codigo = "GL-SQL-TIMEOUT-001"`, igual que antes de este change
