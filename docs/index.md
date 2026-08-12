# Secure Software Delivery Core — índice v0.2

**Estado:** `TECHNICAL DESIGN: ACCEPT`; `OVERALL GATE 1: HOLD` por Gitleaks e IP/autorización humana. No es una declaración de cumplimiento ni una implementación aprobada. **Fecha de corte:** 2026-08-11.

Este corpus separa un modelo de control portable de sus adaptadores. Un requisito dice qué riesgo debe controlarse; un adaptador explica cómo podría implementarse en GitHub, GitLab o Azure DevOps. Ninguna capacidad de una plataforma se convierte en requisito por sí sola.

| Área | Artefacto principal | Estado |
|---|---|---|
| Evidencia y fuentes | [registro de fuentes](sources/source-register.md) y [trazabilidad](architecture/source-to-control-traceability.v0.2.yaml) | Verificado / propuesto |
| Modelo portable | [modelo de control](architecture/control-model.md) | Propuesto |
| Catálogo verificable | [catálogo YAML](architecture/control-catalog.v0.1.yaml) | Propuesto |
| Riesgos | [modelo de amenazas v0.2](architecture/threat-model.md) | Propuesto |
| Evidencia | [modelo](architecture/evidence-model.md) y [schema](architecture/evidence-bundle.schema.v0.1.json) | Propuesto |
| GitHub | [mapa de capacidades v0.2](github/platform-capability-map.md) | Parcialmente verificado |
| Evaluación | [matriz v0.2](../evals/matrix.v0.2.yaml), [estrategia](../evals/strategy.v0.2.md), [cobertura TM](../evals/threat-to-evaluation-coverage.v0.1.yaml) y [DoR](../evals/evaluation-definition-of-ready.v0.1.md) | Diseñado; no ejecutado |
| Agentes y DSL | [gobierno de agentes](architecture/agent-governance.md) | Propuesto |
| Publicación | [Public Reference Release Gate](architecture/public-reference-release-gate.md) | HOLD humano |
| Decisión fundacional | [ADR-0001](adr/ADR-0001-platform-neutral-control-model.md) | Propuesto para aprobación |

El Gate 1 permanece abierto hasta completar la [lista de cierre](architecture/gate-1-closure-checklist.md).

La siguiente fase solo puede comenzar cuando se resuelvan los HOLD humanos del registro.
La implementación CI/CD de la [PR #1](https://github.com/JackAcity/api-gestionludopatas/pull/1) fue fusionada antes del cierre de Gate 1; es un baseline observado que Gate 2 debe reevaluar, no evidencia de que este modelo haya sido implementado o aprobado.
