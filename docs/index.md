# Secure Software Delivery Core — índice v0.1

**Estado:** diseño y evaluación; no es una declaración de cumplimiento ni una implementación aprobada. **Fecha de corte:** 2026-08-11.

Este corpus separa un modelo de control portable de sus adaptadores. Un requisito dice qué riesgo debe controlarse; un adaptador explica cómo podría implementarse en GitHub, GitLab o Azure DevOps. Ninguna capacidad de una plataforma se convierte en requisito por sí sola.

| Área | Artefacto principal | Estado |
|---|---|---|
| Evidencia y fuentes | [registro de fuentes](sources/source-register.md) | Verificado v0.1 |
| Modelo portable | [modelo de control](architecture/control-model.md) | Propuesto |
| Catálogo verificable | [catálogo YAML](architecture/control-catalog.v0.1.yaml) | Propuesto |
| Riesgos | [modelo de amenazas](architecture/threat-model.md) | Propuesto |
| GitHub | [mapa de capacidades](github/platform-capability-map.md) | Parcialmente verificado |
| Evaluación | [`evals/`](../evals/) | Diseñado; no ejecutado |
| Agentes y DSL | [gobierno de agentes](architecture/agent-governance.md) | Propuesto |
| Decisión fundacional | [ADR-0001](adr/ADR-0001-platform-neutral-control-model.md) | Propuesto para aprobación |

La siguiente fase solo puede comenzar cuando se aprueben los supuestos abiertos, el catálogo, el modelo de amenazas y la matriz de evaluación. Los workflows existentes en la PR `#1` son un candidato no fusionado, no evidencia de que este modelo haya sido implementado o aprobado.
