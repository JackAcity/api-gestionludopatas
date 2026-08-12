# Gate 1 — registro de cierre auditable

**Technical design:** `ACCEPT` (revisión independiente Gate 1.1, 2026-08-11).
**Overall Gate 1:** `ACCEPT` (decisión humana registrada el 2026-08-12).
**Regla:** `PASS` prueba diseño suficiente; `HOLD` bloquea cierre; `DEFERRED` tiene
gate destino y no bloquea Gate 1; `N-A` describe una implementación histórica fuera del
alcance de validación de Gate 1.

| ID | Criterio | Estado | Evidencia | Próximo responsable / gate |
|---|---|---|---|---|
| G1-01 | Registro de fuentes y autoridad | PASS | [Source Register](../sources/source-register.md), [NIST SSDF](../standards/nist-ssdf.md), [SLSA](../standards/slsa.md) | Revalidar en cada revisión de fuente. |
| G1-02 | Trazabilidad source → locator → decisión | PASS | [Matriz](source-to-control-traceability.v0.2.yaml) | Mantener al introducir control nuevo. |
| G1-03 | Modelo de amenazas | PASS | [Threat Model v0.2](threat-model.md), 18 rutas TM | Gate 2 ejecuta los casos seleccionados. |
| G1-04 | Modelo y contrato de evidencia | PASS | [Schema](evidence-bundle.schema.v0.1.json), [semántica](evidence-semantic-validation.v0.1.md), 5 vectores | Gate 2 implementa resolvers de plataforma si el vertical los necesita. |
| G1-05 | Estrategia y cobertura de evaluación | PASS | [Estrategia](../../evals/strategy.v0.2.md), [cobertura TM](../../evals/threat-to-evaluation-coverage.v0.1.yaml) | Gate 2 prioriza y ejecuta fixtures; no se usa un contador fijo como gate. |
| G1-06 | Mapa de capacidades GitHub | PASS | [Capability Map v0.2](../github/platform-capability-map.md), snapshot API 2026-08-11 | Revalidar justo antes de implementar cada adaptador. |
| G1-07 | Gobierno de agentes / SDC-007 | PASS | [Agent Governance](agent-governance.md), [trazabilidad SDC-007](source-to-control-traceability.v0.2.yaml) | Gate 2 aplica la política al vertical. |
| G1-08 | Clasificación Gitleaks | PASS | [Registro sanitizado](../security/gitleaks-classification.v0.1.md) y [decisión humana](gate-1-human-closure-2026-08-12.md): GL-001 a GL-007 confirmados como falsos positivos. | Reescaneo ante cambios y remediar/rotar si cambia la clasificación. |
| G1-09 | Autorización IP y publicación pública | PASS | [Decisión humana](gate-1-human-closure-2026-08-12.md): autorización temporal para mantener este repositorio público. | Revalidar ante cambio de alcance, producción o claim de referencia pública. |
| G1-10 | Retención, ACL, ubicación y borrado operativos | DEFERRED | [Evidence Model](evidence-model.md) declara el límite. | Gate 2 / decisión de plataforma antes de producir evidencia sensible. |
| G1-11 | OIDC, runner topology, artifact registry y trust provider | DEFERRED | [Open Questions](open-questions.md), [Capability Map](../github/platform-capability-map.md) | Gate 2 — Minimum Vertical Design. |
| G1-12 | Descripción, licencia y topics de portafolio | DEFERRED | PRRG-001 incluye la precondición. | Cierre de PRRG-001, antes de claim de referencia pública. |
| G1-13 | Implementación CI/CD pre-Gate | N-A | [PR #1](https://github.com/JackAcity/api-gestionludopatas/pull/1) fue fusionado el 2026-08-11; CI/DevSecOps del `main` actual registran éxito. | Gate 2 debe reevaluar el baseline contra controles aprobados; checks verdes y el merge no son evidencia de cierre de Gate 1. |
| G1-14 | Aprobación independiente del diseño técnico | PASS | Veredicto Gate 1.1 registrado en el cuerpo de PR #2 y este registro. | Arquitecto/seguridad reabre solo ante cambio material. |

## Cierre registrado

La [decisión humana del 2026-08-12](gate-1-human-closure-2026-08-12.md) confirmó los
siete hallazgos Gitleaks y autorizó temporalmente la visibilidad pública. Por ello el
Gate 1 queda aceptado. Esta aceptación no transforma los elementos `DEFERRED` en
controles operativos ni autoriza por sí sola un despliegue, una promoción productiva o
