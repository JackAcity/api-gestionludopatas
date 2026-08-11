# Propuesta de contrato API SQL — REF_GestionLudopatas

- **Estado:** Propuesta TO-BE corregida; no implementada ni aprobada.
- **Fuente técnica AS-BUILT:** `REF_GestionLudopatas_DDL_CreacionTablas.sql` y `REF_GestionLudopatas_DDL_StoredProcedures.sql`.
- **Apoyo documental:** `06B_documentacion_script_creacion_tablas.md` y `06C_documentacion_script_stored_procedures.md`.
- **Versión documental:** `0.2.0-propuesta`.
- **Motor/base:** SQL Server 2019 / `bd_autobot`.
- **Fecha de elaboración/corrección:** 2026-08-04.

## 1. Objetivo

Definir una propuesta HTTP/OpenAPI 3.1 para que UiPath consuma una API que ejecute los ocho Stored Procedures vigentes. El documento no implementa endpoints, no ejecuta SP y no modifica los scripts SQL.

## 2. Alcance

Incluye ocho operaciones HTTP, modelos de request/response, schemas documentales de `dbo.Corte` y `dbo.bitacora_transacciones`, seguridad propuesta, idempotencia, mapeo completo de errores controlados y técnicos, y trazabilidad endpoint ↔ SP ↔ tabla.

Fuera de alcance: CRUD completo de tablas, endpoints de actualización de bitácora, clasificación horaria adicional, cambios de SQL, transacción compuesta resolver+crear y consulta de datos de negocio.

## 3. Supuestos y distinciones

### 3.1 Confirmado AS-BUILT

- `SP_CORTE_ResolverInicio` devuelve una fila y puede actualizar cortes vencidos dentro de una transacción con `UPDLOCK, HOLDLOCK`.
- `SP_CORTE_Crear` inserta una fila y devuelve `corte_id`; no es idempotente.
- Los seis SP de pendientes solo consultan, devuelven 0..N filas, no aplican `ORDER BY` y no reservan filas.
- Resolver y crear no son una operación atómica única.
- Las columnas de estado y reintentos de la bitácora son `NULLABLE`; los filtros SQL pueden excluir valores `NULL` por lógica de tres valores.

### 3.2 Propuesto TO-BE

- Rutas, métodos, códigos HTTP, nombres JSON, autenticación, headers de trazabilidad e idempotencia.
- Serialización de `DATETIME` como RFC 3339 en HTTP. La conversión exacta de zona horaria hacia/desde SQL `DATETIME` queda pendiente.
- Errores normalizados en `application/problem+json` con un código estable de API.
- Esta corrección no modifica `06B` ni `06C`. Para el contrato, `@FechaHoraCorte` se trata como **condicional** según el script SQL y el informe de validación; los scripts continúan siendo la fuente técnica prevalente.

## 4. Alternativas de nomenclatura

### Opción A — Recursos y búsquedas explícitas (recomendada)

- `/api/v1/cortes/resoluciones-inicio`
- `/api/v1/cortes`
- `/api/v1/pendientes/{sistema}/{flujo}/busqueda`

**Ventajas:** separa cortes y pendientes, evita nombres físicos de SP, hace explícito que las consultas usan `POST` por su cuerpo y mantiene una convención uniforme.

### Opción B — Operaciones RPC orientadas a acciones

- `/api/v1/cortes:resolverInicio`
- `/api/v1/cortes:crear`
- `/api/v1/pendientes:buscar?system=...&flow=...`

**Ventajas:** compacta el número de rutas. **Riesgos:** mezcla parámetros de operaciones con firmas diferentes y reduce trazabilidad/validación específica.

### Recomendación

Adoptar la **Opción A**. Conserva una operación por SP, permite schemas y códigos de error específicos y evita un endpoint genérico con comportamiento condicional no existente en SQL.

## 5. Convenciones HTTP propuestas

- `POST` para las ocho operaciones: dos modifican estado y seis requieren un cuerpo complejo de selección.
- `200 OK` para resolver inicio y para colecciones de pendientes, incluido `[]`.
- `201 Created` para crear corte.
- `400` para JSON mal formado; `422` para validación semántica; `409` para conflicto de estado/idempotencia; `500/503/504` para fallos técnicos clasificados.
- Request/response JSON en `camelCase`; cada schema conserva `x-sql-name` y `x-sql-type` en OpenAPI.
- `X-Correlation-Id` opcional de entrada y `X-Trace-Id` obligatorio de salida.

### 5.1 Precedencia normativa de validación API ↔ SQL

1. JSON ilegible, cuerpo no deserializable o tipo JSON incompatible → `400 GL-API-REQ-001`.
2. Regla específica de una operación que replica una validación SQL → código funcional `GL-CORTE-*` o `GL-PEND-*` correspondiente, aunque la API la detecte antes de ejecutar el SP.
3. Validación de contrato sin código funcional específico → `422 GL-API-REQ-002`.
4. Si el SP devuelve el error controlado, se aplica exactamente el mismo código funcional que en la prevalidación.

La propiedad `source` puede ser `api` o `sql` según dónde se detecte la condición; el `status`, `code`, mensaje funcional y `retryable` deben ser equivalentes. Las respuestas `422` del OpenAPI incluyen ejemplos específicos por operación y reservan `GL-API-REQ-002` como fallback.

## 6. Inventario de operaciones

| Ruta propuesta | Método | operationId | Stored Procedure | Resultado |
|---|---|---|---|---|
| `/api/v1/cortes/resoluciones-inicio` | POST | `resolverInicioCorte` | `dbo.SP_CORTE_ResolverInicio` | Objeto, exactamente 1 |
| `/api/v1/cortes` | POST | `crearCorte` | `dbo.SP_CORTE_Crear` | Objeto, exactamente 1 |
| `/api/v1/pendientes/calimaco/ingresos/busqueda` | POST | `buscarPendientesCalimacoIngreso` | `dbo.SP_Pendientes_CALIMACO_Ingreso` | Arreglo, 0..N |
| `/api/v1/pendientes/calimaco/salidas/busqueda` | POST | `buscarPendientesCalimacoSalida` | `dbo.SP_Pendientes_CALIMACO_Salida` | Arreglo, 0..N |
| `/api/v1/pendientes/cmp/ingresos/busqueda` | POST | `buscarPendientesCmpIngreso` | `dbo.SP_Pendientes_CMP_Ingreso` | Arreglo, 0..N |
| `/api/v1/pendientes/cmp/salidas/busqueda` | POST | `buscarPendientesCmpSalida` | `dbo.SP_Pendientes_CMP_Salida` | Arreglo, 0..N |
| `/api/v1/pendientes/sica/ingresos/busqueda` | POST | `buscarPendientesSicaIngreso` | `dbo.SP_Pendientes_SICA_Ingreso` | Arreglo, 0..N |
| `/api/v1/pendientes/sica/salidas/busqueda` | POST | `buscarPendientesSicaSalida` | `dbo.SP_Pendientes_SICA_Salida` | Arreglo, 0..N |

## 7. Contratos de cortes

### 7.1 Resolver inicio

**Request**
```json
{"fechaHoraEvaluacion":"2026-08-04T21:00:00-05:00","timeoutMinutos":30}
```
**Response 200**
```json
{"accion":"RETOMA","corteId":125,"corteColgadoOficialId":null,"corteColgadoManualId":124}
```
Cardinalidad: exactamente una fila. Los tres identificadores son anulables; `accion` no lo es. La API no debe cambiar las reglas de frontera del SP.

### 7.2 Crear corte

**Request oficial**
```json
{"tipoCorte":"oficial","fechaHoraCorte":"2026-08-05T07:00:00-05:00","fechaHoraEjecucion":"2026-08-05T07:02:10-05:00"}
```
**Request manual**
```json
{"tipoCorte":"manual","fechaHoraCorte":null,"fechaHoraEjecucion":"2026-08-05T08:12:00-05:00"}
```
**Response 201**
```json
{"corteId":126}
```
Cardinalidad: exactamente una fila. `corteId` es obligatorio y no anulable.

## 8. Contratos de pendientes

### 8.1 CALIMACO y CMP

**Request común**
```json
{"corteIdActual":126,"maxReintentosPorSistema":3,"esReintentoForzado":false}
```
`esReintentoForzado` es opcional en HTTP con default `false`, igual al default SQL `BIT = 0`. El valor `true` solo omite el filtro del último corte.

**Response CALIMACO 200**
```json
[{"id":501,"numeroDocumento":"00123456","tipoDocumento":"DNI","corteId":120,"ultimoCorteId":125,"calimacoUltimoCorteId":124,"calimacoReintentos":1}]
```
**Response CMP 200**
```json
[{"id":502,"numeroDocumento":"CE-009981","tipoDocumento":"CARNET_EXTRANJERIA","corteId":121,"ultimoCorteId":125,"cmpUltimoCorteId":null,"cmpReintentos":0}]
```

### 8.2 SICA

**Request común**
```json
{"maxReintentosPorSistema":3}
```

**Response SICA ingreso 200 — `PendienteSicaIngresoItem`**
```json
[{"id":503,"numeroDocumento":"A1234567","tipoDocumento":"PASAPORTE","nombresApellidos":"PEREZ|GOMEZ|ANA","fechaInscripcion":"20260801","corteId":121,"ultimoCorteId":125,"sicaReintentos":1}]
```
`nombresApellidos` es una propiedad obligatoria y no anulable porque el SP filtra `nombres_apellidos IS NOT NULL`. El SQL no exige texto no vacío y permite `fecha_inscripcion = NULL`.

**Response SICA salida 200 — `PendienteSicaSalidaItem`**
```json
[{"id":504,"numeroDocumento":"00998877","tipoDocumento":"DNI","nombresApellidos":null,"fechaInscripcion":null,"corteId":122,"ultimoCorteId":126,"sicaReintentos":0}]
```
`nombresApellidos` y `fechaInscripcion` se incluyen como propiedades del ResultSet, pero sus valores pueden ser `NULL`.

### 8.3 Colección vacía

```http
HTTP/1.1 200 OK
Content-Type: application/json

[]
```
Una colección vacía significa que no hay filas elegibles bajo los filtros AS-BUILT; no es un error.

## 9. Mapeo SQL ↔ JSON

| Origen SQL | Nombre JSON | Tipo API | Uso |
|---|---|---|---|
| `@FechaHoraEvaluacion DATETIME` | `fechaHoraEvaluacion` | string `date-time` | Request resolver |
| `@TimeoutMinutos INT` | `timeoutMinutos` | `integer/int32`, 0..2147483647 | Request resolver |
| `accion VARCHAR(20)` | `accion` | string enum | Response resolver |
| `corte_id INT NULL` | `corteId` | `integer/int32` 1..2147483647 o null | Response resolver |
| `corte_colgado_oficial_id INT NULL` | `corteColgadoOficialId` | `integer/int32` 1..2147483647 o null | Response resolver |
| `corte_colgado_manual_id INT NULL` | `corteColgadoManualId` | `integer/int32` 1..2147483647 o null | Response resolver |
| `@TipoCorte VARCHAR(10)` | `tipoCorte` | string enum | Request crear |
| `@FechaHoraCorte DATETIME = NULL` | `fechaHoraCorte` | string `date-time` o null | Request crear |
| `@FechaHoraEjecucion DATETIME` | `fechaHoraEjecucion` | string `date-time` | Request crear |
| `OUTPUT corte_id INT` | `corteId` | `integer/int32`, 1..2147483647 | Response crear |
| `@corte_id_actual INT` | `corteIdActual` | `integer/int32`, 1..2147483647 | Request CALIMACO/CMP |
| `@MaxReintentosPorSistema INT` | `maxReintentosPorSistema` | `integer/int32`, 1..2147483647 | Request pendientes |
| `@in_EsReintentoForzado BIT = 0` | `esReintentoForzado` | boolean, default false | Request CALIMACO/CMP |

Los modelos completos de las tablas están en `components/schemas/Corte` y `components/schemas/BitacoraTransacciones`. Son componentes documentales y no crean endpoints CRUD. Todo valor derivado de SQL Server `INT` declara `format: int32` y límites compatibles con `-2147483648..2147483647`; identificadores y parámetros con reglas funcionales conservan mínimos más restrictivos.

## 10. Modelo común de error

```json
{
  "type": "https://errors.example.invalid/gestion-ludopatas/gl-corte-cre-002",
  "title": "Solicitud no procesable",
  "status": 422,
  "code": "GL-CORTE-CRE-002",
  "detail": "fechaHoraCorte es obligatoria para un corte oficial.",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
  "timestamp": "2026-08-04T21:00:00-05:00",
  "retryable": false,
  "source": "sql"
}
```

| Campo | Obligatorio | Nulabilidad/format | Justificación |
|---|---|---|---|
| `type` | Sí | URI, no null | Identifica de forma estable la familia del error. |
| `title` | Sí | string, no null | Resumen estable y seguro. |
| `status` | Sí | integer 400..599 | Duplica el estado HTTP para clientes y logs. |
| `code` | Sí | `GL-*`, no null | Código funcional/técnico estable para decisiones de UiPath. |
| `detail` | Sí | string, no null | Detalle seguro; no incluye stack trace, SQL ni secretos. |
| `traceId` | Sí | string, no null | Correlaciona Job UiPath, API y telemetría. |
| `timestamp` | Sí | RFC 3339 | Momento de clasificación del error. |
| `retryable` | Sí | boolean | Evita reintentos indiscriminados. |
| `source` | Sí | enum | Distingue API, SQL, seguridad o red sin revelar internals innecesarios. |
| `sqlErrorNumber` | No | `integer/int32` positivo o null | Solo para errores controlados o seguros; puede omitirse en fallos técnicos sensibles. |
| `violations` | No | array o null | Detalle por campo para 400/422. |

Un **error funcional** deriva de parámetros o estado de negocio/persistencia conocidos y tiene código estable no reintentable. Un **error técnico** deriva de disponibilidad, timeout, deadlock, permisos o defectos internos; solo los casos clasificados expresamente se marcan reintentables.

## 11. Resumen de errores controlados

### 11.1 Errores runtime de los ocho endpoints

Los siguientes 23 errores 510xx/511xx pueden ocurrir durante la invocación de los Stored Procedures. Las validaciones equivalentes detectadas por la API deben conservar el mismo código funcional según la precedencia de la sección 5.1.

| SQL | SP/ámbito | HTTP | Código API | Reintentable | Condición |
|---|---|---|---|---|---|
| 51000 | SP_CORTE_ResolverInicio | 422 | `GL-CORTE-RES-001` | No | @FechaHoraEvaluacion IS NULL. |
| 51001 | SP_CORTE_ResolverInicio | 422 | `GL-CORTE-RES-002` | No | @TimeoutMinutos IS NULL o @TimeoutMinutos < 0. |
| 51002 | SP_CORTE_ResolverInicio | 409 | `GL-CORTE-RES-003` | No | Existen dos o más cortes oficiales en_proceso. |
| 51003 | SP_CORTE_ResolverInicio | 409 | `GL-CORTE-RES-004` | No | Existen dos o más cortes manuales vencidos en_proceso. |
| 51004 | SP_CORTE_ResolverInicio | 422 | `GL-CORTE-RES-005` | No | El timeout excede el rango seguro de DATETIME al restarlo. |
| 51010 | SP_CORTE_Crear | 422 | `GL-CORTE-CRE-001` | No | @TipoCorte es NULL o distinto de oficial/manual. |
| 51011 | SP_CORTE_Crear | 422 | `GL-CORTE-CRE-002` | No | Tipo oficial y @FechaHoraCorte IS NULL. |
| 51012 | SP_CORTE_Crear | 422 | `GL-CORTE-CRE-003` | No | Tipo manual y @FechaHoraCorte IS NOT NULL. |
| 51013 | SP_CORTE_Crear | 422 | `GL-CORTE-CRE-004` | No | @FechaHoraEjecucion IS NULL. |
| 51100 | SP_Pendientes_CALIMACO_Ingreso | 422 | `GL-PEND-CAL-ING-001` | No | @corte_id_actual IS NULL o <= 0. |
| 51101 | SP_Pendientes_CALIMACO_Ingreso | 422 | `GL-PEND-CAL-ING-002` | No | @MaxReintentosPorSistema IS NULL o <= 0. |
| 51102 | SP_Pendientes_CALIMACO_Ingreso | 422 | `GL-PEND-CAL-ING-003` | No | @in_EsReintentoForzado IS NULL. |
| 51110 | SP_Pendientes_CALIMACO_Salida | 422 | `GL-PEND-CAL-SAL-001` | No | @corte_id_actual IS NULL o <= 0. |
| 51111 | SP_Pendientes_CALIMACO_Salida | 422 | `GL-PEND-CAL-SAL-002` | No | @MaxReintentosPorSistema IS NULL o <= 0. |
| 51112 | SP_Pendientes_CALIMACO_Salida | 422 | `GL-PEND-CAL-SAL-003` | No | @in_EsReintentoForzado IS NULL. |
| 51120 | SP_Pendientes_CMP_Ingreso | 422 | `GL-PEND-CMP-ING-001` | No | @corte_id_actual IS NULL o <= 0. |
| 51121 | SP_Pendientes_CMP_Ingreso | 422 | `GL-PEND-CMP-ING-002` | No | @MaxReintentosPorSistema IS NULL o <= 0. |
| 51122 | SP_Pendientes_CMP_Ingreso | 422 | `GL-PEND-CMP-ING-003` | No | @in_EsReintentoForzado IS NULL. |
| 51130 | SP_Pendientes_CMP_Salida | 422 | `GL-PEND-CMP-SAL-001` | No | @corte_id_actual IS NULL o <= 0. |
| 51131 | SP_Pendientes_CMP_Salida | 422 | `GL-PEND-CMP-SAL-002` | No | @MaxReintentosPorSistema IS NULL o <= 0. |
| 51132 | SP_Pendientes_CMP_Salida | 422 | `GL-PEND-CMP-SAL-003` | No | @in_EsReintentoForzado IS NULL. |
| 51140 | SP_Pendientes_SICA_Ingreso | 422 | `GL-PEND-SICA-ING-001` | No | @MaxReintentosPorSistema IS NULL o <= 0. |
| 51150 | SP_Pendientes_SICA_Salida | 422 | `GL-PEND-SICA-SAL-001` | No | @MaxReintentosPorSistema IS NULL o <= 0. |

### 11.2 Errores operativos de despliegue, startup o healthcheck

Los siguientes seis errores 500xx pertenecen a los scripts DDL/despliegue de SP. No se presentan como errores runtime de los endpoints; deben impedir el arranque o marcar el healthcheck como no saludable.

| SQL | SP/ámbito | HTTP | Código API | Reintentable | Condición |
|---|---|---|---|---|---|
| 50000 | DDL de tablas | 503 | `GL-SQL-DEPLOY-001` | No | El contexto posterior a USE no es bd_autobot. |
| 50001 | DDL de tablas | 503 | `GL-SQL-SCHEMA-001` | No | dbo.Corte existe, pero falta al menos una columna obligatoria. |
| 50002 | DDL de tablas | 503 | `GL-SQL-SCHEMA-002` | No | dbo.bitacora_transacciones existe, pero falta al menos una columna obligatoria. |
| 50010 | Despliegue de SP | 503 | `GL-SQL-DEPLOY-010` | No | El script de SP se ejecuta fuera de bd_autobot. |
| 50011 | Despliegue de SP | 503 | `GL-SQL-DEPLOY-011` | No | No existe dbo.Corte como tabla de usuario. |
| 50012 | Despliegue de SP | 503 | `GL-SQL-DEPLOY-012` | No | No existe dbo.bitacora_transacciones como tabla de usuario. |

La matriz detallada, con ejemplos JSON para cada error y la precedencia normativa, está en `MATRIZ_MAPEO_ERRORES_SQL_HTTP.md`. Los códigos intermedios no emitidos por los scripts no reciben mapeos inventados.

## 12. Seguridad propuesta

| Alternativa | Ventajas | Riesgos | Evaluación |
|---|---|---|---|
| OAuth 2.0 Client Credentials | Identidad técnica por ambiente, tokens de corta vida, scopes, rotación centralizada y mejor auditoría. | Requiere proveedor de identidad, gestión de certificados/secretos y sincronización horaria. | **Recomendada** para UiPath → API. |
| API Key en header | Implementación simple y compatible con UiPath. | Secreto de larga vida, menor granularidad, mayor impacto de filtración; exige rotación y controles de red. | Alternativa transitoria, no preferida. |
| Autenticación integrada empresarial | Evita secreto de aplicación cuando existe dominio y Kerberos administrado. | Acoplamiento a red/dominio, delegación y operación más complejas; menor portabilidad. | Viable solo si la plataforma corporativa lo exige. |

**Recomendación:** OAuth 2.0 Client Credentials con una identidad de aplicación por ambiente, scope mínimo `gestion-ludopatas.execute`, secretos/certificados fuera del código y restricción de red. OpenAPI conserva `ApiKeyAuth` únicamente como alternativa de análisis marcada `x-active: false`; no está autorizada por la seguridad global ni por ninguna operación. Las operaciones usan exclusivamente la recomendación OAuth2.

## 13. Idempotencia y reintentos

### 13.1 Crear corte

- Header propuesto obligatorio: `Idempotency-Key` (16..128 caracteres).
- Vigencia propuesta: 24 horas, configurable y pendiente de aprobación.
- Primera solicitud válida: ejecutar el SP, persistir fingerprint del payload, status y respuesta; devolver `201`.
- Repetición con misma clave y mismo payload: reproducir el status/cuerpo original y devolver `Idempotency-Replayed: true` sin ejecutar otra vez el SP.
- Misma clave con payload distinto: `409 GL-IDEMP-001`.
- Limitación crítica: `SP_CORTE_Crear` no tiene clave natural ni soporte de idempotencia. La garantía depende enteramente de un almacén/API consistente. Debe resolverse el caso de crash después del commit SQL y antes de registrar la respuesta.

### 13.2 Resolver inicio

`Idempotency-Key` se propone como opcional para reproducir la misma respuesta ante timeout del cliente. Sin ese mecanismo, una segunda ejecución puede observar un estado distinto aunque las actualizaciones de cierre sean naturalmente no duplicativas.

### 13.3 Pendientes

Los seis SP son de solo lectura; pueden reintentarse técnicamente. Sin embargo, no reservan filas, por lo que dos consumidores concurrentes pueden recibir los mismos pendientes. La prevención de procesamiento duplicado pertenece al diseño de consumo/actualización, fuera de estos SP.

### 13.4 Política de reintento UiPath propuesta

- Reintentar solo cuando `retryable=true`.
- Backoff exponencial con jitter y límite configurado.
- No reintentar automáticamente `400`, `401`, `403`, `409` funcional ni `422`.
- Para `503/504` reintentables, conservar el mismo `Idempotency-Key` en crear corte.

## 14. Matriz de trazabilidad

| Endpoint propuesto | Método | Stored Procedure | Tablas | Tipo de operación | Respuesta exitosa | Errores principales |
|---|---|---|---|---|---|---|
| `/api/v1/cortes/resoluciones-inicio` | POST | `dbo.SP_CORTE_ResolverInicio` | `dbo.Corte` | Lectura + actualización condicional transaccional | 200, una resolución | 51000–51004; conflictos 51002/51003; técnicos SQL |
| `/api/v1/cortes` | POST | `dbo.SP_CORTE_Crear` | `dbo.Corte` | Inserción | 201, `corteId` | 51010–51013; idempotencia; técnicos SQL |
| `/api/v1/pendientes/calimaco/ingresos/busqueda` | POST | `dbo.SP_Pendientes_CALIMACO_Ingreso` | `dbo.bitacora_transacciones` | Consulta | 200, arreglo 0..N | 51100–51102; técnicos SQL |
| `/api/v1/pendientes/calimaco/salidas/busqueda` | POST | `dbo.SP_Pendientes_CALIMACO_Salida` | `dbo.bitacora_transacciones` | Consulta | 200, arreglo 0..N | 51110–51112; técnicos SQL |
| `/api/v1/pendientes/cmp/ingresos/busqueda` | POST | `dbo.SP_Pendientes_CMP_Ingreso` | `dbo.bitacora_transacciones` | Consulta | 200, arreglo 0..N | 51120–51122; técnicos SQL |
| `/api/v1/pendientes/cmp/salidas/busqueda` | POST | `dbo.SP_Pendientes_CMP_Salida` | `dbo.bitacora_transacciones` | Consulta | 200, arreglo 0..N | 51130–51132; técnicos SQL |
| `/api/v1/pendientes/sica/ingresos/busqueda` | POST | `dbo.SP_Pendientes_SICA_Ingreso` | `dbo.bitacora_transacciones` | Consulta | 200, arreglo 0..N | 51140; técnicos SQL |
| `/api/v1/pendientes/sica/salidas/busqueda` | POST | `dbo.SP_Pendientes_SICA_Salida` | `dbo.bitacora_transacciones` | Consulta | 200, arreglo 0..N | 51150; técnicos SQL |

## 15. Limitaciones

1. Los endpoints, seguridad, códigos HTTP e idempotencia son propuestas no implementadas.
2. No existe operación atómica resolver+crear; un caller puede sufrir una carrera entre ambas llamadas.
3. La idempotencia de crear corte no puede garantizarse únicamente con el SP actual.
4. Los SP de pendientes no reservan ni ordenan filas.
5. `corteIdActual` solo se valida como positivo; SQL no confirma que exista.
6. Los `NULL` en estados/reintentos pueden excluir filas de los SELECT.
7. `DATETIME` SQL no conserva zona horaria; la política de conversión debe aprobarse.
8. Los códigos 500xx pertenecen exclusivamente a despliegue/startup/healthcheck y no forman parte de las respuestas runtime normales de los endpoints.
9. No se agregan endpoints de actualización de bitácora porque no existen SP correspondientes en el alcance.

## 16. Decisiones pendientes del desarrollador/API owner

1. Aprobar rutas, versión base y convención `camelCase`.
2. Aprobar OAuth2, proveedor de identidad, scopes y material de credencial.
3. Definir timeout SQL/API y política exacta de reintentos.
4. Aprobar vigencia y almacén de `Idempotency-Key`, incluyendo atomicidad y recuperación ante crash.
5. Definir política de zona horaria y conversión a SQL `DATETIME`.
6. Definir si `sqlErrorNumber` se expone en ambientes no productivos o siempre se omite.
7. Definir límites de tamaño, rate limiting y circuit breaker.
8. Definir autorización por operación o un único scope.
9. Definir si resolver inicio debe exigir idempotencia, no solo soportarla opcionalmente.
10. Definir observabilidad, retención y enmascaramiento de `traceId`/payloads.

## 17. Guía de validación del contrato

### Iteración 1 — Fidelidad SQL

- Comparar nombres, firmas, defaults, tipos y orden de columnas contra ambos scripts.
- Verificar los 29 códigos `THROW` emitidos.
- Confirmar escritura solo en los dos SP de cortes.
- Confirmar cardinalidad y nulabilidad de cada ResultSet.
- Confirmar filtros, ausencia de `ORDER BY`, ausencia de reserva y limitaciones de concurrencia.

### Iteración 2 — Calidad OpenAPI

- Parsear el YAML y comprobar `openapi: 3.1.0`.
- Verificar ocho paths y ocho `operationId` únicos.
- Resolver todos los `$ref` locales.
- Validar ejemplos contra schemas principales.
- Revisar `type: [T, null]`, formatos, required y `additionalProperties`.
- Confirmar schemas separados `PendienteSicaIngresoItem` y `PendienteSicaSalidaItem`.
- Confirmar `format: int32`, máximo 2147483647 y mínimos funcionales en todos los campos SQL `INT`.
- Confirmar que cada respuesta `422` muestra los códigos específicos de su operación y aplica la precedencia de la sección 5.1.
- Confirmar `WWW-Authenticate` en la respuesta OAuth `401` y `ApiKeyAuth` como alternativa no activa.
- Confirmar que servers y token URL usan dominios `.invalid` y que no existen secretos.
- Confirmar reutilización de responses, parameters, headers y securitySchemes.

## 18. Resultado de revisión

- **Iteración 1:** conforme con los scripts fuente; se conservaron 29 `THROW`, se separaron seis errores 500xx operativos de 23 errores runtime y se corrigió la nulabilidad SICA ingreso/salida.
- **Iteración 2:** YAML parseado, referencias locales comprobadas, ocho operaciones y respuestas `422` específicas verificadas, campos SQL `INT` limitados a `int32`, OAuth `401` completado y alternativa API Key marcada como no activa.
