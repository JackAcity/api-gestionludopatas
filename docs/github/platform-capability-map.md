# Mapa GitHub v0.2

**Snapshot de repositorio:** 2026-08-11, API GitHub para
`JackAcity/api-gestionludopatas`. Este documento separa capacidad de plataforma,
configuración observada y control validado.

| Control | Capacidad candidata | Hecho de plataforma / snapshot local | Decisión pendiente |
|---|---|---|---|
| SDC-001 | Rulesets | GitHub documenta PR, checks, firmas, rutas y bypass. API actual: repositorio público, `rulesets: []`. | Diseñar reglas/bypass y probar merge bloqueado. |
| SDC-003 | `permissions`, SHA, Dependabot, triggers | API actual: Actions habilitado, `sha_pinning_required: true`, `allowed_actions: all`, permiso workflow por defecto `read`; Actions no puede aprobar PR. | Política de acciones permitidas, egress y evidencia API. |
| SDC-004 | Environments/concurrency | GitHub documenta environments/protecciones para repos públicos. API actual: `environments: []`; ninguna protección activa. | Crear sólo después del Gate 1; definir revisores independientes y rollback. |
| SDC-005 | OIDC / runner labels | OIDC requiere `id-token: write` y trust restringido. No hay snapshot de proveedor, runner o claims. | Confirmar Vault/cloud, cuentas, labels, red, Docker y egress. |
| SDC-006 | Attestations/SBOM | GitHub documenta attestations para repos públicos; no existe productor ni consumidor configurado en este repo. | Elegir registro, consumidor y prueba de rechazo. |
| SDC-007 | CODEOWNERS/ruleset/audit log | GitHub puede ayudar con ownership y revisión, no crea independencia semántica por sí solo. | Identidades de agentes, roles humanos y evidencias de handoff. |

## Estado de capacidades

### Resuelto para este demo público

- El repositorio es público; GitHub documenta Rulesets, Environments y Artifact
  Attestations como capacidades disponibles para repositorios públicos.
- La configuración de seguridad previa a diseño existe: SHA completa exigida para
  Actions, `GITHUB_TOKEN` de solo lectura por defecto y aprobación automática por
  Actions deshabilitada.

### No configurado o no verificado

- No existen rulesets ni environments observables por API al corte.
- No se verificó GHAS, secret scanning, audit log exportable, retención, runner groups,
  proveedor OIDC, Vault, registro de artefactos ni consumidor de provenance.
- GitHub permite que reusable workflows públicos se referencien por SHA, tag o branch;
  por tanto la exigencia de SHA para Actions no se trata como cobertura completa de
  reusable workflows sin una evaluación específica (EVAL-008).

### No extrapolable a un cliente enterprise

Un repositorio público personal no demuestra equipos organizacionales, reglas basadas en
teams, workflow reutilizable centralizado ni separación de funciones a escala. El futuro
adaptador enterprise debe verificarse en la organización objetivo, privada o pública,
con sus licencias y configuración reales.
