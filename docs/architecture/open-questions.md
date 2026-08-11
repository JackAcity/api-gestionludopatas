# Preguntas abiertas y decisiones humanas

| ID | Pregunta / supuesto | Impacto | Decisor |
|---|---|---|---|
| OQ-001 | ¿Qué plan GitHub tiene el repo y qué capacidades privadas están licenciadas? | Environments, reviewers, attestations, GHAS. | Owner GitHub. |
| OQ-002 | ¿Quiénes son revisores independientes y release authorities? | No se puede imponer separación de deberes con una sola identidad. | Liderazgo. |
| OQ-003 | ¿Dónde ejecutará el runner de confianza y cómo se parchea/audita? | Riesgo crítico de red on-premise y Docker. | Infra/Seguridad. |
| OQ-004 | ¿Vault/on-premise soporta OIDC GitHub y qué claims acepta? | Define identidad de deploy y elimina/no elimina token persistente. | Vault/Infra. |
| OQ-005 | ¿Cuál es registro de artefactos y consumidor de provenance/SBOM? | Sin consumidor no hay control SDC-006 efectivo. | Plataforma. |
| OQ-006 | ¿Qué RPO/RTO, SLI/SLO y destino de observabilidad aplican? | Sin ello no se diseña recuperación ni gates de calidad. | Operaciones/negocio. |
| OQ-007 | ¿Qué cambios DB pueden automatizarse y cuáles requieren pase DBA? | Afecta rollback y autorización. | DBA/Arquitectura. |

**Hallazgo de preparación:** un escaneo local completo de historial con Gitleaks v8.30.1 reportó siete coincidencias. El resultado fue sanitizado y no se considera secreto confirmado: debe clasificarse en fixture, placeholder/documentación o exposición real antes de afirmar que el repositorio está limpio. No se debe copiar el contenido de las coincidencias a issues o logs.
