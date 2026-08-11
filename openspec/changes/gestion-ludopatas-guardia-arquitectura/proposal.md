## Why

Auditoría `dotnet-audit` sobre `GestionLudopatas.Api` (2026-08-10, hallazgo #4) encontró
que el proyecto no tiene ninguna prueba automatizada que verifique la regla DIP central
de la rúbrica `dotnet-clean-style` (§5, §10): `Application`/`Domain` nunca deben
referenciar un tipo concreto de infraestructura (`Microsoft.Data.SqlClient`, EF Core,
`RabbitMQ.Client`, etc.). Hoy esa regla se cumple (verificado manualmente durante la
auditoría — ningún archivo bajo `Application/`/`Domain/` importa `Microsoft.Data.SqlClient`),
pero no hay nada que la proteja de una regresión futura: un desarrollador podría, sin
mala intención, agregar un `using Microsoft.Data.SqlClient;` en un puerto de
`Application` y nadie lo notaría hasta un problema de acoplamiento más adelante. La
rúbrica exige esta guardia como parte del build, no como revisión manual.

## What Changes

- Se agrega `ArquitecturaTests.cs` en el proyecto de test (xUnit, mismo framework que
  el resto de la suite) con una prueba por reflection: enumera todos los tipos del
  ensamblado principal cuyo namespace empieza con `GestionLudopatas.Api.Application`
  o `GestionLudopatas.Api.Domain`, e inspecciona constructores, métodos, propiedades y
  campos de cada uno — falla si algún miembro referencia un tipo de un namespace
  prohibido (`Microsoft.Data.SqlClient`, `System.Data.SqlClient`, `Npgsql`,
  `Microsoft.EntityFrameworkCore`, `RabbitMQ.Client`).
- Es una prueba puramente aditiva — no cambia código de producción, solo agrega
  cobertura.
- Escala minimal de este proyecto (un solo `.csproj`, D1 del change original): la
  guardia no puede verificar referencias de assembly (no hay assemblies separados por
  capa) — verifica por namespace vía reflection sobre los tipos ya cargados, mismo
  resultado práctico que la versión "referencias de proyecto" que usaría la escala
  enterprise.

## Capabilities

### New Capabilities
- `guardia-arquitectura-dip`: prueba determinística que falla el build si
  `Application`/`Domain` referencia un tipo concreto de infraestructura, protegiendo
  la regla DIP (§5 de `dotnet-clean-style`) de regresión futura.

### Modified Capabilities
(ninguna — no cambia comportamiento de negocio ni contrato HTTP.)

## Impact

- **Código nuevo**: `test/GestionLudopatas.Api.Tests/Arquitectura/ArquitecturaTests.cs`.
- **Sin cambios en código de producción.**
- **Independiente de otros changes en curso** (`gestion-ludopatas-limpieza-estructura`,
  `gestion-ludopatas-casos-uso-result`) — no depende de ellos ni ellos de este; puede
  aplicarse en cualquier orden. Recomendado aplicarlo después de
  `gestion-ludopatas-limpieza-estructura` únicamente por prolijidad del árbol de test
  (no por dependencia real).
