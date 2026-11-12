# Evidencias de la Actividad 1

## Entorno

- Worker Service: .NET 8.
- Base de datos: SQL Server 2025, base `OpinionesWebDB`, puerto 14330.
- API REST: ASP.NET Core .NET 8, `http://localhost:5080`.
- Salida temporal: `data/extracted`.
- Registros: 900, errores: 0, duración: 321 ms.

## Resultados por fuente

| Fuente | Tecnología | Registros | Errores | Duración |
|---|---|---:|---:|---:|
| Encuestas internas | CsvHelper | 500 | 0 | 37 ms |
| Reseñas web | ADO.NET | 200 | 0 | 301 ms |
| Comentarios sociales | HttpClient | 200 | 0 | 18 ms |
| Total | | 900 | 0 | 321 ms |

## Archivos generados

- `data/extracted/encuestas.json`: 500 registros.
- `data/extracted/resenas_web.json`: 200 registros.
- `data/extracted/comentarios_sociales.json`: 200 registros.
- `data/extracted/manifest.json`: totales, errores y duración por fuente.

## API REST

- `GET /health`: estado del servicio y total de comentarios.
- `GET /api/comentarios`: lista completa.
- `GET /api/comentarios?plataforma=Instagram`: filtro por plataforma.
- `GET /api/comentarios/{id}`: comentario individual o 404.
- Documentación Swagger: `http://localhost:5080/swagger`.

La API respondió con 200 comentarios y el endpoint de salud confirmó el estado del servicio.

## Diagramas y documento

- `docs/diagramas/arquitectura.png`
- `docs/diagramas/flujo.png`
- `docs/diagramas/swagger.png`
- `docs/Informe_Actividad1.docx`
- `docs/Informe_Actividad1.pdf`

## Logs

- `logs/extraccion-20261112.log`: ejecución del Worker Service.
- `logs/api.log`: arranque de la API y carga de los 200 comentarios.

## Nota

La cadena de conexión se suministra por la variable de entorno `ConnectionStrings__OpinionesWebDB` y no se incluye en los archivos de la entrega.
