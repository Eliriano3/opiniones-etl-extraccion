# Actividad 1. Arquitectura y proceso de extracción ETL

Estudiante: Eduardo Liriano  
Matrícula: 20231017  
Profesor: Francis Ramírez  
Big Data, ITLA

## Descripción

La entrega contiene un Worker Service en .NET 8 que extrae datos de tres fuentes del proyecto Sistema de Análisis de Opiniones de Clientes: un archivo CSV de encuestas, una base de datos SQL Server y una API REST de comentarios sociales. Los datos se guardan en archivos JSON temporales para la siguiente etapa del pipeline.

## Estructura

- `src/Opiniones.Extraction.Domain`: contratos y modelos.
- `src/Opiniones.Extraction.Application`: orquestador de extracción.
- `src/Opiniones.Extraction.Infrastructure`: extractores, DataLoader y LoggerService.
- `src/Opiniones.Extraction.Worker`: Worker Service .NET 8.
- `src/Opiniones.Api`: API REST de comentarios sociales.
- `data/input`: archivos CSV de origen.
- `data/extracted`: evidencia de los 900 registros extraídos.
- `docs`: informe técnico, diagramas y evidencias.
- `logs`: registros de la ejecución.

## Resultados

| Fuente | Registros | Errores |
|---|---:|---:|
| Encuestas CSV | 500 | 0 |
| Reseñas web | 200 | 0 |
| Comentarios sociales | 200 | 0 |
| Total | 900 | 0 |

La ejecución completa tardó 321 ms.

## API REST

La API está en `http://localhost:5080`, con Swagger en `/swagger` y los endpoints `/health`, `/api/comentarios` y `/api/comentarios/{id}`.

## Repositorio

El código fuente completo está en esta carpeta y en el repositorio Git local del proyecto. La publicación en GitHub se realiza con la cuenta del estudiante.
