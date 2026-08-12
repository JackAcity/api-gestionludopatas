# Preguntas abiertas y decisiones humanas

| ID | Pregunta / supuesto | Impacto | Decisor | Estado |
|---|---|---|---|---|
| OQ-001 | El repo ya es público; ¿qué capacidades adicionales de GHAS, audit/export, runner groups y del futuro target privado/enterprise están licenciadas? | No se puede extrapolar el demo público al entorno de cliente. | Owner GitHub. | DEFERRED — Gate 2 |
| OQ-002 | ¿Quiénes son revisores independientes y release authorities? | No se puede imponer separación de deberes con una sola identidad. | Liderazgo. | DEFERRED — Gate 2 |
| OQ-003 | ¿Dónde ejecutará el runner de confianza y cómo se parchea/audita? | Riesgo crítico de red on-premise y Docker. | Infra/Seguridad. | DEFERRED — Gate 2 |
| OQ-004 | ¿Vault/on-premise soporta OIDC GitHub y qué claims acepta? | Define identidad de deploy y elimina/no elimina token persistente. | Vault/Infra. | DEFERRED — Gate 2 |
| OQ-005 | ¿Cuál es registro de artefactos y consumidor de provenance/SBOM? | Sin consumidor no hay control SDC-006 efectivo. | Plataforma. | DEFERRED — Gate 2 |
| OQ-006 | ¿Qué RPO/RTO, SLI/SLO y destino de observabilidad aplican? | Sin ello no se diseña recuperación ni gates de calidad. | Operaciones/negocio. | DEFERRED — Gate 2 |
| OQ-007 | ¿Qué cambios DB pueden automatizarse y cuáles requieren pase DBA? | Afecta rollback y autorización. | DBA/Arquitectura. | DEFERRED — Gate 2 |
| OQ-008 | ¿El código, contratos, nombres de dominio e información operativa proceden de un empleador/cliente y pueden publicarse legal y contractualmente? | Puede exigir sanitizar, bifurcar o retirar este demo público. | Dueño legal/negocio. | RESUELTO PARA GATE 1 — [autorización temporal](gate-1-human-closure-2026-08-12.md); revalidar PRRG-001 antes de producción, reutilización o claim de referencia pública. |
| OQ-009 | ¿Qué descripción, licencia, topics y aviso de referencia debe tener el repositorio público? | Define expectativas de reutilización y evita presentarlo como producción/compliance. | Dueño del repositorio. | DEFERRED — PRRG-001 |

**Hallazgo de preparación:** un escaneo local completo de historial con Gitleaks v8.30.1 reportó siete coincidencias. Su [registro sanitizado](../security/gitleaks-classification.v0.1.md) y la [decisión humana](gate-1-human-closure-2026-08-12.md) registran la confirmación de GL-001 a GL-007 sin copiar contenido a issues o logs. Esto no permite afirmar que el repositorio esté libre de secretos sin un reescaneo posterior.
