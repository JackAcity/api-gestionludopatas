# ADR-0001 — Modelo de control portable antes de adaptadores

**Estado:** propuesto.

**Decisión:** el núcleo describe riesgos, requisitos, controles, evidencia y verificaciones sin terminología de GitHub. GitHub es el primer adaptador; una regla de plataforma nunca redefine el requisito.

**Contexto:** la mezcla de “usar Rulesets” con “proteger la integración confiable” hace imposible comparar capacidades de GitLab/Azure DevOps, medir huecos y auditar si un control protege su riesgo real.

**Consecuencias:** cada implementación debe referenciar un `control_id`, producir su evidencia esperada y pasar su caso de evaluación. Se mantiene un mapa de capacidades y un registro de excepciones. Aumenta documentación inicial, pero evita acoplar decisiones arquitectónicas a un proveedor. Alternativa descartada: documentar solo YAML de Actions; no permite trazabilidad ni portabilidad.
