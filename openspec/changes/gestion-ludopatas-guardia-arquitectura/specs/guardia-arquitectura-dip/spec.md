## ADDED Requirements

### Requirement: Ningún tipo de Application o Domain referencia infraestructura concreta
Ningún tipo (público o no público) cuyo namespace comience con `GestionLudopatas.Api.Application` o `GestionLudopatas.Api.Domain` SHALL tener un constructor, método, propiedad o campo cuyo tipo pertenezca a `Microsoft.Data.SqlClient`, `System.Data.SqlClient`, `Npgsql`, `Microsoft.EntityFrameworkCore` o `RabbitMQ.Client`.

#### Scenario: Suite verde con el estado actual del código
- **WHEN** se ejecuta la prueba de guardia de arquitectura contra el código actual de `GestionLudopatas.Api`
- **THEN** la prueba pasa, porque ningún tipo de `Application`/`Domain` referencia hoy esos namespaces

#### Scenario: Regresión futura detectada
- **WHEN** un tipo bajo `GestionLudopatas.Api.Application.*` o `GestionLudopatas.Api.Domain.*` agrega un miembro cuyo tipo pertenece a `Microsoft.Data.SqlClient` (u otro namespace prohibido)
- **THEN** la prueba de guardia de arquitectura falla, identificando el tipo y miembro infractor

### Requirement: La guardia corre como parte de `dotnet test` sin configuración adicional
La prueba SHALL usar únicamente `System.Reflection` (ya disponible en el BCL) y el framework de test ya usado por el proyecto (xUnit) — SHALL NOT requerir un paquete NuGet nuevo ni un paso de build separado.

#### Scenario: Ejecución estándar
- **WHEN** se corre `dotnet test` sobre `test/GestionLudopatas.Api.Tests`
- **THEN** la prueba de guardia de arquitectura se ejecuta junto con el resto de la suite, sin pasos ni comandos adicionales
