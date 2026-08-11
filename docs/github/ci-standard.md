# Adaptador GitHub: estándar CI (propuesto)

Mapea SDC-002: build desde fuente limpia, pruebas, validación de contrato y evidencia descargable ligada a SHA. El contenido exacto depende del servicio; no toda API necesita los mismos analizadores. Un cambio de DB, contrato o seguridad incorpora su prueba específica y no se oculta bajo un build verde genérico.

Gates candidatos se seleccionan por riesgo y se prueban con fixtures que fallan deliberadamente. Cache, locks, cobertura y análisis estático se implementan después de medir su reproducibilidad/coste.
