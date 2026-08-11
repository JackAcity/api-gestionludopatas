# Refuerzo de DIP en límites Core/API

## Problema

La inversión de dependencias de los ocho casos de uso hacia sus puertos SQL funciona,
pero tres elementos de borde están ubicados o protegidos de manera incompleta:

- `Domain/Errores/ErrorMapeoSql` mezcla números de SQL Server con estados HTTP.
- `Application/Errores/ProblemaDetalle` modela directamente `application/problem+json`.
- La guardia de arquitectura niega SQL/EF/mensajería, pero no mecanismos HTTP ni
  llamadas estáticas como `Results.*` en Application/Domain.

## Cambio propuesto

Mover el mapeo SQL→HTTP, la excepción funcional residual del borde y el cuerpo
`problem+json` a `Api/Errores`; mantener en Application/Domain solo casos de uso,
puertos, resultados de negocio y códigos estables. Extender la guardia con tipos HTTP
prohibidos y una comprobación de código fuente para las llamadas estáticas de borde.
Extraer también el puerto de almacenamiento de idempotencia en API, para que el endpoint
no dependa de su implementación en memoria.

## Fuera de alcance

- Partir el monolito en varios proyectos/assemblies.
- Cambiar contratos HTTP, códigos `GL-*` o la semántica de Result.
- Cambiar la semántica, TTL o estrategia de idempotencia vigente.
