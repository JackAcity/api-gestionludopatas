# SLSA

**Fuente:** SRC-SLSA-001, SLSA v1.2 approved. SLSA aporta tracks y requisitos
incrementales para cadena de suministro. Aquí se usa para diseñar SDC-006; no se
declara nivel SLSA hasta que evidencia y verificación del consumidor satisfagan los
requisitos concretos.

| Locator SLSA v1.2 | Interpretación local |
|---|---|
| Build L1 — Provenance exists | El artefacto debe tener digest y provenance que describa builder, proceso e inputs. |
| Build L2 — Hosted build platform | Si se pretende proteger contra manipulación posterior al build, la provenance debe ser firmada por la plataforma y el consumidor debe validar autenticidad. |
| Build requirements — Producer / consistent build process | La repetibilidad es una expectativa de proceso, no una afirmación automática de reproducibilidad bit a bit. |
| Build requirements — Consumer validation | Generar una attestation no basta: el consumidor debe validarla antes de confiar. |

El objetivo actual es verificar propiedades de artefacto, no afirmar SLSA Build L1,
L2 o L3. La plataforma, su aislamiento y el consumidor todavía son decisiones abiertas.
