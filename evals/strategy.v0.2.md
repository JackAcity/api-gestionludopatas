# Estrategia de evaluación v0.2

**Estado:** diseño; no se ejecutaron fixtures ni se infiere madurez de controles.

## Regla de aceptación

No existe un número mágico de casos que conceda confianza. `100+` es un objetivo
experimental para detectar regresión de precisión a escala, no un gate normativo. Un
trust decision requiere cobertura explícita de amenazas, perfiles de riesgo y modos de
fallo, además de medir precisión y evidencia.

Cada control aplicable se deriva por estas dimensiones:

1. camino positivo conforme;
2. violación directa;
3. bypass o evasión;
4. límite / caso ambiguo;
5. excepción válida y con expiración;
6. caso benigno que no debe activar falso positivo;
7. interacción con otro control.

## Familias derivadas del modelo de amenazas

| Familia | Amenazas | Controles | Cobertura inicial esperada |
|---|---|---|---|
| EVF-01 gobierno SCM | TM-01, TM-02, TM-18 | SDC-001, SDC-007 | ruleset, bypass, tags/releases, ownership y publicación de referencia |
| EVF-02 validación | TM-07 | SDC-002, SDC-003 | build/test, dependencia y lockfile/feed |
| EVF-03 automatización | TM-03 a TM-07 | SDC-003, SDC-005, SDC-006 | triggers, permisos, acciones/reusable workflows, cache, artefactos e input injection |
| EVF-04 release y BD | TM-14, TM-15, TM-17 | SDC-004, SDC-006 | aprobación, TOCTOU, digest, rollback y compatibilidad de migración |
| EVF-05 identidad y runners | TM-03, TM-08, TM-09, TM-12 | SDC-005, SDC-003 | claims OIDC, egress, Docker socket, persistencia y scopes |
| EVF-06 integridad de artefacto | TM-02, TM-06, TM-12 a TM-14 | SDC-006 | digest, provenance, SBOM, verificación del consumidor y builder |
| EVF-07 agentes | TM-01, TM-10, TM-11, TM-18 | SDC-007, SDC-005 | contexto/prompt injection, tool misuse, separación y publicación |
| EVF-08 evidencia y recuperación | TM-16, TM-17 | SDC-004, SDC-006 | schema, hashes, correlación, retención, rollback y restore |
| EVF-09 publicación de referencia | TM-18 | PRRG-001 | propiedad intelectual, autorización, sanitización operacional, licencia y confirmación de escaneo |

## Medición y stop conditions

Para cada fixture se registran resultado esperado, resultado observado, evidencia
producida, severidad esperada y recomendación esperada. Se calculan true positives,
false positives, false negatives, exactitud de evidencia, severidad y recomendación.

No se avanza si ocurre cualquiera de estos casos:

- una ruta crítica no tiene familia EVF, control y caso planificado;
- un caso conforme se reporta como fallo sin justificación;
- un caso vulnerable pasa sin una excepción documentada;
- la evidencia no puede validar contra el schema o contiene secreto;
- la recomendación declara compliance, aislamiento o confianza no demostrados.
- el Public Reference Release Gate no tiene evidencia y aprobación humana cuando TM-18 aplica.

Las suites y fixtures se diseñan autocontenidos, sintéticos y sin secretos reales. Las
variantes se añaden por modo de fallo nuevo, no para alcanzar un contador.
