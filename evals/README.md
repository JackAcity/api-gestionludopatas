# Evaluaciones

Las evaluaciones no ejecutan producción. Cada fixture en `vulnerable/`, `compliant/` o `exceptions/` debe ser autocontenido, no contener secretos reales y declarar el resultado esperado en la matriz. Un auditor es aceptable solo si detecta casos vulnerables y también acepta los casos conformes sin reclamar controles no demostrados.
