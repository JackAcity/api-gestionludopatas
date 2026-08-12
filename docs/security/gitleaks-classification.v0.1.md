# Registro sanitizado de clasificación Gitleaks v0.1

**Escáner:** Gitleaks v8.30.1.
**Alcance reproducible:** todas las referencias Git del repositorio al 2026-08-11.
**Comando conceptual:** `gitleaks git <repo> --log-opts=--all --redact=100`.
**Regla:** no se almacenan coincidencias, literales ni secretos en este registro.

El escáner informó siete coincidencias de la regla `generic-api-key` en el commit `799324ce8d4a8da21fd8aec72bc4d112d57ae121`. La preclasificación fue confirmada por decisión humana el 2026-08-12 para GL-001 a GL-007; véase el [registro de cierre Gate 1](../architecture/gate-1-human-closure-2026-08-12.md). La decisión no afirma que el repositorio esté libre de secretos.

## Cálculo reproducible de `location_hash`

Para cada hallazgo se calcula SHA-256 de los bytes UTF-8 de esta secuencia, usando LF
(`\n`) como separador y sin terminador final:

```text
commit_sha_completo
normalized_repo_relative_path
rule_id
decimal_start_line
```

`normalized_repo_relative_path` usa `/`, es relativo a la raíz del repositorio y no
contiene `.` ni `..`. El contenido detectado, literal, valor de secreto y columna no
forman parte de la entrada. Esto permite reproducir la ubicación sin revelar el match.

| ID | `location_hash` | Clasificación inicial | Razonamiento sanitizado | Preclasificación | Fecha | Remediación |
|---|---|---|---|---|---|---|
| GL-001 | `sha256:0786ae2be7c8999b8430cf76d9a34368e2fa256ccf226dff7379eb7caba1eebc` | candidate-false-positive | Constante de código de error; no hay asignación de secreto ni valor de alta entropía. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-002 | `sha256:ffe860bdcc1f06ca4bc6ffed47f2937936df62e43465ec347b26e50cd1ff32f3` | candidate-false-positive | Constante de código de error; no hay asignación de secreto ni valor de alta entropía. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-003 | `sha256:5fd297ca9ce359b6e4f91a6e0414eb39699b35580f7d4655a72fe9942bcf23a3` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-004 | `sha256:7b4749f32071385c11dc46ccc8a638a48b2dec70ac6553edffa9b527274f761a` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-005 | `sha256:24a82501899f99da1b8980852e3caeaa57e4cb13c8c4228f672944c3d83b5a74` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-006 | `sha256:e3e391bd7038503d8cb1911c14404325de1eb0b0b2599cecfd688b4e2443c21a` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |
| GL-007 | `sha256:349f1f4f3e3b45c28f3fae1d8e18471ac0df11d6817423dda8f986805e0bcba9` | candidate-false-positive | Código de error de contrato OpenAPI, no credencial. | Codex preclassification; human confirmation required | 2026-08-11 | Ninguna hasta confirmación. |

**Límite de claim:** la confirmación cubre únicamente GL-001 a GL-007 en el commit indicado; no permite afirmar "secret-free", "sin secretos" ni equivalentes sin un reescaneo posterior. Si se reclasifica un caso como exposición, se detiene publicación/merge, se rota la credencial y se documenta la remediación sin copiar el valor secreto.
