# Modelo de control portable v0.1

## Cadena de razonamiento obligatoria

`Evidencia → riesgo → requisito → control → implementación → verificación → evidencia producida`.

Un control no existe porque una herramienta lo ofrezca. Existe solo cuando un riesgo, una fuente y un método de verificación justifican coste y operación. El nombre del control y requisito no contiene términos de proveedor; GitHub aparece únicamente en una implementación candidata.

## Terminología controlada

| Etiqueta | Significado |
|---|---|
| **FACT** | Hecho observado, con fecha y evidencia reproducible. |
| **FINAL AUTHORITATIVE GUIDANCE** | Publicación final que recomienda prácticas; por sí sola no crea una obligación local. Ejemplo: NIST SP 800-218. |
| **CONDITIONAL SPECIFICATION REQUIREMENT** | `MUST`/`SHALL` aplicable solo cuando se pretende satisfacer un track, nivel o perfil declarado. Ejemplo: SLSA Build L2. |
| **LOCAL MANDATORY REQUIREMENT** | Obligación adoptada expresamente por política, contrato, regulación o mandato de proyecto, con dueño y fundamento registrados. |
| **PLATFORM CAPABILITY** | Función que una plataforma declara soportar; requiere configuración y prueba. |
| **RESEARCH EVIDENCE** | Hallazgo que orienta una decisión, no obligación de cumplimiento. |
| **ENGINEERING DECISION** | Elección local con alternativas, dueño y reversa. |
| **ASSUMPTION** | Premisa no demostrada que bloquea o condiciona diseño. |
| **HYPOTHESIS** | Afirmación que se probará mediante evaluación. |
| **EXCEPTION** | Desviación aprobada, temporal, con riesgo residual y fecha de revisión. |

Un locator de NIST sustenta una recomendación final y una interpretación local. Solo se
convierte en `LOCAL MANDATORY REQUIREMENT` al registrar una adopción explícita. El
catálogo actual contiene controles **propuestos**, no mandatos organizacionales adoptados.

## Perfiles de riesgo

| Perfil | Criterio | Consecuencia de gobierno |
|---|---|---|
| Bajo | Reversible, sin dato sensible ni identidad/infraestructura. | PR y gates deterministas; dueño técnico. |
| Medio | Afecta contrato, datos no sensibles, dependencia o disponibilidad limitada. | Revisión independiente, reversa probada y evidencia de ambiente. |
| Alto | Producción, datos sensibles, privilegios/identidad, DB destructiva o alto blast radius. | Separación de deberes, aprobación de ambiente, artefacto verificable y plan de recuperación ensayado. |

Los perfiles son una decisión inicial; la clasificación concreta se registra por cambio y puede subir, nunca bajar, ante incertidumbre relevante.
