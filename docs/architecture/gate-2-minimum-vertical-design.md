# Gate 2 — vertical mínimo: referencias inmutables de workflows

**Estado:** diseño propuesto; ningún control se declara operativo todavía.
**Vertical:** SDC-003 `workflow-supply-chain-integrity`, familia EVF-03.
**Decisión:** comprobar de forma determinista las referencias `uses:` de todos los workflows versionados antes de ampliar el alcance a permisos, runners, OIDC, despliegue o provenance.

## Por qué este vertical primero

El repositorio ya tiene workflows de CI, DevSecOps y despliegue con referencias SHA. Esta rebanada trata TM-05 y aporta señal parcial sobre TM-03/TM-04 sin requerir un secreto, un proveedor externo, un runner de confianza ni cambio de ambiente. También cubre los casos planificados EVAL-003 y EVAL-008 con fixtures sintéticos.

No demuestra aislamiento de runner, permisos mínimos completos, protección de `main`, identidad de despliegue, integridad de artefacto ni autorización de producción. Esos riesgos quedan explícitamente fuera de este vertical.

## Invariante y decisión de implementación

`INV-SDC003-REF-001`: cada referencia externa `uses:` en `.github/workflows/**/*.yml` o `.yaml` debe terminar en un SHA Git hexadecimal completo de 40 caracteres. Una acción local con ruta relativa `./` se permite porque queda ligada al commit evaluado. Una referencia dinámica, incompleta o de tipo no reconocido falla cerrada.

La implementación propuesta es un validador PowerShell versionado en el repositorio, sin descarga de herramienta adicional. El parser se limita a la sintaxis `uses:` que el repositorio adopta y falla si no puede clasificar una línea relevante. Esta elección evita añadir una dependencia de auditoría antes de poder verificarla; el costo es mantener fixtures para cada sintaxis admitida. Si el repositorio necesita sintaxis no cubierta, se amplía el parser y sus pruebas antes de habilitarla, nunca mediante una exclusión implícita.

## Cambios previstos para la implementación

1. Añadir `tools/validate-workflow-references.ps1`, con entradas explícitas y códigos de salida deterministas.
2. Añadir fixtures autocontenidos bajo `evals/fixtures/evf-03/`; no contendrán secretos ni acciones ejecutables.
3. Añadir pruebas que ejecuten el validador contra los fixtures y contra los workflows reales.
4. Añadir un job `workflow-policy` de solo lectura al CI existente. No recibe secretos ni permisos de escritura.
5. Publicar el resultado de la ejecución como log y artefacto efímero del workflow, sin presentar todavía un Evidence Bundle de retención auditada.

## Casos de aceptación adversariales

| Caso | Dimensión | Resultado esperado | Riesgo / caso matriz |
|---|---|---|---|
| `WRP-001` | positivo | referencia de acción con SHA completo pasa. | línea base conforme |
| `WRP-002` | violación directa | acción externa con `@vN` o `@main` falla con `mutable-action-reference`. | EVAL-003 / TM-05 |
| `WRP-003` | bypass | reusable workflow externo con tag o rama falla con `mutable-reusable-workflow-reference`. | EVAL-008 / TM-05 |
| `WRP-004` | benigno | acción local `./.github/actions/...` pasa. | control de falso positivo |
| `WRP-005` | límite | referencia dinámica, sin `@` o con SHA de longitud incorrecta falla cerrada. | evasión de parser / TM-05 |
| `WRP-006` | integración | los workflows reales del commit pasan y el job CI conserva `contents: read`. | verificación del vertical |

Cada fixture registra sólo regla, ruta sintética y resultado esperado. No se crea un token, un secreto ni una referencia que descargue código remoto.

## Evidencia y límites

La evidencia mínima de esta rebanada será fuente de workflow, resultado de fixture, SHA del commit y URL/ID de ejecución CI. Es suficiente para evaluar el validador, no para afirmar la integridad completa de la cadena de suministro. La retención, independencia del verificador, inventario de runner y cadena de custodia siguen en G1-10/G1-11 y se resolverán en verticales posteriores.

Un agente puede proponer el validador o fixtures, pero no es su único verificador, aprobador ni ejecutor: las pruebas locales, CI independiente y revisión humana se mantienen como barreras distintas, conforme a SDC-007.

## Criterio de salida y rollback

El vertical queda aceptable sólo si WRP-001 a WRP-006 producen el resultado esperado, el job CI bloquea un cambio que introduce una referencia mutable y ningún workflow real requiere excepción. Se detiene si aparece falso negativo, una sintaxis no clasificable, una necesidad de permiso adicional o una excepción sin dueño/expiración.

El rollback consiste en retirar el job, el script y los fixtures de esta rebanada; no hay cambio de despliegue, estado de base de datos, secreto, runner ni ambiente que revertir.

## Siguiente vertical, no incluido

Después de evidencia satisfactoria, Gate 2 decide si sigue SDC-001 (protección de `main`) o SDC-005 (runner/identidad). La elección dependerá de que se resuelvan OQ-001 a OQ-004; no se infiere desde el éxito de este validador.
