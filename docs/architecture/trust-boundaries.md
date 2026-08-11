# Límites de confianza

1. **Autor/agente → SCM:** toda contribución es candidata; ni el texto de PR ni una skill es evidencia.
2. **SCM → runner hospedado:** usar para validación sin secretos de despliegue; checkout de código de PR sigue siendo no confiable.
3. **SCM → runner autoalojado:** frontera crítica; un Environment no aísla el proceso/host. Solo jobs confiables, runners dedicados y segmentados.
4. **Workflow → proveedor de identidad:** OIDC debe restringir issuer, audience y subject al repo/ref/workflow esperado; no usar token largo como sustituto por conveniencia.
5. **Artefacto → ambiente:** solo digest/provenance verificados; el ambiente no recompila una fuente distinta bajo identidad informal.
6. **Agente → decisiones:** el agente puede proponer y ejecutar checks no privilegiados, nunca ser aprobación única ni poseer secretos de producción.
