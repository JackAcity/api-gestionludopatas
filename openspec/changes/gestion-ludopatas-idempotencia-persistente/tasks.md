## 1. Preflight y diseño

- [x] 1.1 Confirmar con una consulta de solo lectura el mínimo privilegio del login DEV:
  `CREATE TABLE=0`, `EXECUTE SP_CORTE_Crear=1`, `VIEW DEFINITION=0`.
- [x] 1.1a Inspección DBA: SQL Server 2022 sin Always On, `dbo.Corte` convencional,
  sin clave natural; `SP_CORTE_Crear` hace un único `INSERT` y retorna el ID como result
  set. No hay triggers y las columnas obligatorias restantes tienen defaults locales.
  El paquete captura ese result set con `INSERT ... EXEC`.
- [x] 1.2 Documentar la decisión transaccional, los límites y las preguntas que solo
  puede responder DBA (`design.md`).
- [x] 1.3 Preparar un paquete SQL que no modifica `bd_autobot` ni se ejecuta desde la
  API (`database/idempotencia-persistente/`).

## 2. Aprobación y ejecución DBA

- [ ] 2.1 DBA provisiona una base propia normal en la misma instancia SQL Server y
  confirma topología compatible para una transacción cross-database local.
- [ ] 2.2 DBA revisa los parámetros reales y la semántica transaccional de
  `dbo.SP_CORTE_Crear`; ejecuta la instalación en DEV.
- [ ] 2.3 DBA realiza los casos de `VALIDACION_DBA.md`, incluida una ejecución que se
  fuerce a rollback antes del commit y una repetición concurrente.

## 3. Adaptación .NET

- [ ] 3.1 Crear el puerto semántico `ICreadorCorteIdempotente` en Application y mover
  la coordinación de `crearCorte` fuera del endpoint, sin filtrar SQL/HTTP al núcleo.
- [ ] 3.2 Implementar el adaptador SQL que llama
  `api.SP_Corte_Crear_Idempotente` con `Task`, `CancellationToken` y fingerprint
  SHA-256 binario.
- [ ] 3.3 Reemplazar el singleton en memoria tras la certificación DEV; conservar el
  contrato `201`, replay y `409 GL-IDEMP-001`.

## 4. Verificación de salida

- [ ] 4.1 Pruebas unitarias: creado, replay, conflicto, propagación de cancelación y
  excepción SQL inesperada.
- [ ] 4.2 Prueba DEV concurrente con barrera: dos solicitudes iguales producen un solo
  corte; la segunda recibe `Idempotency-Replayed: true`.
- [ ] 4.3 Prueba DEV de recuperación: simular caída/controlar cancelación alrededor de
  la llamada y demostrar que el reintento conserva el mismo `corteId`.
- [ ] 4.4 Ejecutar suite completa, build sin warnings y colección Newman de contrato.
