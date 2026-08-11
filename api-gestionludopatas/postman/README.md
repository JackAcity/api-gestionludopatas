# Validación operativa con Newman

La colección `GestionLudopatas.contract.postman_collection.json` cubre una regla de
negocio por cada una de las ocho operaciones. Sus solicitudes son deliberadamente
inválidas: deben devolver `422` con el `code` `GL-*` exacto, `application/problem+json`
y las cabeceras de trazabilidad. No crean cortes ni ejecutan un Stored Procedure, por lo
que sirven como canario del contrato aun cuando el login de `bd_autobot` no esté listo.

## Antes de ejecutar

1. Use una máquina con acceso a DEV y con una API key permitida por la allowlist.
   La plantilla apunta a `http://10.99.200.100:9012`; cambie `baseUrl` solo si ejecuta
   una instancia local (el puerto del compose actual es `9012`, no `8092`).
2. Cree el environment local desde la plantilla y complete `baseUrl`, `apiKey`,
   `correlationId` e `idempotencyKey`. El archivo local está ignorado para no confirmar
   una API key en el repositorio:

   ```powershell
   Copy-Item .\GestionLudopatas.postman_environment.json .\GestionLudopatas.local.postman_environment.json
   ```
3. Instale Newman 6: `npm install --global newman@6`.

## Ejecución

Desde esta carpeta:

```powershell
.\Ejecutar-Contrato.ps1
```

O indicando un environment local separado:

```powershell
.\Ejecutar-Contrato.ps1 -EnvironmentFile .\GestionLudopatas.local.postman_environment.json
```

El resultado esperado es 8 solicitudes y 8 assertions de contrato aprobadas. Un `401` o
`403` señala configuración de acceso; un `500` indica que la prevalidación dejó de ocurrir
antes de SQL; un `422` con otro código es una regresión del contrato.

## Lectura funcional segura (`200`)

La colección `GestionLudopatas.readonly.postman_collection.json` ejecuta únicamente las
búsquedas SICA de ingreso y salida. Ambas son SP de solo lectura, sin reserva de filas;
por ello se pueden ejecutar en DEV sin crear ni alterar datos. Un arreglo `[]` es un
resultado `200` válido.

```powershell
.\Ejecutar-Contrato.ps1 -CollectionFile .\GestionLudopatas.readonly.postman_collection.json
```

El resultado esperado es 2 solicitudes y 2 assertions aprobadas, con `200`, un arreglo
JSON y las cabeceras de trazabilidad. No use la colección histórica de operaciones
exitosas para este fin: incluye creación de cortes y exige aprobación operacional.

La colección histórica `GestionLudopatas.postman_collection.json` se conserva para las
operaciones exitosas y sus ejemplos. No se ejecuta automáticamente: incluye `POST /cortes`
y depende tanto de datos aprobados como del login real de `bd_autobot`.
