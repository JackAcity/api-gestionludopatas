# Modelo de evidencia v0.2

Una configuración no demuestra un control. La unidad mínima es un **Evidence Bundle**
que valida contra [`evidence-bundle.schema.v0.1.json`](evidence-bundle.schema.v0.1.json).
El schema es un contrato de diseño: no implica que ya exista un productor, repositorio de
evidencia o mecanismo de retención.

## Propiedades exigidas al bundle

| Propiedad | Regla de diseño | Estado operativo |
|---|---|---|
| Correlación | `control_id`, `change_id`, repositorio, `commit_sha`, workflow/run y ambiente se relacionan sin memoria humana. | Por implementar/evaluar. |
| Integridad | `content_digest` identifica el contenido y `artifact_digest` identifica el artefacto cuando aplica. | Por implementar/evaluar. |
| Productor y verificador | Se registra `producer`, `actor`, `actor_type` y `verifier`; el productor no es por sí solo el verificador independiente. | Por implementar/evaluar. |
| Tiempo | `timestamp` usa UTC RFC 3339; la fuente de tiempo y tolerancia de clock se definen por plataforma. | TBD de plataforma. |
| Sensibilidad | Cada bundle declara sensibilidad; nunca se incluyen secretos en contenido, URI ni logs adjuntos. | Requisito de diseño. |
| Retención y acceso | `retention_class` clasifica la intención. Periodos, ubicación, borrado y ACL requieren política aprobada. | TBD organizacional. |
| Reproducibilidad | URI, digest, commit y versión de herramienta deben permitir revalidar o explicar por qué no es reproducible. | Por implementar/evaluar. |

## Preguntas de auditoría

| Pregunta | Campos/evidencia mínimos | Verificador independiente candidato |
|---|---|---|
| ¿Qué cambió y por qué? | `change_id`, `commit_sha`, URI de PR/issue/OpenSpec | API SCM / revisor |
| ¿Quién o qué lo produjo? | `actor`, `actor_type`, `producer`, run ID | audit log SCM |
| ¿Qué lo verificó? | `verifier`, resultado, reporte y versión de herramienta | reejecución o descarga con digest |
| ¿Qué artefacto se desplegó? | `artifact_digest`, provenance/SBOM y log de verificación | consumidor/registro |
| ¿Quién autorizó y dónde? | ambiente, evento de aprobación y deployment log | audit log del entorno |
| ¿Se puede recuperar? | evidencia de prueba de rollback y runbook fechado | operador distinto al autor |

Un control falla si el bundle no puede correlacionarse, no puede verificarse de forma
independiente o expone información sensible. La retención no se declara suficiente hasta
que el decisor organizacional apruebe duración, ubicación, ACL y proceso de borrado.
