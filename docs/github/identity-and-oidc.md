# Adaptador GitHub: identidad y OIDC (propuesto)

Mapea SDC-005. El job solicita `id-token: write` solo si necesita federación. El proveedor debe validar issuer, audience, repositorio, ref/Environment y, si procede, reusable workflow; GitHub advierte que el formato de `sub` puede ser inmutable en repositorios nuevos y debe comprobarse antes de escribir trust policies.

No se aprueba almacenamiento de credencial larga en GitHub como solución por defecto. Si Vault/on-premise no admite federación, se diseña excepción con rotación, dueño, expiración, alcance y test de revocación.
