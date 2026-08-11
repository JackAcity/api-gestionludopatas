# Diseño: DIP pragmática reforzada

## Decisiones

### D1 — El adaptador SQL→HTTP pertenece a API

`ErrorMapeoSql` recibe una señal técnica de SQL Server y produce una clasificación del
contrato HTTP. No expresa una invariante de negocio, por lo que vive en `Api/Errores`
junto con `ManejadorExcepcionesGlobal`, su único consumidor de producción.

### D2 — El cuerpo `application/problem+json` pertenece a API

`ProblemaDetalle` tiene nombres de serialización y semántica HTTP. El traductor de
`Result<T>` y el manejador global lo comparten en el borde API; Application conserva
solamente `ResultadoError`, que representa un fallo esperado sin construir una
respuesta.

### D3 — Guardia en dos niveles

La reflexión inspecciona constructores, retornos, parámetros, campos, propiedades y
genéricos de Application/Domain. Niega SQL/EF/mensajería y tipos HTTP concretos, pero
mantiene la excepción pragmática de `StatusCodes` aislado. Una comprobación adicional
de fuente detecta llamadas de borde que no aparecen en firmas, como `Results.*`.

### D4 — El proyecto único sigue siendo deliberado

La separación por carpetas de D1 del diseño principal se mantiene. La guardia
determinista compensa la falta de límites de assemblies sin introducir cuatro proyectos
para un servicio de ocho operaciones.

### D5 — El puerto de idempotencia es propiedad de API

La idempotencia de `POST /cortes` conserva `status` y cuerpo JSON para reproducir una
respuesta HTTP. Por eso `IIdempotencyStore` y sus tipos opacos viven en `Api/Idempotencia`,
no en Application. `CorteEndpoints` depende de ese puerto; `Infrastructure/Idempotencia`
aporta la implementación en memoria y el composition root realiza el binding.
