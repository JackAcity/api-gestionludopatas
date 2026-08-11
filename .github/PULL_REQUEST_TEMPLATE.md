## Objetivo

<!-- Que cambia y por que. Enlazar issue y/o cambio OpenSpec. -->

## Riesgo y reversa

- Riesgo: <!-- bajo | medio | alto, y justificacion -->
- Reversa: <!-- commit, imagen o procedimiento verificable -->
- Cambia contrato, esquema SQL o secreto: <!-- no | si, enlace al runbook/aprobacion -->

## Evidencia

- [ ] `dotnet format --verify-no-changes`
- [ ] `dotnet build -warnaserror`
- [ ] `dotnet test`
- [ ] CI y DevSecOps verdes
- [ ] Prueba de contrato o razon documentada si no aplica

## Seguridad

- [ ] No se agregaron secretos, datos personales ni valores de ambiente.
- [ ] Revise permisos, entradas externas, logs y dependencias afectadas.
