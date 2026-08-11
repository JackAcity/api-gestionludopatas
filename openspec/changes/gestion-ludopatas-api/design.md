## Context

`bd_autobot` (SQL Server 2019) expone 8 Stored Procedures que UiPath necesita invocar para el flujo de gestión de ludópatas (resolución/creación de "cortes" de proceso, búsqueda de pendientes CALIMACO/CMP/SICA). UiPath no puede conectarse directo a SQL Server — credenciales de base de datos en un robot RPA son una superficie de ataque inaceptable. `GestionLudopatas.Api` es el único punto de entrada: recibe HTTP, ejecuta el SP correspondiente, traduce el resultado/error a JSON.

Fuente técnica de firmas, defaults y los 29 `THROW` SQL: `endpoint/PROPUESTA_CONTRATO_API_SQL_GestionLudopatas.md` y `endpoint/MATRIZ_MAPEO_ERRORES_SQL_HTTP.md` (ya validados en 2 iteraciones contra los scripts DDL/SP). Ese documento también incluye un OpenAPI 3.1 completo (`openapi_gestion_ludopatas_v0.2.0-propuesta.yaml`) usado como referencia de schemas y ejemplos.

Despliegue: on-premise, un solo consumidor conocido (host UiPath), sin CI/CD ni infraestructura cloud en alcance.

## Goals / Non-Goals

**Goals:**
- Exponer los 8 SP como HTTP, sin que el consumidor toque SQL Server directo.
- Errores normalizados y trazables 1:1 contra los 29 `THROW` ya catalogados — cero invención de códigos.
- SOLID: cada SP es un caso de uso aislado (SRP), nuevos SP no tocan los existentes (OCP), los "pendientes" comparten contrato de puerto uniforme (LSP/ISP), Application depende de abstracciones, no de `SqlClient` concreto (DIP).
- Seguridad apropiada para el escenario real: un solo consumidor, red on-premise controlada.

**Non-Goals:**
- No se implementa OAuth2 Client Credentials (ver Decisión D2) ni ningún Identity Provider.
- No se toca DDL/SP ni se agregan endpoints de escritura sobre `bitacora_transacciones`.
- No se resuelve la transacción atómica resolver+crear (limitación ya documentada en la fuente: son dos llamadas independientes).
- No se implementa paginación/reserva de filas en los 6 SP de pendientes — el SP no la tiene, la API no la inventa.
- No se define aún la política exacta de conversión de zona horaria `DATETIME` (abierto en la fuente, ver Open Questions).
- No se escriben tests de integración contra `bd_autobot` real dentro de `dotnet test` — es una instancia on-prem compartida, no algo que el equipo levante por corrida de test. El deliverable de pruebas de este lado es **unitario** (puertos mockeados/fake); la verificación contra la base y el Vault reales la cubre la colección Postman corrida con `newman` contra el deploy de dev (spec `coleccion-postman`).

## Decisions

**D1 — Un solo proyecto Web API con capas por carpeta, no 4 csproj separados.**
El proyecto hermano (`reto_tecnico_backend_senior`) usa Domain/Application/Infrastructure/Api como assemblies separados porque tiene 4 microservicios reales con ciclos de vida y despliegue independientes. `GestionLudopatas.Api` es un único bounded context (un puente, no un sistema distribuido): las mismas capas como carpetas dentro de un solo `.csproj` dan la misma separación de responsabilidades sin la ceremonia de 4 proyectos para 8 endpoints. Si el alcance crece a un sistema multi-servicio, se puede partir después — YAGNI por ahora.

**D2 — Seguridad: API Key + IP allowlist, no OAuth2 Client Credentials.**
El documento fuente (§12) recomienda OAuth2 Client Credentials. Se descarta para este alcance: un solo consumidor conocido (UiPath) en red on-premise no gana nada de OAuth2 (rotación centralizada, múltiples clientes, scopes) y sí gana un Identity Provider adicional que operar, parchear y asegurar. API Key (header `X-Api-Key`, secreto largo, rotable, comparación en tiempo constante) + IP allowlist (solo la subnet del host UiPath) es la superficie mínima correcta para este escenario. El propio OpenAPI fuente ya deja `ApiKeyAuth` definido — se promueve a único esquema activo; se documenta como override deliberado, igual que la tabla de contradicciones resueltas del proyecto hermano.
- Alternativa considerada: OAuth2 — descartada por el motivo anterior.
- Alternativa considerada: mTLS — descartada por costo operativo de gestión de certificados para un solo consumidor; queda como upgrade path si se suman más consumidores.

**D3 — Mapeo de errores SQL→HTTP data-driven, una sola tabla, no un switch por handler.**
Los 29 `THROW` (23 runtime + 6 deploy) viven en un diccionario único `int sqlErrorNumber -> ErrorMapping(httpStatus, code, mensaje, retryable)`. Cada handler solo captura `SqlException` y delega al mapeador; agregar un SP nuevo no implica repetir el switch. Esto es literalmente la tabla de la sección 4 de `MATRIZ_MAPEO_ERRORES_SQL_HTTP.md` convertida a datos.

**D4 — Puertos por caso de uso, no un repositorio genérico.**
`ICorteResolver`, `ICorteCreator`, y un puerto por cada búsqueda de pendientes (`IBuscarPendientesCalimacoIngreso`, etc.) en vez de una interfaz `IGestionLudopatasRepository` con 8 métodos. Un consumidor de Application que solo resuelve inicio de corte no debe depender de los otros 7 métodos (ISP). Los 6 puertos de pendientes comparten una forma común (`Task<IReadOnlyList<TItem>> BuscarAsync(TRequest req, CancellationToken ct)`) para que el adaptador de Infrastructure sea intercambiable y testeable de forma uniforme (LSP).

**D5 — Login SQL de solo `EXECUTE`, sin acceso a tablas.**
La cadena de conexión de la API usa un login SQL Server con `GRANT EXECUTE` únicamente sobre los 8 SP — sin `SELECT/INSERT/UPDATE` directo sobre `Corte` ni `bitacora_transacciones`. Si la API se ve comprometida, el radio de acción queda limitado a lo que los SP ya permiten (que incluyen su propia validación de negocio).

**D6 — Errores 500xx (50000-50012) son señal de healthcheck, no respuesta runtime.**
Se validan en el arranque (o en un healthcheck `/health/sql` periódico): si el esquema/despliegue de SQL no es compatible, el servicio se marca no saludable y deja de aceptar tráfico. Nunca se devuelven como cuerpo de un `POST` de negocio — evita filtrar detalle de despliegue a UiPath.

**D7 — Idempotencia de `crearCorte`: reserva atómica por instancia y upgrade path persistente.**
La implementación actual usa el singleton `IdempotencyStore` en memoria porque `bd_autobot` es solo-`EXECUTE` (D5) y aún no existe una base propia de la API. La operación `ReservarAsync` es atómica por clave: devuelve exactamente una de tres decisiones mutuamente excluyentes: ejecutar (el propietario de la reserva), reproducir (misma clave y fingerprint) o conflicto (`409 GL-IDEMP-001`, fingerprint distinto). Mientras el propietario ejecuta el SP, otra solicitud con el mismo fingerprint espera su resultado; nunca ejecuta el SP de nuevo. Solo una respuesta exitosa se publica con TTL de 24 horas; fallo funcional o excepción libera la reserva para un reintento.

Esto elimina la carrera `Buscar → ejecutar SP → Guardar` dentro de una instancia, pero **no satisface durabilidad entre reinicios ni coordinación entre varias instancias**. Antes de escalar horizontalmente o considerar el requisito de persistencia de 24 horas cerrado, se debe sustituir por una tabla en una base propia de la API, por ejemplo `IdempotencyRecord(key, requestFingerprint, status, responseBody, createdAt)`, con restricción única y adquisición atómica. El hueco de "crash entre commit del SP y publicación de la respuesta" sigue siendo un riesgo abierto — eliminarlo requeriría coordinar transaccionalmente dos recursos o rediseñar el SP, fuera de alcance.

**D8 — Secretos vía HashiCorp Vault, mismo patrón que `api-sica` (no inventar uno propio).**
`api-sica` (repo hermano NestJS, `C:\Users\jaaguilar\Documents\projects\Q2-2026\api-sica`) ya implementó y verificó en producción el acceso a Vault (`openspec/changes/archive/vault-integration/` en ese repo): mismo Vault (`https://dev-app-vault.acity.com.pe`), KV v2, mount `secret`, sin SDK — HTTP directo con header `X-Vault-Token`, path **dedicado por servicio** (nunca compartido entre servicios), fail-fast si Vault no responde o el token es inválido. `GestionLudopatas.Api` sigue el mismo patrón:
- **Dos paths separados, no uno** (corrección sobre el diseño inicial, por single responsibility — decisión explícita del usuario): la connection string SQL y la API Key son secretos con dueño y ciclo de rotación distintos (el DBA rota la password de BD; nosotros rotamos la API Key sin que el DBA intervenga), así que no comparten el mismo KV entry. `api-gestionludopatas/db` → `host/port/dbname/username/password`. `api-gestionludopatas/apikey` → `api_key` únicamente. Sin campo equivalente a `bot_user_id` — ninguno de los 8 SP recibe un identificador de usuario/bot como parámetro.
- **Ningún path lleva segmento de ambiente — ni siquiera dev/qa** (corrección explícita del usuario: el objetivo es replicar la estructura que tendrá producción, y en producción no hay ambigüedad de ambiente que desambiguar). `dev` y `qa` comparten el mismo Vault (`https://dev-app-vault.acity.com.pe`) y **el mismo path** (`api-gestionludopatas/db`, `api-gestionludopatas/apikey`) — al pasar de probar en dev a probar en qa, se sobreescribe el valor (KV v2 versiona automáticamente, no se pierde el anterior). El Vault de producción es una instancia separada con los mismos paths sin segmento. Esto es deliberadamente distinto del patrón de `api-sica` (que sí separa `/dev/`, `/qa/` dentro de un mismo Vault) — decisión propia de este proyecto, no un error de replicar mal el precedente.
- Convención del equipo: prefijo `api-` para un backend puro (mismo criterio que `api-sica`), prefijo `app_` reservado para un sistema completo (ej. `app_reclamos`).
- En .NET el equivalente al truco de import dinámico de Node (`main.ts` hace fetch antes de que `envs.ts` se evalúe) es: resolver ambos secretos de Vault **antes** de `builder.Build()` en `Program.cs` (dos llamadas), e inyectar los valores resueltos en `builder.Configuration` antes de que cualquier `AddXxx` que dependa de ellos se registre. Mismo requisito, mecanismo distinto por diferencia de runtime.
- Cliente HTTP mínimo (`HttpClient` nativo) genérico a propósito — `VaultSecretClient.ObtenerAsync` solo sabe leer un KV v2 en un path dado y devolver sus campos; no conoce el dominio "connection string" ni "API Key", eso lo arma quien lo llama (`Program.cs`). Sin agregar un SDK de Vault — igual razón que el precedente: es una sola llamada GET a `/v1/secret/data/{path}` con parseo `data.data`.
- TLS: nunca deshabilitar validación de certificado; si el Vault usa CA interna, confiar en ella explícitamente (equivalente a `NODE_EXTRA_CA_CERTS` del precedente), no un bypass global.
- La IP allowlist (D2) **no** se considera secreto — se configura vía `appsettings`/variable de entorno por ambiente, no vía Vault; a diferencia de la API Key, no es una credencial, es topología de red.

**D9 — Despliegue Docker en el mismo ambiente on-premise que `api-sica`.**
No se provisiona infraestructura nueva: `GestionLudopatas.Api` se despliega como un contenedor más en los mismos app servers Linux donde ya corre `api-sica` (ver `openspec/changes/archive/arquitectura-despliegue/servidores.md` en ese repo para las coordenadas exactas de cada ambiente — no se duplican acá por ser credenciales operativas de un repo distinto). Mismo patrón operativo:
- `docker-compose` con build + `up -d`, imagen versionada `gestion-ludopatas-api:<version>[-dev]`.
- Puerto elegido para no colisionar con los servicios ya corriendo en ese host (verificar puertos ocupados antes de fijar uno, igual que se hizo para `api_sica` en `8091`).
- `STAGE`/ambiente controla exposición de Swagger/OpenAPI — deshabilitado fuera de `dev` (mismo criterio OWASP que el precedente).
- Verificación post-deploy: logs del contenedor sin errores de conexión SQL/Vault, más un smoke test de los 8 endpoints (o al menos `/health` + un endpoint de cada tipo) contra la URL real del ambiente — no solo local.

**D10 — Fallos de negocio esperados: `Result<T>` desde el caso de uso, no excepción (post-auditoría, change `gestion-ludopatas-casos-uso-result`).**
Auditoría `dotnet-audit` (2026-08-10) encontró que la validación de negocio de los 8
casos de uso vivía en los adaptadores `Infrastructure/Sql/*`, lanzando
`ErrorFuncionalException` capturada por `ManejadorExcepcionesGlobal` — exactamente el
patrón que la rúbrica `dotnet-clean-style` §7 señala como el mecanismo equivocado para
un fallo esperado. Se introdujo `Application/Resultados/Result.cs` y una clase
`Manejador*` por caso de uso (`ManejadorCrearCorte`, `ManejadorResolverInicioCorte`, y
uno por cada una de las 6 búsquedas de pendientes) que valida y devuelve `Result<T>` de
éxito/fallo sin lanzar; los adaptadores SQL quedaron reducidos a ejecutar el SP y
mapear filas. `ErrorFuncionalException` sigue existiendo, acotada a lo que sí es
legítimamente excepcional: los middlewares de seguridad (`ApiKeyAuthenticationMiddleware`,
`IpAllowlistMiddleware`, corto-circuito transversal antes de llegar a un caso de uso) y
el conflicto de idempotencia de `crearCorte` (D7). Detalle completo, incluida la
restricción de DI descubierta durante la implementación (CALIMACO/CMP ingreso y salida
comparten el mismo `TRequest`/`TItem`, ver `IBuscarPendientesCalimacoIngreso`/`...Salida`/
`...CmpIngreso`/`...CmpSalida` en `Application/Pendientes/IBuscarPendientes.cs`) en
`openspec/changes/gestion-ludopatas-casos-uso-result/design.md`. Contrato HTTP sin
cambios — mismos códigos `GL-*`, mismo status por regla.

## Risks / Trade-offs

- [Crash entre `SP_CORTE_Crear` commit y publicación de idempotencia] → Mitigación parcial: el registro en memoria se publica inmediatamente después de la ejecución exitosa y antes de responder HTTP; ventana de riesgo mínima pero no eliminada. Documentado, no resuelto (limitación heredada del SP, no de la API).
- [Reinicio o más de una instancia durante el TTL] → El almacén en memoria no se comparte ni sobrevive al proceso. Mitigación actual: despliegue de una sola instancia. Mitigación requerida antes de escalar: tabla propia de la API con adquisición atómica por clave; no otorgar escritura sobre `bd_autobot`.
- [Un solo consumidor con API Key de larga vida] → Mitigación: rotación programada + IP allowlist como segunda barrera; si la key se filtra sin acceso a la red on-premise, no sirve.
- [6 SP de pendientes no reservan filas — dos llamadas concurrentes ven las mismas filas] → Fuera de alcance de la API (la fuente ya lo señala como limitación del SP); no se simula reserva a nivel HTTP porque introduciría estado que el SP no tiene.
- [Zona horaria `DATETIME` sin política aprobada] → Se serializa tal cual devuelve SQL Server con offset de servidor asumido (config explícita, sin fallback silencioso); bloqueante real si el servidor SQL y el host API no comparten zona horaria — ver Open Questions.
- [Vault caído en el arranque] → Mitigación intencional: fail-fast, igual que el precedente `api-sica` (D8). Preferible a arrancar con una connection string vacía o cacheada.
- [Ambiente compartido con `api-sica`] → Mitigación: puerto y nombre de contenedor propios, sin dependencias cruzadas de red más allá de compartir el mismo Vault (con path dedicado, D8) y, potencialmente, el mismo host Docker — nunca el mismo path de Vault ni la misma base de datos.

## Migration Plan

Rollout deliberado en dos fases (decisión explícita del usuario): validar que el
servicio funciona end-to-end en dev antes de introducir la dependencia de Vault.

**Fase 1 — dev con configuración plana:**
1. Provisionar login SQL Server dedicado (`bd_autobot`) con `GRANT EXECUTE` en los 8 SP (D5) — sin esto, la API no arranca (falla healthcheck).
2. Desplegar `GestionLudopatas.Api` como contenedor Docker en el mismo app server Linux on-premise donde ya corre `api-sica` (D9), con connection string y API Key como variables de entorno normales (sin Vault todavía).
3. Registrar la IP/subnet del host UiPath en el allowlist del ambiente antes de habilitar tráfico real.
4. Validar los 8 endpoints con la colección Postman ya existente en `endpoint/GestionLudopatas_Postman_Collection_v0.2.0-propuesta.json` (actualizar auth de OAuth2 a API Key antes de usarla) contra la URL real del contenedor desplegado en dev, no solo local.

**Fase 2 — cutover a Vault (D8):**
5. DevOps crea el path dedicado en Vault `api-gestionludopatas/dev/db` con los campos de D8 (mismo procedimiento operativo que se siguió para `api-sica/dev/db`, sin reutilizar ese path).
6. Redeploy en dev leyendo el secreto desde Vault en vez de variables de entorno planas; repetir la validación del paso 4 y confirmar los mismos resultados.
7. Repetir Fase 1 + Fase 2 para qa/prod cuando corresponda — no antes de cerrar dev.

**Rollback**: apagar/quitar el contenedor — no hay migración de datos de negocio (la API no posee `Corte`/`bitacora_transacciones`, solo su propia tabla de idempotencia).

## Open Questions

1. Política exacta de conversión de zona horaria para `DATETIME` SQL ↔ RFC 3339 HTTP (abierta también en el documento fuente, §16.5).
2. ¿La tabla de idempotencia vive en `bd_autobot` (requiere permiso de escritura adicional, fuera de D5) o en una base propia de la API? Por defecto: base propia, para no romper el principio de "solo EXECUTE" contra `bd_autobot`.
3. Timeout de comando SQL y política de reintento exactos (la fuente los deja como decisión pendiente del API owner, §16.3).
