## Context

`GestionLudopatas.Api` es un único proyecto (`.csproj`) con capas por carpeta —
decisión D1 de `openspec/changes/gestion-ludopatas-api/design.md`: escala minimal, no
justifica 4 `.csproj` separados. El usuario pidió explícitamente que la organización
física sea "rígida, como la arquitectura empresarial de referencia"
(`reto_tecnico_backend_senior`, que sí separa `Domain`/`Application`/`Infrastructure`/`Api`
como proyectos). Este change traduce esa rigidez al nivel de carpeta física que
corresponde a la escala minimal (D1), sin volver a proponer separar en `.csproj`
(eso seguiría siendo YAGNI para 8 endpoints — no cambia el análisis de D1).

## Goals / Non-Goals

**Goals:**
- Ninguna carpeta vacía bajo `src/GestionLudopatas.Api/`.
- Toda la capa de entrada HTTP (Endpoints, Middleware, Security) vive bajo una única
  carpeta `Api/`, reflejando el límite que en la escala enterprise sería el proyecto
  `<Servicio>.Api`.
- `Program.cs` es composition root puro — arma el pipeline, no contiene lógica de
  bootstrap.

**Non-Goals:**
- No se separa en múltiples `.csproj` (sigue siendo YAGNI para este alcance, D1 del
  change original se mantiene vigente).
- No cambia el namespace de ningún tipo movido — ver D1 de este design.md.
- No cambia el contrato HTTP ni el orden del pipeline de middleware.
- No toca `Domain/Errores` ni `Application/Errores` (esas carpetas sí están pobladas y
  con separación intencional — Domain = datos puros GL-*, Application = excepción/DTO
  de nivel caso de uso).

## Decisions

**D1 — Mover `Endpoints/`, `Middleware/`, `Security/` bajo `Api/` sin cambiar
namespace.**
El namespace raíz del proyecto ya es `GestionLudopatas.Api`. Si el namespace de un
tipo en `Api/Endpoints/CorteEndpoints.cs` pasara a `GestionLudopatas.Api.Api.Endpoints`,
se duplica "Api" — ruido, no señal. Se prioriza la carpeta física (lo que el usuario
pidió: orden visual, rigidez estructural) sobre la convención de que namespace = ruta
de carpeta exacta; el namespace se queda como `GestionLudopatas.Api.Endpoints`,
`.Middleware`, `.Security` — sin cambios en código que los referencia (`using`
statements intactos).
- Alternativa considerada: namespace `GestionLudopatas.Api.Api.*` — descartada, el
  stutter no aporta información y ensucia cada `using`.
- Alternativa considerada: no usar la palabra "Api" para la carpeta (ej. `Presentacion/`)
  — descartada explícitamente por el usuario: quiere el vocabulario alineado 1:1 con
  el proyecto de referencia, donde esa capa se llama `*.Api`.

**D2 — Bootstrap de Vault sale de `Program.cs` a
`Infrastructure/Vault/VaultBootstrapExtensions.cs`.**
Método de extensión `public static async Task CargarSecretosSiHabilitadoAsync(this
WebApplicationBuilder builder)` — mismo contenido que hoy vive inline en `Program.cs`
(líneas 14-37: leer `Vault:Habilitado`, resolver `Vault:Address`/`Token`/`PathDb`/
`PathApiKey`, llamar a `VaultSecretClient`, escribir `ConnectionStrings:BdAutobot` y
`Seguridad:ApiKey` en `builder.Configuration`). `Program.cs` pasa a tener una sola
línea para esto: `await builder.CargarSecretosSiHabilitadoAsync();`. Vive en
`Infrastructure/Vault` porque ya es donde vive `VaultSecretClient`/`VaultHttpClientFactory`
— mismo criterio de cohesión (D8 del change original: todo lo de Vault en un solo
lugar).

**D3 — Carpetas vacías se eliminan, no se documentan como decisión de diseño.**
`Application/Seguridad`, `Domain/Cortes`, `Domain/Pendientes` no representan ninguna
decisión — son residuo de un scaffold temprano. No aplica el formato de "decisión
documentada" (§11 de la rúbrica) porque no hay nada que decidir: simplemente no se
usaron.

## Risks / Trade-offs

- [Mover archivos rompe imports/rutas relativas en tooling externo (scripts, CI, IDE
  bookmarks)] → Mitigación: este proyecto no tiene CI configurado todavía (ver
  `openspec/changes/gestion-ludopatas-api/design.md`, sin alcance cloud); el único
  consumidor de rutas es el propio repo. Verificar `dotnet build` + `dotnet test`
  verdes después del movimiento cubre el caso real de riesgo.
- [Extraer el bootstrap de Vault a un método async de extensión sobre
  `WebApplicationBuilder` es un patrón menos común que verlo inline] → Mitigación: es
  exactamente el mismo patrón que ya usa `AddPersistenciaSql()` (extensión sobre
  `IServiceCollection`) — coherente con el resto del proyecto, no una técnica nueva.

## Migration Plan

Movimiento de archivos + extracción, sin pasos de despliegue (no hay dato ni estado
externo involucrado). Orden:
1. Crear `Api/Endpoints/`, `Api/Middleware/`, `Api/Security/`; mover los 6 archivos de
   producción (namespace intacto).
2. Mover los archivos de test espejo a `test/.../Api/Endpoints/`, `test/.../Api/Security/`.
3. Crear `Infrastructure/Vault/VaultBootstrapExtensions.cs`; recortar `Program.cs`.
4. Eliminar las 3 carpetas vacías.
5. `dotnet build` + `dotnet test` verdes (línea base 76/76 antes de este change).
6. Actualizar rutas referenciadas en `openspec/changes/gestion-ludopatas-casos-uso-result/`
   (`design.md`, `tasks.md`) que hoy dicen `Endpoints/...`/`Middleware/...` para que
   digan `Api/Endpoints/...`/`Api/Middleware/...` — este change se aplica primero.

**Rollback**: revertir el commit del movimiento — no hay estado externo ni datos de
negocio involucrados.

## Open Questions

Ninguna — alcance acotado y sin ambigüedad de negocio.
