## Context

`dotnet-clean-style` §10 exige una prueba automatizada (reflection sobre los
assemblies ya referenciados, sin dependencia nueva — YAGNI) que falle si
`Application`/`Domain` referencia infraestructura concreta. El proyecto de referencia
enterprise (`reto_tecnico_backend_senior`) puede verificar esto por **referencias de
assembly** (Domain/Application son proyectos separados que ni siquiera podrían
compilar si referenciaran `Infrastructure`). `GestionLudopatas.Api` es un solo
`.csproj` (D1) — todos los tipos comparten un único assembly, así que la verificación
tiene que ser por **namespace + inspección de miembros vía reflection**, no por
referencia de proyecto.

## Goals / Non-Goals

**Goals:**
- Un test falla si cualquier tipo público o privado bajo `GestionLudopatas.Api.Application.*`
  o `GestionLudopatas.Api.Domain.*` tiene un miembro (constructor, método, propiedad,
  campo) cuyo tipo pertenece a un namespace de infraestructura prohibido.
- El test corre como parte de `dotnet test` normal — sin configuración adicional, sin
  paquete NuGet nuevo (reflection es `System.Reflection`, ya en el BCL).

**Non-Goals:**
- No se adopta `ArchUnitNET` todavía — la rúbrica (§10) lo deja como alternativa para
  cuando el equipo crezca y la prueba reflection casera empiece a pesar; con 2
  namespaces y un puñado de tipos, sigue siendo la opción más simple (YAGNI/KISS).
- No verifica otras reglas SOLID (SRP, ISP) — alcance acotado a DIP, que es lo que
  pide §10 explícitamente.
- No bloquea el uso de `Microsoft.AspNetCore.Http.StatusCodes` (constantes de status
  HTTP) en `Domain`/`Application` — son constantes de vocabulario HTTP estándar, no
  una dependencia de infraestructura intercambiable (no es el tipo de acoplamiento que
  DIP busca prevenir); ya se usan hoy en `Domain/Errores/ErrorMapeoSql.cs` y es
  aceptable.

## Decisions

**D1 — Lista de namespaces prohibidos, explícita y corta, no heurística genérica.**
`Microsoft.Data.SqlClient`, `System.Data.SqlClient`, `Npgsql`,
`Microsoft.EntityFrameworkCore`, `RabbitMQ.Client` — los mismos que nombra la rúbrica
§5 literalmente, más los que ya son relevantes por el stack real de este proyecto
(`Microsoft.Data.SqlClient`, el driver SQL en uso). No se prohíbe todo
`Microsoft.Extensions.*`/`Microsoft.AspNetCore.*` en bloque — eso incluiría tipos que
son parte legítima del vocabulario de `Application` en un proyecto Minimal API
(`IConfiguration`, `CancellationToken`, `StatusCodes`) y generaría falsos positivos.

**D2 — Reflection sobre tipos del assembly ya cargado, no análisis estático de
código fuente.**
El test usa `typeof(Result<>).Assembly.GetTypes()` (el ensamblado de
`GestionLudopatas.Api` referenciado desde el proyecto de test) filtrado por
`Namespace?.StartsWith("GestionLudopatas.Api.Application")` /
`"GestionLudopatas.Api.Domain"`, e inspecciona `ConstructorInfo`/`MethodInfo`/
`PropertyInfo`/`FieldInfo` de cada tipo con `BindingFlags` que incluyan miembros
privados (una referencia oculta en un campo privado también viola DIP). Alternativa
considerada: analizador Roslyn — descartado, mucho más esfuerzo para el mismo
resultado a esta escala (YAGNI, mismo criterio que §10 de la rúbrica).

La guardia inspecciona dependencias declaradas en metadata (tipos de miembros), no
directivas `using`: un `using Microsoft.Data.SqlClient;` no usado no se emite al
assembly y por tanto no es observable por reflection. La prueba negativa usa un tipo de
fixture con un miembro privado `SqlConnection`, que verifica el comportamiento real que
la guardia promete proteger.

**D3 — Ubicación del test: `test/GestionLudopatas.Api.Tests/Arquitectura/`.**
Carpeta nueva, dedicada — no mezclada con `Application/`/`Domain/`/`Infrastructure/`
del proyecto de test (que espejan la estructura de producción por feature). La
guardia de arquitectura es transversal, no pertenece a ninguna feature — mismo
criterio que tendría un archivo `ArchitectureTests.cs` a nivel raíz en el proyecto de
referencia enterprise.

## Risks / Trade-offs

- [Falsos positivos si un tipo de `Application`/`Domain` usa un tipo genérico de BCL
  que "parece" infraestructura pero no lo es] → Mitigación: lista de namespaces
  prohibidos explícita y corta (D1), no un patrón amplio tipo "cualquier cosa fuera de
  `System.*`".
- [El test no detecta acoplamiento indirecto — ej. un método que recibe
  `IReadOnlyDictionary<string,string>` construido en `Infrastructure` pero pasado como
  parámetro genérico] → Aceptado: fuera de alcance, DIP es sobre el TIPO de la
  dependencia declarada, no sobre de dónde viene el valor en runtime — mismo alcance
  que tendría la verificación por referencia de assembly en la escala enterprise.

## Migration Plan

Sin despliegue — es una prueba nueva. Se agrega, se corre `dotnet test`, se confirma
verde (no debería fallar: la auditoría ya verificó manualmente que hoy no hay
violaciones).

## Open Questions

Ninguna.
