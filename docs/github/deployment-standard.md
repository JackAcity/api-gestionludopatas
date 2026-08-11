# Adaptador GitHub: estándar de despliegue (propuesto)

Mapea SDC-004/005/006. Un job de deploy usa artefacto/digest aprobado, ambiente objetivo y runner de confianza acorde; serializa despliegues, registra aprobación y comprueba salud/recuperación. Un runner autoalojado no es aislado por Environment, por lo que no recibe PRs no confiables ni secretos compartidos.

Para esta API on-premise faltan runner dedicado, política de Environment, identidad al Vault, método de artifact registry y prueba de rollback. El deploy a DB sigue pase DBA separado.
