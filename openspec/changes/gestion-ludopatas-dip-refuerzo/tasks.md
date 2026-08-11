## 1. Borde de errores

- [x] 1.1 Mover `ErrorMapeoSql`/`ErrorMapeo` de Domain a API y mantener el contrato de mapeo SQL→HTTP.
- [x] 1.2 Mover `ProblemaDetalle` y `ErrorFuncionalException` de Application a API; conservar `ResultadoError` como el único error de caso de uso en Application.
- [x] 1.3 Actualizar endpoints, middleware y pruebas sin cambiar status, código ni cuerpo HTTP.

## 2. Guardia

- [x] 2.1 Prohibir tipos HTTP concretos en Application/Domain y demostrar una infracción controlada HTTP (`HttpContext`).
- [x] 2.2 Detectar en código fuente llamadas de borde no visibles por reflexión, como `Results.*`, con muestra negativa controlada.
- [x] 2.3 Hacer que `CorteEndpoints` dependa del puerto `IIdempotencyStore`, no de la implementación concreta; el binding DI tiene prueba dedicada.

## 3. Verificación

- [x] 3.1 Pruebas de arquitectura incluidas en suite completa: 118/118 verdes.
- [x] 3.2 `dotnet build api-gestionludopatas.slnx --no-restore -warnaserror --verbosity minimal`: 0 warnings, 0 errors.
