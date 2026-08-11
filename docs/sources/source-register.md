# Registro de fuentes v0.2

Regla: una fuente es normativa solo si su estado y emisor lo permiten. Fecha de acceso de todos los registros: **2026-08-11**.

| ID | Organización / documento | Versión, estado y fecha | URL canónica | Tipo de evidencia | Soporta | No soporta / límite |
|---|---|---|---|---|---|---|
| SRC-NIST-001 | NIST, *SP 800-218 Secure Software Development Framework* | v1.1, Final, 2022-02-03 | https://csrc.nist.gov/pubs/sp/800/218/final | Guía/recomendación final autoritativa | Vocabulario y prácticas SSDF: preparar, proteger software, producir software seguro y responder a vulnerabilidades. | No es por sí sola una obligación local; no prescribe GitHub, un workflow, una herramienta ni certifica cumplimiento. |
| SRC-NIST-002 | NIST, *SP 800-218 Rev. 1 SSDF v1.2* | Draft, 2025-12-17 | https://csrc.nist.gov/Projects/ssdf/publications | Borrador oficial | Señala evolución propuesta del SSDF. | No es baseline normativa hasta publicación final; no se usa para declarar conformidad. |
| SRC-SLSA-001 | SLSA, *SLSA specification* | v1.2, Approved, 2025-11-24 | https://slsa.dev/spec/v1.2/ | Especificación aprobada de consenso | Niveles/tracks de seguridad de cadena de suministro y formatos de provenance. | No prueba que un repositorio alcanza un nivel ni sustituye controles operativos. |
| SRC-DORA-001 | DORA, *Continuous delivery core* | Investigación/capacidad, vigente al acceso | https://dora.dev/capabilities/continuous-delivery/ | Evidencia de investigación | CI, testing continuo, seguridad y observabilidad como capacidades asociadas a entrega segura. | No es estándar de cumplimiento ni prescribe un proveedor. |
| SRC-DORA-002 | DORA, *Software delivery performance metrics* | Investigación, actualizado 2026-01-05 | https://dora.dev/guides/dora-metrics/ | Evidencia de investigación | Cinco métricas de throughput/estabilidad para mejora continua. | No define umbrales universales ni mide por sí sola seguridad. |
| SRC-GH-001 | GitHub Docs, *Secure use reference* | Guía de plataforma, vigente al acceso | https://docs.github.com/en/actions/reference/security/secure-use | Capacidad/guía de plataforma | Mínimo privilegio, riesgos de checkout y acciones de terceros. | No es un estándar universal ni garantiza configuración efectiva. |
| SRC-GH-002 | GitHub Docs, *Available rules for rulesets* | Capacidad de plataforma, vigente al acceso | https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/available-rules-for-rulesets | Capacidad de plataforma | PR, checks, firmas, rutas y bypass como opciones de ruleset. | No decide qué regla es apropiada ni evita bypass mal configurado. |
| SRC-GH-003 | GitHub Docs, *Deployments and environments* | Capacidad de plataforma, vigente al acceso | https://docs.github.com/en/actions/reference/workflows-and-actions/deployments-and-environments | Capacidad/limitación de plataforma | Environments, protecciones, secretos y restricciones de plan para repos privados. | No vuelve aislado un runner autoalojado ni reemplaza aprobación independiente. |
| SRC-GH-004 | GitHub Docs, *OpenID Connect reference* | Capacidad de plataforma, vigente al acceso | https://docs.github.com/en/actions/reference/security/oidc | Capacidad/guía de plataforma | Tokens de corta vida y condiciones sobre claims para federación. | No configura el trust del proveedor ni concede acceso sin `id-token: write`. |
| SRC-GH-005 | GitHub Docs, *Artifact attestations* | Capacidad de plataforma, vigente al acceso | https://docs.github.com/en/actions/concepts/security/artifact-attestations | Capacidad/guía de plataforma | Provenance/SBOM firmados y necesidad de verificarlos. | No garantiza seguridad si no se verifica; disponibilidad privada depende del plan. |
| SRC-NIST-003 | NIST NCCoE, *Accelerating the Adoption of Software and Artificial Intelligence Agent Identity and Authorization* | Initial Public Draft / concept paper, 2026-02-05 | https://csrc.nist.gov/pubs/other/2026/02/05/accelerating-the-adoption-of-software-and-ai-agent/ipd | Evidencia emergente oficial | Riesgos de identidad, autorización, auditoría, no repudio y prompt injection en agentes con acceso a datos, herramientas y aplicaciones. | No es un estándar final ni prescribe separación de funciones para agentes. |
| SRC-SF-001 | unclebob, *SwarmForge README* | Proyecto operativo, estado dinámico | https://github.com/unclebob/swarm-forge | Referencia de herramienta, no normativa | Roles, worktrees, handoffs y separación de agentes locales. | No es estándar de seguridad, no aprueba cambios ni convierte agentes en actores confiables. |
| SRC-DSL-001 | Unmesh Joshi / Martin Fowler, *DSLs Enable Reliable Use of LLMs* | Artículo técnico, 2026-07-14 | https://martinfowler.com/articles/llm-and-dsls.html | Perspectiva de ingeniería, no normativa | DSL pequeño, modelo semántico y validador determinista como arnés para LLMs. | No prueba que un DSL sea apropiado, seguro o suficiente sin evaluación independiente. |

**Contradicción verificada:** SSDF 1.1 es el baseline final; SSDF 1.2 figura como Draft en NIST, por lo que no se debe tratar como final. **Limitación de GitHub:** las reglas de aprobación y attestation para repos privados dependen del plan; su disponibilidad debe medirse mediante API/UI en el repositorio objetivo antes de diseñar un control obligatorio.

## Supersession y datos no publicados

| Fuente | `superseded_by` | Dato deliberadamente no inferido |
|---|---|---|
| SRC-NIST-001 | Ninguno: v1.2 sigue Draft. | Fecha de vigencia de una revisión futura. |
| SRC-NIST-002 | No aplica: es borrador. | Fecha de publicación final. |
| SRC-SLSA-001 | Ninguno conocido al acceso. | Nivel alcanzado por este repositorio. |
| SRC-DORA-001/002 | No aplica: investigación actualizable. | Umbral universal de rendimiento. |
| SRC-GH-001 a 005 | No aplica: documentación viva. | Plan/licencia/capacidad activa de este repo. |
| SRC-NIST-003 | No aplica: trabajo emergente. | Requisito normativo final para agentes. |
| SRC-SF-001 / SRC-DSL-001 | No aplica: referencia no normativa. | Requisito de seguridad o cumplimiento. |
