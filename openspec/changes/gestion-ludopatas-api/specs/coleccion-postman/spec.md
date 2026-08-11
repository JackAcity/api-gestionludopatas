## ADDED Requirements

### Requirement: Colección Postman completa y sincronizada con el contrato
El sistema SHALL entregar una colección Postman con un request por cada uno de los 8 endpoints, usando la autenticación real (API Key, Decisión D2) y ejemplos de body/response tomados literal de las specs `corte-resolver-inicio`, `corte-crear` y las 6 de `pendientes-*`. La colección SHALL reemplazar el flujo OAuth2 de la propuesta original en `endpoint/GestionLudopatas_Postman_Collection_v0.2.0-propuesta.json` — no se entrega con un mecanismo de auth que el servicio no implementa.

#### Scenario: Un request por endpoint
- **WHEN** se revisa la colección entregada
- **THEN** existen exactamente 8 requests de negocio, uno por operación, cada uno apuntando a la ruta y método definidos en su spec

#### Scenario: Auth alineada con la implementación real
- **WHEN** se inspecciona la configuración de auth de la colección o de sus requests
- **THEN** usa el header `X-Api-Key` (D2); no queda ningún request ni ambiente configurado para OAuth2 Client Credentials

#### Scenario: Al menos un caso de error por endpoint
- **WHEN** se revisa la colección
- **THEN** cada uno de los 8 requests de negocio tiene, además del caso feliz, al menos un ejemplo guardado de un caso de error representativo (422 o 409 según su spec), con el body de error `problem+json` real

### Requirement: Variables de ambiente, no valores hardcodeados
El ambiente Postman SHALL parametrizar `baseUrl`, `apiKey`, `correlationId` e `idempotencyKey` (este último solo relevante para `crearCorte`) como variables de colección/ambiente. Ningún request SHALL tener estos valores escritos directo en la URL, headers o body.

#### Scenario: Cambio de ambiente sin editar requests
- **WHEN** se cambia el ambiente Postman activo (ej. de dev a qa)
- **THEN** todos los requests apuntan al nuevo `baseUrl` y usan el `apiKey` de ese ambiente sin necesidad de editar ningún request individualmente

### Requirement: Validación de la colección contra el ambiente real
La colección SHALL ejecutarse con `newman` (o equivalente) contra el deploy real de dev (Fase 1 o 2 de `despliegue-docker`/`secretos-vault`) antes de considerar el change listo para archivar — no alcanza con haberla escrito, tiene que correr contra el servicio desplegado. El resultado (requests/asserts pasados) SHALL quedar documentado, mismo criterio que `reto_tecnico_backend_senior` (`README.md`: "Verificada con newman contra el stack real").

#### Scenario: Corrida completa contra dev
- **WHEN** se ejecuta la colección con `newman` contra la URL real del servicio desplegado en dev
- **THEN** los 8 requests de negocio (más sus casos de error) responden con el status y shape esperados, y el resultado (N/N requests, M/M asserts) queda documentado

#### Scenario: Falla si el contrato se desvía
- **WHEN** una respuesta real no coincide con lo que la colección espera (status, campo, código `GL-*`)
- **THEN** la corrida de `newman` falla en ese assert — es la señal de que la implementación o la spec quedaron desincronizadas
