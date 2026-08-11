# Validación semántica de Evidence Bundle v0.1

## Separación de responsabilidades

| Capa | Pregunta | Resultado |
|---|---|---|
| JSON Schema | ¿La instancia tiene campos, tipos y relaciones locales válidas? | `structure_valid` o errores de estructura. |
| Validador semántico | ¿Las identidades, la cadena de custodia y correlaciones declaradas satisfacen reglas de control? | `semantic_valid` o códigos de regla. |
| Adaptador de sistema fuente | ¿El commit, run, digest, firma y evento existen realmente y son confiables? | evidencia verificable o `unknown`. |

Un `content_digest` no autentica por sí mismo un bundle. La autenticidad exige que un
adaptador consulte el sistema fuente, valide una firma/attestation cuando exista y
correlacione el resultado con la identidad de la fuente.

## Reglas deterministas propuestas

| ID | Regla | Aplicación |
|---|---|---|
| SEM-001 | `producer` y `verifier` deben ser identidades canónicas distintas cuando el control exige verificación independiente. | Todos los controles con evidencia independiente. |
| SEM-002 | La cadena debe contener evento `produced` del productor y evento `verified` del verificador. | Bundles que afirman validación. |
| SEM-003 | Cada evento de cadena debe conservar el `content_digest` del bundle o registrar explícitamente una transformación versionada. | Todos los bundles. |
| SEM-004 | SHA de commit es completa y compatible con `scm_type`/`commit_hash_algorithm`. | Bundles ligados a fuente. |
| SEM-005 | `workflow`, `run_id`, `commit_uri`, digest de artefacto y URI de evidencia se resuelven contra la API/registro fuente cuando aplican. | Adaptador de plataforma futuro. |
| SEM-006 | La identidad del productor/verificador se resuelve contra un emisor confiable y se contrasta con el actor del evento. | Adaptador de identidad futuro. |
| SEM-007 | Una attestation requiere digest, autenticidad verificable y política de consumidor; digest sin trust no satisface SDC-006. | SDC-006. |

Los resolvers de SEM-005 y SEM-006 son deliberadamente **TBD**: implementarlos exige
decidir SCM, identidad, registro, retención y permisos. Si no pueden consultar la fuente,
deben devolver `unknown`, nunca `pass`.

## Vectores reproducibles

Los fixtures en [`evals/evidence/`](../../evals/evidence/) prueban Schema + SEM-001 a
SEM-004 sin depender de red ni secretos. Ejecutar:

```powershell
python -m pip install -r evals/evidence/requirements.txt
python evals/evidence/validate_vectors.py
```

El validador de fixtures no afirma que un commit, workflow run, artefacto o firma real
existan. Esa limitación es intencional y forma parte del Gate 1.

## Mejora no bloqueante para Gate 2

El harness actual aplica SEM-001 a sus fixtures porque todos afirman verificación
independiente. Antes de usarlo fuera de fixtures debe consultar una política explícita:
`control_id → verification_policy → independence_required`. Así una regla contextual no
se convierte en prohibición universal. Está registrada como EVAL-DOR-002 y no reabre el
diseño de Gate 1.
