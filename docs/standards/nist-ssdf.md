# NIST SSDF

**Fuente normativa:** SRC-NIST-001, NIST SP 800-218 v1.1, final. El uso local es un
mapeo de prácticas y tareas, no una certificación ni una declaración de conformidad.
SSDF v1.2 permanece **Draft** (SRC-NIST-002) y solo informa análisis de brechas.

## Cómo se usa en este modelo

Las cuatro familias —Prepare the Organization (PO), Protect the Software (PS),
Produce Well-Secured Software (PW) y Respond to Vulnerabilities (RV)— no son
controles ejecutables. Cada control SDC debe apuntar a una práctica/tarea concreta y
declarar la interpretación local en la [matriz de trazabilidad](../architecture/source-to-control-traceability.v0.2.yaml).

| Locator SSDF v1.1 | Uso local limitado |
|---|---|
| PS.1.1 | Proteger código y configuración como código con mínimo privilegio. Apoya controles de integración, workflows e identidades. |
| PS.2.1 | Poner información de verificación de integridad a disposición del adquirente. Apoya digests, provenance y verificación de artefactos. |
| PO.5.1 | Separar y proteger cada entorno implicado en desarrollo. Apoya límites runner/ambiente; no prescribe un proveedor ni un modelo de aprobación. |
| PW.4.1 | Adquirir y mantener componentes de software razonablemente seguros. Apoya la gobernanza de dependencias y acciones de terceros. |
| PW.7.1 / PW.8.1 | Determinar y aplicar revisión/análisis y pruebas de código según riesgo. Apoyan validación independiente, no un check concreto de GitHub. |

La formulación normativa exacta permanece en SP 800-218. Las traducciones anteriores
son interpretaciones de ingeniería locales y deben revisarse por perfil de riesgo.
