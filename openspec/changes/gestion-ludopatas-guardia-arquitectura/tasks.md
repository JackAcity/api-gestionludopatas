## 1. Guardia de arquitectura

- [x] 1.1 Crear `test/GestionLudopatas.Api.Tests/Arquitectura/ArquitecturaTests.cs`.
- [x] 1.2 Implementar helper que obtiene todos los `Type` del assembly de
  `GestionLudopatas.Api` cuyo `Namespace` empieza con
  `GestionLudopatas.Api.Application` o `GestionLudopatas.Api.Domain`.
- [x] 1.3 Implementar la lista de namespaces prohibidos (D1 de design.md):
  `Microsoft.Data.SqlClient`, `System.Data.SqlClient`, `Npgsql`,
  `Microsoft.EntityFrameworkCore`, `RabbitMQ.Client`.
- [x] 1.4 Implementar la inspección: para cada tipo filtrado, revisar
  `GetConstructors`, `GetMethods`, `GetProperties`, `GetFields` (con
  `BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
  BindingFlags.Static`) y verificar que ningún parámetro, tipo de retorno, tipo de
  propiedad o tipo de campo pertenezca a un namespace prohibido.

## 2. Prueba determinística (requirement "Ningún tipo de Application o Domain
   referencia infraestructura concreta")

- [x] 2.1 `[Fact] ApplicationYDomain_NoReferencianInfraestructuraConcreta()` — corre
  contra el código actual, debe pasar en verde (confirma que la auditoría manual fue
  correcta).
- [x] 2.2 `[Fact]` adicional que verifica el mensaje de fallo es identificable (nombre
  de tipo + nombre de miembro infractor) — inyectar temporalmente un tipo de prueba
  con una dependencia prohibida dentro del propio test (no en producción) para
  confirmar que el helper efectivamente detecta el caso, luego remover ese tipo de
  prueba o dejarlo como fixture reusable dentro del archivo de test.

## 3. Verificación

- [x] 3.1 `dotnet test` — 101/101 en verde (99 de línea base + 2 de la guardia), sin
  warnings nuevos; `dotnet build -warnaserror` también terminó con 0 warnings/0 errores.
- [x] 3.2 Confirmar que el helper falla intencionalmente ante una dependencia declarada
  de `Microsoft.Data.SqlClient.SqlConnection`: el fixture privado
  `TipoDePruebaConDependenciaSql` deja la prueba negativa permanente y verifica el
  mensaje con tipo y miembro. Un `using` no usado no aparece en metadata y reflection
  no puede detectarlo; se reemplaza esa prueba manual por la verificación determinística.
