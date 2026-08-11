# Guía de importación y uso en Postman — Gestión de Ludópatas

- **Estado del contrato:** propuesta TO-BE `0.2.0-propuesta`.
- **Implementación:** no implementada ni aprobada como versión productiva.
- **Fuente técnica principal:** `openapi_gestion_ludopatas_v0.2.0-propuesta.yaml`.
- **Fuentes complementarias:** `PROPUESTA_CONTRATO_API_SQL_GestionLudopatas.md` y `MATRIZ_MAPEO_ERRORES_SQL_HTTP.md`.
- **Seguridad propuesta activa:** OAuth 2.0 Client Credentials.
- **API Key:** alternativa documentada, no activa y no utilizada por la colección.
- **Ejecución real:** no se ejecutaron endpoints ni Stored Procedures durante la generación.

## 1. Objetivo

Permitir al desarrollador importar una colección Postman y un environment para revisar y, cuando exista una implementación autorizada, ejecutar las ocho operaciones HTTP propuestas. La colección conserva métodos, paths, bodies, headers, ejemplos exitosos, errores aplicables, trazabilidad e idempotencia definidos por el contrato.

Las respuestas guardadas son **ejemplos contractuales**. No demuestran que la API exista, que responda actualmente ni que los Stored Procedures hayan sido ejecutados.

## 2. Archivos incluidos

1. `GestionLudopatas_Postman_Collection_v0.2.0-propuesta.json`
2. `GestionLudopatas_Postman_Environment_v0.2.0-propuesta.json`
3. `GUIA_IMPORTACION_USO_POSTMAN_GestionLudopatas.md`

La colección usa **Postman Collection Format v2.1.0** y no contiene pre-request scripts, test scripts ni eventos ejecutables.

## 3. Requisitos

- Postman Desktop o Web con capacidad para importar Collection v2.1.0 y environments.
- URL base real del ambiente, cuando la API haya sido implementada y habilitada.
- URL real del proveedor OAuth2, cuando sea aprobada.
- `clientId`, `clientSecret` y scope autorizados para el ambiente.
- Permiso para invocar el scope `gestion-ludopatas.execute`.
- Autorización para utilizar datos de prueba en el ambiente correspondiente.

Los archivos entregados no contienen hosts reales, credenciales, tokens ni secretos.

## 4. Importar la colección

1. Abrir Postman.
2. Seleccionar **Import**.
3. Arrastrar o seleccionar `GestionLudopatas_Postman_Collection_v0.2.0-propuesta.json`.
4. Confirmar que aparece la colección **Gestión de Ludópatas API — v0.2.0-propuesta**.
5. Verificar que contiene cinco carpetas:
   - `00 - Autenticación referencial`
   - `01 - Cortes`
   - `02 - Pendientes CALIMACO`
   - `03 - Pendientes CMP`
   - `04 - Pendientes SICA`

## 5. Importar el environment

1. Seleccionar **Import**.
2. Importar `GestionLudopatas_Postman_Environment_v0.2.0-propuesta.json`.
3. Confirmar que aparece **Gestión de Ludópatas — v0.2.0-propuesta**.
4. Revisar que `accessToken`, `clientSecret` y `apiKey` estén vacíos.
5. No compartir una exportación posterior que contenga valores reales.

## 6. Seleccionar el environment

En el selector de environment de Postman, activar:

`Gestión de Ludópatas — v0.2.0-propuesta`

Antes de enviar una request, abrir el environment y comprobar que las variables necesarias tienen el valor del ambiente autorizado.

## 7. Variables incluidas

| Variable | Valor inicial entregado | Uso |
|---|---|---|
| `baseUrl` | `https://api.example.invalid/gestion-ludopatas` | URL base ilustrativa de las ocho operaciones. |
| `tokenUrl` | `https://identity.example.invalid/oauth2/token` | Endpoint ilustrativo para obtener el token. |
| `accessToken` | vacío | Bearer token copiado manualmente. |
| `clientId` | vacío | Identificador OAuth2 del ambiente. |
| `clientSecret` | vacío, tipo secret | Secreto OAuth2 del ambiente. |
| `scope` | `gestion-ludopatas.execute` | Scope propuesto activo. |
| `apiKey` | vacío, tipo secret | Alternativa no activa; ninguna request la utiliza. |
| `correlationId` | `uipath-job-example-001` | Valor de `X-Correlation-Id`. |
| `idempotencyKey` | `cut-example-00000001` | Valor ilustrativo de `Idempotency-Key`. |

## 8. Configurar `baseUrl`

El valor entregado es un placeholder `.invalid`. Reemplazarlo únicamente cuando exista una URL aprobada.

`baseUrl` ya incluye el segmento:

```text
/gestion-ludopatas
```

Las requests agregan después los paths `/api/v1/...`. No duplicar `/gestion-ludopatas`, porque produciría una URL incorrecta.

Ejemplo conceptual de concatenación:

```text
{{baseUrl}}/api/v1/cortes
```

## 9. Configurar `tokenUrl`

Reemplazar `https://identity.example.invalid/oauth2/token` por el token endpoint aprobado del ambiente. No registrar el secreto en la colección, en descripciones ni en respuestas guardadas.

## 10. Obtener el token manualmente

1. Abrir `00 - Autenticación referencial`.
2. Seleccionar `OAuth2 - Obtener token client_credentials`.
3. Configurar `clientId` y `clientSecret` en el environment.
4. Confirmar `scope=gestion-ludopatas.execute`.
5. Enviar la request únicamente contra un proveedor de identidad real y autorizado.

La request usa `application/x-www-form-urlencoded` con:

```text
grant_type=client_credentials
client_id={{clientId}}
client_secret={{clientSecret}}
scope={{scope}}
```

No tiene autenticación Bearer heredada y no contiene scripts para guardar el token.

## 11. Copiar el token a `accessToken`

Después de una respuesta OAuth2 válida:

1. Copiar manualmente el valor de `access_token`.
2. Abrir el environment activo.
3. Pegar el valor en `accessToken`.
4. Guardar el environment.
5. Evitar exportar o compartir el environment mientras contenga el token.

Las ocho requests funcionales heredan de la colección:

```text
Authorization: Bearer {{accessToken}}
```

## 12. Configurar `correlationId`

Actualizar `correlationId` con un identificador trazable de la ejecución o prueba. Las ocho operaciones envían:

```http
X-Correlation-Id: {{correlationId}}
```

La API propuesta debe responder con `X-Trace-Id`. Ambos valores facilitan la correlación entre UiPath, la API y los registros técnicos.

## 13. Configurar `idempotencyKey`

### Crear corte

`Idempotency-Key: {{idempotencyKey}}` está habilitado y es obligatorio. Usar una clave nueva para cada intención de creación. Ante un reintento técnico del mismo request, conservar exactamente la misma clave y el mismo payload.

Reutilizar la clave con un payload diferente puede producir:

```text
409 - GL-IDEMP-001
```

### Resolver inicio

El header está incluido, pero deshabilitado porque es opcional. Para usar la propuesta de idempotencia:

1. Abrir los headers de `Resolver inicio de corte`.
2. Habilitar `Idempotency-Key`.
3. Definir una clave apropiada.
4. Conservarla si se reintenta la misma intención.

Los endpoints de pendientes no usan este header.

## 14. Estructura y ejecución de carpetas

### `01 - Cortes`

- `Resolver inicio de corte`  
  `operationId: resolverInicioCorte`  
  `POST /api/v1/cortes/resoluciones-inicio`

- `Crear corte`  
  `operationId: crearCorte`  
  `POST /api/v1/cortes`

### `02 - Pendientes CALIMACO`

- `Buscar pendientes CALIMACO ingreso`  
  `operationId: buscarPendientesCalimacoIngreso`

- `Buscar pendientes CALIMACO salida`  
  `operationId: buscarPendientesCalimacoSalida`

### `03 - Pendientes CMP`

- `Buscar pendientes CMP ingreso`  
  `operationId: buscarPendientesCmpIngreso`

- `Buscar pendientes CMP salida`  
  `operationId: buscarPendientesCmpSalida`

### `04 - Pendientes SICA`

- `Buscar pendientes SICA ingreso`  
  `operationId: buscarPendientesSicaIngreso`

- `Buscar pendientes SICA salida`  
  `operationId: buscarPendientesSicaSalida`

Cada request contiene un body principal válido tomado del OpenAPI. Revisar los datos antes de enviarlos contra un ambiente implementado.

## 15. Interpretar respuestas exitosas

| Operación | HTTP | Resultado |
|---|---:|---|
| Resolver inicio | 200 | Un objeto de resolución. |
| Crear corte | 201 | Un objeto con `corteId`. |
| Seis búsquedas de pendientes | 200 | Colección de cero a muchas filas. |

Una colección vacía `[]` es una respuesta exitosa: indica que no existen filas elegibles bajo los filtros AS-BUILT.

En SICA se conservaron ejemplos distintos:

- Ingreso usa `PendienteSicaIngresoItem`; `nombresApellidos` es string no nulo.
- Salida usa `PendienteSicaSalidaItem`; el ejemplo permite `nombresApellidos: null` y `fechaInscripcion: null`.

`Idempotency-Replayed: true` indica que la API reprodujo una respuesta almacenada por idempotencia; no implica una nueva ejecución del Stored Procedure.

## 16. Interpretar `retryable`

El body común de error incluye:

```json
{
  "retryable": true
}
```

Este valor indica que el cliente puede considerar un reintento técnico controlado. No obliga a reintentar.

Aplicar:

- límite de intentos;
- backoff exponencial;
- jitter;
- conservación de `Idempotency-Key` al reintentar crear corte.

No reintentar ciegamente los errores marcados `retryable: false`.

## 17. Tratamiento de errores HTTP

| HTTP | Tratamiento |
|---:|---|
| 400 | Corregir serialización, JSON o tipos incompatibles. |
| 401 | Obtener o renovar el token. Revisar `WWW-Authenticate`. |
| 403 | Revisar scope, identidad y autorización. |
| 409 | Corregir el conflicto de estado o de idempotencia. No se agregó 409 a endpoints que no lo declaran. |
| 422 | Corregir la regla funcional específica o la validación contractual. |
| 500 | Escalar como error interno o de serialización; no asumir reintento. |
| 503 | Revisar disponibilidad, deadlock o permisos SQL; obedecer `retryable`. |
| 504 | Aplicar reintento técnico controlado cuando `retryable=true`. |

Las respuestas `422` incluyen todos los códigos funcionales específicos de cada operación y `GL-API-REQ-002` como fallback.

Los errores técnicos o de estado conservan un request válido. Su descripción identifica la precondición —por ejemplo, deadlock, indisponibilidad, permisos o inconsistencia persistida— porque no pueden provocarse únicamente cambiando el body.

## 18. Respuestas guardadas

Las respuestas visibles bajo cada request son ejemplos contractuales derivados del OpenAPI y de la matriz SQL→HTTP. Incluyen:

- status y código HTTP;
- `Content-Type`;
- `X-Trace-Id`;
- `WWW-Authenticate` en 401;
- `Idempotency-Replayed` en éxitos de operaciones con idempotencia;
- body compatible con `ApiError` o con el schema exitoso;
- descripción de la condición;
- `originalRequest` coherente con la validación cuando puede expresarse mediante el request.

No deben confundirse con un historial de ejecución real.

## 19. Estado de `ApiKeyAuth`

`ApiKeyAuth` figura en el OpenAPI como alternativa de análisis con:

```text
x-active: false
```

La colección:

- no configura `X-API-Key`;
- no usa `{{apiKey}}` en ninguna request;
- no agrega API Key a la seguridad raíz;
- conserva la variable vacía únicamente para reflejar el alcance aprobado del environment.

No activar API Key sin una decisión contractual nueva.

## 20. Solución de problemas

### La colección importa, pero las requests no resuelven variables

- Confirmar que el environment correcto esté seleccionado.
- Revisar que no existan espacios adicionales en los nombres.
- Confirmar que `baseUrl`, `accessToken` y `correlationId` estén definidos.

### La URL contiene `/gestion-ludopatas` dos veces

- Corregir `baseUrl`.
- Conservar una sola aparición del segmento en la URL base.

### Se recibe 401

- Revisar `accessToken`.
- Confirmar expiración, issuer, audience y configuración OAuth2 del ambiente.
- Consultar el header `WWW-Authenticate`.
- No usar `apiKey` como reemplazo.

### Se recibe 403

- Confirmar el scope `gestion-ludopatas.execute`.
- Revisar que la identidad técnica esté autorizada para la operación.

### Se recibe 409 al crear corte

- Revisar si la misma `Idempotency-Key` se utilizó con un payload diferente.
- Para reintentar la misma intención, conservar clave y payload.
- Para una intención nueva, generar una clave nueva.

### Se recibe 422

- Abrir las respuestas guardadas de la request.
- Comparar el código `GL-CORTE-*` o `GL-PEND-*`.
- Usar `GL-API-REQ-002` solo cuando no exista un código funcional específico.

### El token no se guarda automáticamente

Es el comportamiento esperado. La colección no contiene scripts; copiar manualmente `access_token` a `accessToken`.

### El importador muestra una advertencia sobre `.invalid`

Los dominios `.invalid` son placeholders deliberados. Reemplazarlos únicamente por URLs autorizadas cuando la API y el proveedor de identidad existan.

## 21. Alcance de la validación realizada

La colección y el environment fueron revisados en dos iteraciones:

1. **Fidelidad contractual:** ocho operaciones, métodos, paths, `operationId`, bodies, SICA diferenciado, OAuth2, headers, idempotencia y errores aplicables.
2. **Integridad Postman:** JSON parseable, schema v2.1.0, cinco carpetas, nueve requests totales, variables definidas, ausencia de scripts, cuerpos JSON válidos, `WWW-Authenticate`, headers de idempotencia y nombres reales de los tres entregables.

No se modificaron los tres archivos fuente y no se ejecutaron endpoints reales.
