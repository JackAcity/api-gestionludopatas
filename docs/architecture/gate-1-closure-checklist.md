# Gate 1 — lista de cierre

**Estado:** abierto. El cumplimiento de esta lista no autoriza implementación por sí
solo: requiere aprobación humana registrada.

- [ ] Source Register actualizado y cada fuente material clasificada por autoridad/estado.
- [ ] Matriz source-to-control con locator, interpretación local y decisión separadas.
- [ ] Las recomendaciones finales de NIST están diferenciadas de requisitos locales
      adoptados; los `MUST` condicionales de SLSA indican el nivel que los activa.
- [ ] Threat Model v0.2 revisado con familias SCM, Actions, dependencias, runners,
      agentes, artefactos, evidencia y BD; cada ruta tiene control y familia EVF.
- [ ] Evidence Bundle Schema y validador semántico tienen vectores reproducibles válidos,
      attestation sin digest, verificador ausente e identidades no independientes.
- [ ] Diseño de resolvers de SCM, identidad, workflow/run, registro y firma deja claro que
      `content_digest` no establece autenticidad por sí solo.
- [ ] Retención, ACL, borrado y ubicación reciben decisión organizacional.
- [ ] Estrategia de evaluación usa cobertura de amenazas/modos de fallo, no un contador
      como trust gate; el [plan de cobertura](../../evals/threat-to-evaluation-coverage.v0.1.yaml)
      asigna toda ruta TM a control/precondición, familia y caso planificado.
- [ ] Registro sanitizado de Gitleaks recibe confirmación humana o remediación.
- [ ] Capability Map sincronizado con la condición pública del repositorio y API snapshots.
- [ ] SDC-007 se acepta explícitamente como decisión local sustentada, no como requisito
      NIST final.
- [ ] Dueño legal/negocio decide si el material público contiene propiedad intelectual,
      información operativa o contratos que deban sanitizarse.
- [ ] Public Reference Release Gate (PRRG-001) obtiene evidencia y aprobación para todos
      los elementos aplicables antes de cualquier claim de referencia pública.
- [ ] Dueño del repositorio define descripción, licencia, topics y aviso de referencia.
- [ ] PR #1 sigue congelado; no se usa como evidencia de aprobación de arquitectura.
- [ ] Arquitecto/seguridad aprueba o rechaza Gate 1 y registra el veredicto.
