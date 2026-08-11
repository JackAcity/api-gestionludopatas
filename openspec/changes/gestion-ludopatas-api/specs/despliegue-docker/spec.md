## ADDED Requirements

### Requirement: Imagen Docker del servicio
El sistema SHALL construirse como una imagen Docker versionada (`gestion-ludopatas-api:<version>[-<env>]`) a partir de un `Dockerfile` propio del proyecto, ejecutable en el mismo ambiente Linux on-premise donde ya corre `api-sica`. El contenedor SHALL ejecutarse con un usuario no root (mismo criterio de endurecimiento ya aplicado en `reto_tecnico_backend_senior`, C14).

#### Scenario: Build reproducible
- **WHEN** se ejecuta el build de la imagen contra un commit dado
- **THEN** la imagen resultante queda etiquetada con la versión correspondiente y puede desplegarse en cualquier ambiente sin recompilar

#### Scenario: Usuario no root
- **WHEN** el contenedor arranca
- **THEN** el proceso de la API corre como usuario no root dentro del contenedor

### Requirement: Despliegue vía docker-compose en el ambiente compartido
El sistema SHALL desplegarse mediante `docker-compose` (build + `up -d`) en el mismo app server Linux donde ya está desplegado `api-sica`, sin introducir un host o red nuevos. El puerto expuesto SHALL ser propio y no SHALL colisionar con ningún puerto ya ocupado por otro servicio en ese host.

#### Scenario: Deploy en ambiente existente
- **WHEN** se despliega `GestionLudopatas.Api` en un ambiente (dev/qa/prod) donde `api-sica` ya corre
- **THEN** ambos servicios corren simultáneamente en el mismo host, en puertos distintos, sin que uno interfiera con el otro

#### Scenario: Verificación post-deploy sin errores de conexión
- **WHEN** el contenedor termina de arrancar
- **THEN** los logs no muestran errores de conexión a SQL Server ni a Vault; un `GET /health` contra la URL real del ambiente responde `200`

### Requirement: Exposición de documentación OpenAPI condicionada al ambiente
El sistema SHALL exponer la documentación OpenAPI/Swagger interactiva únicamente en el ambiente `dev`. En `qa` y `prod` SHALL permanecer deshabilitada (mismo criterio OWASP aplicado en `api-sica`).

#### Scenario: Swagger habilitado en dev
- **WHEN** el servicio corre con el ambiente configurado como `dev`
- **THEN** la documentación interactiva está disponible

#### Scenario: Swagger deshabilitado fuera de dev
- **WHEN** el servicio corre con el ambiente configurado como `qa` o `prod`
- **THEN** la documentación interactiva no está disponible; solo queda el contrato OpenAPI estático para referencia
