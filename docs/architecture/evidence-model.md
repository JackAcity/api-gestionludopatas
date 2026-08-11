# Modelo de evidencia v0.1

Una configuración no demuestra un control. Cada evidencia es inmutable o conserva hash, productor, fecha, identidad, alcance y relación con `commit`, `control_id` y ambiente.

| Pregunta de auditoría | Evidencia mínima | Verificador independiente |
|---|---|---|
| ¿Qué cambió y por qué? | issue/OpenSpec, PR, commit firmado si aplica, diff | revisor / API SCM |
| ¿Quién o qué lo produjo? | identidad humana/agent, run ID y actor de aprobación | audit log SCM |
| ¿Qué verificó el cambio? | resultado CI, reportes de prueba/análisis, versión de herramientas | reejecución o descarga de artifact |
| ¿Qué artefacto se desplegó? | digest inmutable, SBOM, provenance/attestation si está disponible | verificador de attestation / registro |
| ¿Quién autorizó y dónde? | ambiente, aprobación, deployment log y URL/host sanitizada | audit log del entorno |
| ¿Se puede recuperar? | prueba de rollback, runbook y resultado fechado | operador diferente al autor |

Retención, ubicación, acceso y borrado son **TBD** organizacionales. El control falla si la evidencia no puede correlacionarse sin depender de memoria humana o de logs con secretos.
