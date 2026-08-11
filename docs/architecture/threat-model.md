# Modelo de amenazas v0.1

**Método:** activos, actores, límites de confianza, rutas de ataque y controles candidatos. Es un diseño; falta validación de arquitectura/operación.

## Activos y límites

| Activo | Límite de confianza | Actor legítimo | Riesgo principal |
|---|---|---|---|
| Intención, issue y OpenSpec | humano → repositorio | product owner / arquitecto | requisito ambiguo o manipulado |
| Código y workflow | estación autor/agente → SCM | contribuidor | PR malicioso, cambio de workflow |
| `GITHUB_TOKEN` / GitHub App | job → GitHub API | workflow concreto | privilegio excesivo o exfiltración |
| Runner y workspace | SCM → runner → Docker/red | plataforma | código no confiable ejecutado con acceso interno |
| Identidad OIDC/Vault/cloud | GitHub → proveedor de identidad | job de deploy | claims amplios, trust confuso, token persistente |
| Artefacto, SBOM, provenance | builder → registro → ambiente | release engineering | sustitución o provenance sin verificar |
| Ambiente, BD y secretos | pipeline → red on-premise | operador aprobado | escalamiento, DDL no autorizado, fuga |
| Evidencia/audit log | plataforma → auditor | auditor independiente | retención insuficiente o evidencia no ligable |

## Rutas prioritarias

| ID | Ruta de ataque | Control candidato | Riesgo residual |
|---|---|---|---|
| TM-01 | PR modifica workflow y ejecuta código de atacante con secreto. | SDC-003: triggers seguros, permisos mínimos, no secretos/runner privilegiado para PR. | Acción legítima comprometida. |
| TM-02 | Acción de tercero cambia detrás de un tag. | SDC-003: referencia inmutable, actualización revisada y evaluación. | SHA de release comprometida aguas arriba. |
| TM-03 | Runner autoalojado comparte host/red con producción y ejecuta PR. | SDC-005: runner dedicado por confianza/ambiente; ningún PR no confiable. | Compromiso del host dedicado. |
| TM-04 | Agente genera, valida superficialmente, aprueba y despliega su propio cambio. | SDC-007 y SDC-004: independencia determinista y humana proporcional al riesgo. | Sesgo/collusion humana. |
| TM-05 | Artefacto distinto al que aprobó CI se despliega. | SDC-006: digest, provenance/SBOM y verificación de consumidor. | Política de verificación no aplicada. |
| TM-06 | OIDC trust acepta cualquier repo/ref o subject legado ambiguo. | SDC-005: audience/subject/repo/workflow restringidos y prueba de denegación. | Error del proveedor o cambio de claims. |
| TM-07 | Cambio DB incompatible o destructivo pasa junto con aplicación. | SDC-004 + gestión DB: pase DBA, compatibilidad, backup/reversa probada. | Error humano en operación DBA. |

**Principio de agentes:** el agente no es actor confiable. Su salida se trata como contenido de PR no confiable hasta que controles independientes produzcan evidencia.
