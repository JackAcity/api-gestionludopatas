# GestionLudopatas API

Repositorio del backend que expone los ocho Stored Procedures aprobados de
`bd_autobot` para UiPath mediante una API .NET 10 protegida con API Key, allowlist y
HashiCorp Vault.

## Contenido

- [`api-gestionludopatas/`](api-gestionludopatas/) — código .NET, pruebas, Docker,
  Postman y documentación operativa.
- [`endpoint/`](endpoint/) — contrato fuente y matriz de mapeo SQL ↔ HTTP.
- [`openspec/`](openspec/) — propuestas, decisiones arquitectónicas y tareas trazables.

## Inicio rápido

Consulta primero [`api-gestionludopatas/README.md`](api-gestionludopatas/README.md).
Para el pase pendiente de idempotencia persistente, revisar
[`api-gestionludopatas/database/idempotencia-persistente/PASE_DBA_Y_ARQUITECTURA.md`](api-gestionludopatas/database/idempotencia-persistente/PASE_DBA_Y_ARQUITECTURA.md).
El flujo de calidad, seguridad, despliegue y los pendientes de infraestructura estan en
[`docs/devops/PLAN_CI_CD.md`](docs/devops/PLAN_CI_CD.md).

## Seguridad de versionado

El repositorio contiene solo plantillas y configuración no secreta. `.env`, ambientes
Postman locales, build outputs y configuración local de asistentes están excluidos por
`.gitignore`. Los secretos se resuelven en runtime desde Vault y nunca deben agregarse al
control de versiones.
