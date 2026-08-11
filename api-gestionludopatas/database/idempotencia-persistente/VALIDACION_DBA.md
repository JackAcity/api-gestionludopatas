# Validación DBA — idempotencia persistente de `crearCorte`

> Leer primero [PASE_DBA_Y_ARQUITECTURA.md](PASE_DBA_Y_ARQUITECTURA.md). Este archivo
> es la lista ejecutable de validaciones; el pase explica el motivo y el riesgo de cada
> una.

Este runbook se ejecuta en **DEV** con un payload y una ventana aprobados por el dueño de
datos. No muestra ni guarda credenciales. La API no debe apuntar todavía al procedimiento
nuevo hasta completar estas verificaciones.

## Preflight obligatorio

1. Confirmar que la base propia y `bd_autobot` viven en la misma instancia SQL Server.
2. Confirmar que ninguna tabla de este flujo es memory-optimized.
3. La revisión DEV confirmó que `dbo.SP_CORTE_Crear` es un único `INSERT`, sin
   transacción propia; `dbo.Corte` no tiene triggers y sus defaults completan las demás
   columnas obligatorias. Mantener esta revisión como parte de la evidencia de pase.
4. Revisar AG/mirroring y backup/recovery de las dos bases. La transacción entre bases
   locales requiere que la topología la soporte.
5. El preflight DEV confirmó que el login efectivo de la API no pertenece a roles de
   lectura/escritura/owner; conserva solo los permisos explícitos necesarios. Crear/
   provisionar la base de la API y ejecutar
   `001_instalar_idempotencia_corte.sql` con variables explícitas (`sqlcmd -v`, no
   defaults editados dentro del archivo). Confirmar que el login de la API solo tiene
   `EXECUTE` sobre `api.SP_Corte_Crear_Idempotente`, no DML de tabla.

## Casos funcionales SQL

Usar una clave nueva de 16–128 caracteres y una huella SHA-256 de 32 bytes. Sustituir los
valores de ejemplo por datos DEV autorizados.

| Caso | Acción | Resultado esperado |
|---|---|---|
| Primera llamada | Ejecutar el procedimiento con clave/huella nuevas. | `outcome=created`, `replayed=0`, un `corteId`, `httpStatus=201`. |
| Mismo request | Repetir exactamente clave/huella y payload. | `outcome=replayed`, `replayed=1`, mismo `corteId`, sin una segunda fila de `Corte`. |
| Misma clave, otro payload | Repetir clave con otra huella. | `outcome=conflict`, sin `corteId`, sin crear corte. |
| Concurrencia | Dos sesiones empiezan al mismo tiempo con la misma clave/huella. | Una responde `created`, la otra `replayed`; ambas devuelven el mismo ID. |
| Rollback | En DEV, provocar un error controlado después de entrar al wrapper y antes del commit. | No queda fila en `api.IdempotenciaCrearCorte` ni corte nuevo comprometido. |
| Expiración | Ajustar solo en DEV la expiración de una fila de prueba al pasado y repetir la clave. | La clave se reutiliza como nueva creación conforme a la política aprobada. |

Para la comprobación de concurrencia, mantener la primera sesión bloqueada dentro del
procedimiento mediante un breakpoint/control DBA aprobado; no modificar la API ni
cancelar el proceso Docker como mecanismo de prueba.

## Criterio de salida para el cambio .NET

Entregar al equipo API: nombre de base, evidencia de los seis casos, timeout SQL aprobado,
política de purga y confirmación de que el wrapper participa en la misma transacción. Con
eso se implementa el adaptador .NET y se prueban las mismas propiedades por HTTP/Newman.
