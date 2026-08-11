# Adaptador GitHub: seguridad de Actions (propuesto)

Mapea SDC-003: `permissions` por workflow/job, acciones/reusable workflows fijados a referencias inmutables, `persist-credentials: false` cuando no se escribe, y prohibición de `pull_request_target` para compilar/ejecutar código de PR salvo amenaza y compensaciones aprobadas. Las actualizaciones de acciones se revisan como dependencias.

La verificación no es leer YAML: casos EVAL deben detectar tag mutable, permiso amplio y trigger inseguro sin marcar falsamente un workflow de solo lectura correcto.
