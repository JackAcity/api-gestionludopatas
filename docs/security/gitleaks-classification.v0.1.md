# Registro sanitizado de clasificación Gitleaks v0.1

**Escáner:** Gitleaks v8.30.1.
**Alcance reproducible:** todas las referencias Git del repositorio al 2026-08-11.
**Comando conceptual:** `gitleaks git <repo> --log-opts=--all --redact=100`.
**Regla:** no se almacenan coincidencias, literales ni secretos en este registro.

El escáner informó siete coincidencias de la regla `generic-api-key` en el commit
`799324ce8d4a`. La clasificación siguiente es **preliminar automatizada**: una persona
autorizada debe confirmarla antes de que cualquier documento afirme que el repositorio
está libre de secretos.

| ID | `location_hash` | Clasificación preliminar | Razonamiento sanitizado | Reviewer | Fecha | Remediación |
|---|---|---|---|---|---|---|
| GL-001 | `sha256:cca12e2e19c1d26e96fbca891b6d6559c5545283160b7ebcb88837722d8a0aec` | candidate-false-positive | Constante de código de error; no hay asignación de secreto ni valor de alta entropía. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-002 | `sha256:fbca7ea65a20a61fcfbfeb3a31eaffcdc273579e74c33e88227ca9368b46700f` | candidate-false-positive | Constante de código de error; no hay asignación de secreto ni valor de alta entropía. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-003 | `sha256:6aefac54342c87652d101ce55d879946d5d0aa5eadf819c39a41586d8a33e177` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-004 | `sha256:1e5746bc38261c620be73baf9f89f1a0c20c9ed34312c24d9d435a92f97f8248` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-005 | `sha256:c4b025450e367bdb32dc080da940c1eff3f04004be600ae9265b31ba22836b91` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-006 | `sha256:43f25ed46a8a7bcefd4c70486f3bbda48cd493c4259e360743c61a84a9b62462` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-007 | `sha256:203204715528832e81608a7cc3edcb10931032ba8a99790f25d0503f3c754dd2` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |

**Prohibición de claim:** hasta confirmación humana y reescaneo posterior, no se afirma
"secret-free", "sin secretos" ni equivalentes. Si se reclasifica un caso como exposición,
se detiene publicación/merge, se rota la credencial y se documenta la remediación sin
copiar el valor secreto.
