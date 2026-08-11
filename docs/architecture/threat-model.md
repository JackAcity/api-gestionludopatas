# Modelo de amenazas v0.2

**Estado:** diseño ampliado; no es una evaluación de seguridad ejecutada. Cada ruta se
relaciona con controles SDC y una familia de evaluación. Si una ruta no queda cubierta
por un control verificable, el gate permanece abierto.

## Activos y límites de confianza

| Activo | Límite de confianza | Actor legítimo | Riesgo principal |
|---|---|---|---|
| Intención, issue y OpenSpec | humano → SCM | product owner / arquitecto | requisito ambiguo, contexto inyectado o manipulado |
| Código, tags, releases y workflows | estación/agent → SCM | contribuidor | identidad comprometida, bypass administrativo o cambio malicioso |
| `GITHUB_TOKEN`, GitHub App y variables | workflow → API GitHub | job concreto | privilegio excesivo, confused deputy o exfiltración |
| Reusable workflow, cache y artefactos intermedios | repo/tercero → workflow | plataforma | referencia mutable, cache poisoning o sustitución entre workflows |
| Dependencias NuGet, acciones e imágenes | feed/registro → build | engineering | dependency confusion, paquete comprometido o lockfile ignorado |
| Runner, workspace, Docker y egress | SCM → runner → red | plataforma | persistencia entre jobs, socket Docker, workspace poisoning o acceso interno |
| Identidad OIDC/Vault/cloud | GitHub → proveedor de identidad | job de despliegue | claims amplios, token persistente o trust confuso |
| Artefacto, SBOM y provenance | builder → registro → ambiente | release engineering | builder comprometido, sustitución o verificación omitida |
| Ambiente, BD y secretos | pipeline → red on-premise | operador aprobado | escalamiento, migración destructiva, incompatibilidad o fuga |
| Evidencia y auditoría | plataforma → auditor | auditor independiente | borrado, truncamiento, spoofing o pérdida de correlación |

## Rutas prioritarias

| ID | Ruta de ataque o fallo | Controles vinculados | Familia de evaluación | Riesgo residual |
|---|---|---|---|---|
| TM-01 | Un administrador, token comprometido o bypass integra en `main` sin gates. | SDC-001, SDC-007 | EVF-01 | Compromiso de propietario o colusión. |
| TM-02 | Tag/release apunta a revisión no aprobada o historia reescrita. | SDC-001, SDC-006 | EVF-01, EVF-06 | Capacidad del SCM y fuerza de revisión todavía no medidas. |
| TM-03 | PR modifica workflow y ejecuta código de atacante con secreto o token escribible. | SDC-003, SDC-005 | EVF-03, EVF-05 | Acción legítima comprometida. |
| TM-04 | `pull_request_target`, `workflow_run` o interpolación de contexto produce script injection. | SDC-003 | EVF-03 | Reglas de lint incompletas. |
| TM-05 | Reusable workflow o Action de tercero usa tag mutable; el SHA pinning no cubre todos los tipos de referencia. | SDC-003 | EVF-03 | Dependencia fijada comprometida aguas arriba. |
| TM-06 | Cache, artefacto intermedio o workspace de otro workflow contamina el build. | SDC-003, SDC-006 | EVF-03, EVF-06 | Aislamiento real del runner no demostrado. |
| TM-07 | Dependency confusion, feed NuGet no confiable, paquete comprometido o lockfile no aplicado. | SDC-003, SDC-002 | EVF-03, EVF-02 | Vulnerabilidad legítima aún desconocida. |
| TM-08 | Runner autoalojado persiste entre jobs, expone socket Docker, permite egress amplio o ejecuta PR no confiable. | SDC-005, SDC-003 | EVF-05, EVF-03 | Compromiso del host dedicado. |
| TM-09 | OIDC trust acepta repo, ref, audience o workflow no autorizado; un job actúa como confused deputy. | SDC-005 | EVF-05 | Error o cambio en el proveedor de identidad. |
| TM-10 | Agente recibe prompt/context injection, usa una herramienta fuera de intención o propaga una credencial. | SDC-007, SDC-005 | EVF-07, EVF-05 | Error de revisión humana o modelo. |
| TM-11 | Mismo agente genera, se auto-verifica y ejecuta un cambio material. | SDC-007, SDC-004 | EVF-07, EVF-04 | Sesgo o colusión humana. |
| TM-12 | Builder o identidad de build se compromete y genera artefacto/provenance engañoso. | SDC-006, SDC-005 | EVF-06, EVF-05 | Límite de confianza del proveedor de build. |
| TM-13 | Se omite la verificación de digest/provenance o se despliega un artefacto sustituto. | SDC-006, SDC-004 | EVF-06, EVF-04 | Política de consumidor no aplicada en runtime. |
| TM-14 | Hay TOCTOU entre aprobación, artefacto y deployment; se promueve un digest distinto. | SDC-004, SDC-006 | EVF-04, EVF-06 | Registro/ambiente comprometido. |
| TM-15 | Aprobación de producción se autoaprueba o se realiza desde identidad no autorizada. | SDC-004, SDC-007 | EVF-04, EVF-07 | Break-glass mal usado. |
| TM-16 | Evidencia se borra, trunca, falsea o no puede correlacionarse con commit/run/deployment. | SDC-004, SDC-006 | EVF-08 | Retención y exportación organizacionales aún TBD. |
| TM-17 | Migración de BD irreversible, no compatible N/N-1 o restore no probado acompaña al despliegue. | SDC-004 | EVF-04, EVF-08 | Error humano del DBA u operación de restore. |
| TM-18 | Se publica información de empleador/cliente, detalle operativo o secreto en el repositorio de demostración. | PRRG-001 Public Reference Release Gate (precondición, no SDC) | EVF-09 | Clasificación legal/contractual pendiente. |

## Supuestos y límites

- El modelo no supone que GitHub-hosted o self-hosted runners sean seguros por nombre; el aislamiento debe evidenciarse.
- La seguridad de un workflow no se deduce de checks verdes.
- El agente no es actor confiable. Su salida es contenido no confiable hasta verificación independiente.
- La cobertura de amenazas se valida mediante familias EVF, no por el número bruto de fixtures.
