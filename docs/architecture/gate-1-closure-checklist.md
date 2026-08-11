# Gate 1 — registro de cierre auditable

**Technical design:** `ACCEPT` (revisión independiente Gate 1.1, 2026-08-11).
**Overall Gate 1:** `HOLD` hasta resolver G1-08 y G1-09.
**Regla:** `PASS` prueba diseño suficiente; `HOLD` bloquea cierre; `DEFERRED` tiene
gate destino y no bloquea Gate 1; `N-A` no aplica al cierre técnico.

| ID | Criterio | Estado | Evidencia | Próximo responsable / gate |
|---|---|---|---|---|
| G1-01 | Registro de fuentes y autoridad | PASS | [Source Register](../sources/source-register.md), [NIST SSDF](../standards/nist-ssdf.md), [SLSA](../standards/slsa.md) | Revalidar en cada revisión de fuente. |
| G1-02 | Trazabilidad source → locator → decisión | PASS | [Matriz](source-to-control-traceability.v0.2.yaml) | Mantener al introducir control nuevo. |
| G1-03 | Modelo de amenazas | PASS | [Threat Model v0.2](threat-model.md), 18 rutas TM | Gate 2 ejecuta los casos seleccionados. |
| G1-04 | Modelo y contrato de evidencia | PASS | [Schema](evidence-bundle.schema.v0.1.json), [semántica](evidence-semantic-validation.v0.1.md), 5 vectores | Gate 2 implementa resolvers de plataforma si el vertical los necesita. |
| G1-05 | Estrategia y cobertura de evaluación | PASS | [Estrategia](../../evals/strategy.v0.2.md), [cobertura TM](../../evals/threat-to-evaluation-coverage.v0.1.yaml) | Gate 2 prioriza y ejecuta fixtures; no se usa un contador fijo como gate. |
| G1-06 | Mapa de capacidades GitHub | PASS | [Capability Map v0.2](../github/platform-capability-map.md), snapshot API 2026-08-11 | Revalidar justo antes de implementar cada adaptador. |
| G1-07 | Gobierno de agentes / SDC-007 | PASS | [Agent Governance](agent-governance.md), [trazabilidad SDC-007](source-to-control-traceability.v0.2.yaml) | Gate 2 aplica la política al vertical. |
| G1-08 | Clasificación Gitleaks | HOLD | [Registro sanitizado](../security/gitleaks-classification.v0.1.md) contiene siete `candidate-false-positive`. | Humano autorizado: confirmar o remediar/rotar sin copiar secretos. |
| G1-09 | Autorización IP y publicación pública | HOLD | [PRRG-001](public-reference-release-gate.md), OQ-008 | Dueño legal/negocio decide autorización, sanitización o retiro. |
| G1-10 | Retención, ACL, ubicación y borrado operativos | DEFERRED | [Evidence Model](evidence-model.md) declara el límite. | Gate 2 / decisión de plataforma antes de producir evidencia sensible. |
| G1-11 | OIDC, runner topology, artifact registry y trust provider | DEFERRED | [Open Questions](open-questions.md), [Capability Map](../github/platform-capability-map.md) | Gate 2 — Minimum Vertical Design. |
| G1-12 | Descripción, licencia y topics de portafolio | DEFERRED | PRRG-001 incluye la precondición. | Cierre de PRRG-001, antes de claim de referencia pública. |
| G1-13 | PR #1 congelado | PASS | PR #1 permanece draft, separado y sin merge. | Mantener hasta autorización de implementación. |
| G1-14 | Aprobación independiente del diseño técnico | PASS | Veredicto Gate 1.1 registrado en el cuerpo de PR #2 y este registro. | Arquitecto/seguridad reabre solo ante cambio material. |

## Cierre permitido

Cuando G1-08 y G1-09 tengan evidencia humana autorizada, se actualizan esas filas a
`PASS` o se registra una remediación. Solo entonces un humano puede declarar `OVERALL
GATE 1: ACCEPT`; este documento no se autocierra por herramientas ni agentes.
