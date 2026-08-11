# Mapa GitHub v0.1

| Control | Capacidad candidata | Hecho verificado / límite | Decisión pendiente |
|---|---|---|---|
| SDC-001 | Rulesets | GitHub documenta PR, status checks, firmas, rutas y bypass. | Seleccionar reglas y bypass tras evaluar equipo/plan. |
| SDC-003 | `permissions`, SHA, Dependabot, triggers | GitHub recomienda mínimo privilegio y actualizar acciones. | Política exacta de acciones permitidas y evidencia API. |
| SDC-004 | Environments/concurrency | Environments tienen protecciones; reviewers en privados dependen del plan. | Confirmar plan y quién puede aprobar sin autoaprobación. |
| SDC-005 | OIDC | Requiere `id-token: write`; trust debe restringir claims. | Confirmar Vault/cloud, claims y cuenta de servicio. |
| SDC-006 | Attestations/SBOM | Attestation requiere verificación; privados dependen del plan para varios planes. | Plan/licencia y consumidor que hará enforcement. |
| SDC-007 | CODEOWNERS/ruleset/audit log | Puede soportar revisión, no independencia semántica de agentes por sí sola. | Identidades de agentes y separación de roles. |

**No verificado aún:** capacidad/licencia exacta de este repositorio, rulesets existentes, retención, audit log exportable, runner groups y disponibilidad de GHAS/secret scanning. No se debe afirmar que una capacidad está activa hasta obtener snapshot/API y ejecutar un caso de evaluación.
