# Modelo de control portable v0.1

## Cadena de razonamiento obligatoria

`Evidencia → riesgo → requisito → control → implementación → verificación → evidencia producida`.

Un control no existe porque una herramienta lo ofrezca. Existe solo cuando un riesgo, una fuente y un método de verificación justifican coste y operación. El nombre del control y requisito no contiene términos de proveedor; GitHub aparece únicamente en una implementación candidata.

## Terminología controlada

| Etiqueta | Significado |
|---|---|
| **FACT** | Hecho observado, con fecha y evidencia reproducible. |
| **STANDARD REQUIREMENT** | Obligación tomada de una fuente normativa aplicable. |
| **PLATFORM CAPABILITY** | Función que una plataforma declara soportar; requiere configuración y prueba. |
| **RESEARCH EVIDENCE** | Hallazgo que orienta una decisión, no obligación de cumplimiento. |
| **ENGINEERING DECISION** | Elección local con alternativas, dueño y reversa. |
| **ASSUMPTION** | Premisa no demostrada que bloquea o condiciona diseño. |
| **HYPOTHESIS** | Afirmación que se probará mediante evaluación. |
| **EXCEPTION** | Desviación aprobada, temporal, con riesgo residual y fecha de revisión. |

## Perfiles de riesgo

| Perfil | Criterio | Consecuencia de gobierno |
|---|---|---|
| Bajo | Reversible, sin dato sensible ni identidad/infraestructura. | PR y gates deterministas; dueño técnico. |
| Medio | Afecta contrato, datos no sensibles, dependencia o disponibilidad limitada. | Revisión independiente, reversa probada y evidencia de ambiente. |
| Alto | Producción, datos sensibles, privilegios/identidad, DB destructiva o alto blast radius. | Separación de deberes, aprobación de ambiente, artefacto verificable y plan de recuperación ensayado. |

Los perfiles son una decisión inicial; la clasificación concreta se registra por cambio y puede subir, nunca bajar, ante incertidumbre relevante.
