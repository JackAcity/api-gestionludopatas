## ADDED Requirements

### Requirement: No existen carpetas vacías bajo el código fuente del proyecto
El árbol de directorios de `src/GestionLudopatas.Api/` (excluyendo `bin/` y `obj/`) SHALL NOT contener ninguna carpeta sin archivos.

#### Scenario: Verificación de carpetas vacías
- **WHEN** se recorre recursivamente `src/GestionLudopatas.Api/` excluyendo `bin/` y `obj/`
- **THEN** ninguna carpeta encontrada está vacía

### Requirement: La capa de entrada HTTP vive bajo una única carpeta `Api/`
Los archivos de Endpoints, Middleware y Security SHALL residir bajo `src/GestionLudopatas.Api/Api/` (`Api/Endpoints/`, `Api/Middleware/`, `Api/Security/`), sin dejar carpetas sueltas `Endpoints/`, `Middleware/` o `Security/` al mismo nivel que `Domain/`/`Application/`/`Infrastructure/`.

#### Scenario: Endpoints, Middleware y Security bajo Api/
- **WHEN** se listan las carpetas de primer nivel dentro de `src/GestionLudopatas.Api/`
- **THEN** no existen `Endpoints/`, `Middleware/` ni `Security/` como carpetas de primer nivel
- **AND** existen `Api/Endpoints/`, `Api/Middleware/` y `Api/Security/` con los archivos correspondientes

### Requirement: `Program.cs` no contiene lógica de bootstrap de secretos inline
`Program.cs` SHALL limitarse a construir el `WebApplicationBuilder`, registrar servicios, construir la app y mapear el pipeline/endpoints — SHALL NOT contener la resolución de secretos de Vault (lectura de `Vault:Address`/`Token`/`PathDb`/`PathApiKey`, llamadas a `VaultSecretClient`) inline.

#### Scenario: Bootstrap de Vault extraído
- **WHEN** se inspecciona el contenido de `Program.cs`
- **THEN** no contiene el literal de configuración `"Vault:Address"` ni instancia `VaultSecretClient` directamente
- **AND** invoca un método de extensión (`CargarSecretosSiHabilitadoAsync` o equivalente) definido en `Infrastructure/Vault/`

### Requirement: El movimiento de archivos no cambia el comportamiento observable
Después de mover Endpoints/Middleware/Security bajo `Api/` y extraer el bootstrap de Vault, el comportamiento HTTP y el pipeline de middleware SHALL ser idéntico al existente antes del cambio.

#### Scenario: Suite de tests completa sigue en verde
- **WHEN** se ejecuta `dotnet test` después de aplicar este change
- **THEN** todos los tests existentes (línea base 76/76) siguen pasando, sin tests removidos ni deshabilitados
