# Definition of Ready — fixtures de evaluación

Estos refinamientos no bloquean Gate 1. Son requisitos de diseño antes de ejecutar un
fixture de Gate 2 y existen para medir falsos positivos con honestidad.

| ID | Mejora | Alcance | Gate destino |
|---|---|---|---|
| EVAL-DOR-001 | Separar violación de excepción válida | Para lockfile, egress, break-glass, restore y publicación, crear un caso `FAIL` sin autorización y un caso `PASS / EXCEPTION` con alcance, dueño, expiración y evidencia. No reutilizar el mismo fixture para ambas semánticas. | Gate 2, antes de ejecutar la familia EVF correspondiente. |
| EVAL-DOR-002 | Hacer SEM-001 sensible al control | El validador de producción consulta `control_id → verification_policy → independence_required`; el harness offline actual solo demuestra los vectores que afirman independencia. | Gate 2, antes de usar Evidence Bundle fuera de fixtures. |

Los IDs EVAL existentes con dimensión `valid-exception` son planificación de cobertura,
no prueba de una excepción válida. Sus pares se asignan al implementar el fixture real.
