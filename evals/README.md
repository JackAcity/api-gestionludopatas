# Evaluaciones

Las evaluaciones no ejecutan producción. Cada fixture en `vulnerable/`, `compliant/` o `exceptions/` debe ser autocontenido, no contener secretos reales y declarar el resultado esperado en la matriz. Un auditor es aceptable solo si detecta casos vulnerables y también acepta los casos conformes sin reclamar controles no demostrados.

El inventario de casos semilla está en [`matrix.v0.2.yaml`](matrix.v0.2.yaml); el plan
que cubre cada ruta TM sin exigir ejecutar 100 fixtures está en
[`threat-to-evaluation-coverage.v0.1.yaml`](threat-to-evaluation-coverage.v0.1.yaml).
Los vectores de Evidence Bundle y su comando determinista están en
[`evidence/`](evidence/).
