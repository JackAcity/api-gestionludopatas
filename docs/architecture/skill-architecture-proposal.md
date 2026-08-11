# Arquitectura de skills propuesta

Las tres skills separan diseño, adaptador y auditoría; las entradas/salidas se validarán contra `delivery-control` y la matriz de evals. Cada recomendación material debe incluir `control_id`, perfil, locator de fuente, clasificación de autoridad, hecho/hipótesis y método de verificación. Ninguna skill puede otorgarse autoridad por su propio resultado.

| Skill | Criterio de invocación | Verificación propia |
|---|---|---|
| `secure-delivery-design` | Nuevo riesgo, requisito, perfil o control. | Catálogo válido, fuentes clasificadas y supuestos explícitos. |
| `github-secure-delivery` | Control aprobado necesita mapa GitHub. | Capability snapshot y escenarios de fallo/éxito. |
| `delivery-audit` | Existe implementación/evidencia para evaluar. | Ejecuta matriz de fixtures y mide falsos positivos/negativos. |
