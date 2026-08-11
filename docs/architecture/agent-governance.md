# Gobierno de agentes y propuesta de DSL

**Clasificación de la política:** la separación entre generación, verificación,
aprobación y ejecución es una **ENGINEERING DECISION local**. SSDF aporta principios de
mínimo privilegio; SRC-NIST-003 aporta evidencia emergente sobre identidad,
autorización y prompt injection de agentes. Ninguna de esas fuentes finaliza por sí
sola esta política de segregación.

SwarmForge aporta coordinación local por roles y worktrees; no aporta confianza, control de acceso ni aprobación. Para cambios materiales, la secuencia es: **intención humana → candidato de agente → verificación determinista independiente → CI independiente → gates de seguridad/cadena → evidencia → aprobación apropiada → merge/deploy**.

## Tres skills propuestas

| Skill | Entrada | Salida permitida | Prohibiciones |
|---|---|---|---|
| `secure-delivery-design` | riesgos, fuentes, perfil | controles portables, supuestos y evaluaciones | implementar YAML de plataforma o afirmar cumplimiento. |
| `github-secure-delivery` | controles aprobados + capability map | plan GitHub con permisos, límites y pruebas | inferir capacidades/licencia o desplegar. |
| `delivery-audit` | configuración/evidencia + catálogo | hallazgos con `control_id`, evidencia y certeza | aprobar, fusionar, desplegar o validar su propia implementación como única fuente. |

## DSL `delivery-control` — hipótesis, no implementación

El DSL será declarativo, pequeño y validado por esquema antes de usar LLMs. Representa
hechos de control, no comandos de infraestructura:

```yaml
control_claim:
  control_id: SDC-003
  profile: high
  implementation: github
  evidence:
    - type: workflow-source
      assertion: actions_are_immutable_and_permissions_minimal
  verification:
    case_id: EVAL-003
    expected: fail_for_mutable_reference
  exception: null
```

Fase A: modelar vocabulario y JSON Schema/validador determinista. Fase B: crear cobertura
derivada de amenazas y, como objetivo experimental, 100+ casos representativos; medir precisión de evidencia/severidad. Fase C: permitir que un LLM traduzca
lenguaje natural a candidato DSL y reparar solo errores del validador. Fase D: generar
planes de adaptador, nunca cambios o despliegues automáticos. Esta propuesta toma de
Fowler el arnés de modelo semántico + validador, no una promesa de fiabilidad universal.
