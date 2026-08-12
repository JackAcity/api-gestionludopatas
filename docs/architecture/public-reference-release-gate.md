# PRRG-001 — Public Reference Release Gate

**Tipo:** precondición de publicación del laboratorio; no es el octavo control del
catálogo Secure Delivery.
**Estado:** autorización temporal de visibilidad registrada; claim de referencia pública aún no autorizado.

La [decisión humana de Gate 1](gate-1-human-closure-2026-08-12.md) autoriza mantener público este repositorio durante el trabajo actual. No sustituye las precondiciones de este gate ni autoriza una promoción como referencia pública.

Los controles SDC protegen el proceso de entrega. No resuelven por sí mismos el derecho
a publicar código, contratos, nombres de dominio o detalles operativos de un empleador o
cliente. Antes de hacer público, forkear o promocionar una referencia se exige evidencia
para cada punto aplicable:

| Precondición | Evidencia esperada | Dueño |
|---|---|---|
| Titularidad y autorización | Confirmación escrita de dueño legal/negocio o registro de procedencia propia. | Dueño legal/negocio |
| Confidencialidad | Revisión de contratos, nombres, endpoints, IPs, infraestructura y datos. | Dueño de información |
| Sanitización | Inventario de sustituciones por datos/nombres sintéticos y revisión de diff. | Maintainer técnico |
| Secretos | Registro Gitleaks confirmado por humano o remediación/rotación documentada. | Seguridad / dueño de secreto |
| Operación | Confirmación de que runbooks, hosts, Vault, DB y flujos no revelan operación interna. | Operaciones |
| Reutilización | Licencia, descripción, topics y aviso de que es referencia no productiva. | Dueño del repositorio |

**Criterio de decisión:** todos los elementos aplicables tienen evidencia y un aprobador
autorizado. Una ausencia devuelve `HOLD`; no se compensa con checks verdes ni con una
clasificación automatizada. EVAL-031 prueba que la falta de autorización o evidencia
impide el claim de referencia pública.
