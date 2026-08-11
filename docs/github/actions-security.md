# Adaptador GitHub: seguridad de Actions (propuesto)

Mapea SDC-003: `permissions` por workflow/job, acciones fijadas a referencias
inmutables, `persist-credentials: false` cuando no se escribe, y prohibición de
`pull_request_target` para compilar/ejecutar código de PR salvo amenaza y compensaciones
aprobadas. Las actualizaciones de acciones se revisan como dependencias.

**Límite verificado:** el ajuste de GitHub que exige SHA completa para Actions no prueba
por sí solo que los reusable workflows estén fijados: GitHub permite referenciarlos por
SHA, tag o branch. EVAL-008 debe detectar esta evasión antes de afirmar automatización
inmutable.

La verificación no es leer YAML: casos EVAL deben detectar tag mutable, permiso amplio y trigger inseguro sin marcar falsamente un workflow de solo lectura correcto.
