from pathlib import Path

from docx import Document
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Inches, Pt, RGBColor

BASE = Path(__file__).resolve().parent
DIAGRAMAS = BASE / "diagramas"
DOCX = BASE / "Informe_Actividad1.docx"

AZUL = RGBColor(0x10, 0x2A, 0x43)
VERDE = RGBColor(0x0B, 0x6B, 0x33)
GRIS = RGBColor(0x52, 0x60, 0x6D)


def sombrear_celda(celda, color):
    tc_pr = celda._tc.get_or_add_tcPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:fill"), color)
    tc_pr.append(shd)


def bordes_tabla(tabla, color="D9E2EC", tamano="4"):
    tbl = tabla._tbl
    tbl_pr = tbl.tblPr
    borders = OxmlElement("w:tblBorders")
    for borde in ("top", "left", "bottom", "right", "insideH", "insideV"):
        elemento = OxmlElement(f"w:{borde}")
        elemento.set(qn("w:val"), "single")
        elemento.set(qn("w:sz"), tamano)
        elemento.set(qn("w:space"), "0")
        elemento.set(qn("w:color"), color)
        borders.append(elemento)
    tbl_pr.append(borders)


def ancho_columnas(tabla, anchos):
    for fila in tabla.rows:
        for indice, ancho in enumerate(anchos):
            fila.cells[indice].width = Inches(ancho)


def texto_celda(celda, texto, negrita=False, tamano=9.5):
    celda.text = ""
    parrafo = celda.paragraphs[0]
    parrafo.paragraph_format.space_after = Pt(2)
    parrafo.paragraph_format.space_before = Pt(2)
    run = parrafo.add_run(str(texto))
    run.bold = negrita
    run.font.size = Pt(tamano)


def tabla_encabezado(documento, encabezados, filas, anchos):
    tabla = documento.add_table(rows=1, cols=len(encabezados))
    tabla.alignment = WD_TABLE_ALIGNMENT.CENTER
    tabla.style = "Table Grid"
    bordes_tabla(tabla)

    fila_encabezado = tabla.rows[0]._tr
    tr_pr = fila_encabezado.get_or_add_trPr()
    repetir = OxmlElement("w:tblHeader")
    tr_pr.append(repetir)

    for indice, encabezado in enumerate(encabezados):
        celda = tabla.rows[0].cells[indice]
        sombrear_celda(celda, "D9EFDF")
        texto_celda(celda, encabezado, negrita=True, tamano=9.5)

    for fila in filas:
        celdas = tabla.add_row().cells
        for indice, valor in enumerate(fila):
            texto_celda(celdas[indice], valor, tamano=9.5)

    ancho_columnas(tabla, anchos)
    documento.add_paragraph()
    return tabla


def agregar_codigo(documento, codigo):
    tabla = documento.add_table(rows=1, cols=1)
    tabla.alignment = WD_TABLE_ALIGNMENT.CENTER
    tabla.style = "Table Grid"
    bordes_tabla(tabla, color="D9E2EC", tamano="4")

    tr_pr = tabla.rows[0]._tr.get_or_add_trPr()
    sin_division = OxmlElement("w:cantSplit")
    tr_pr.append(sin_division)

    celda = tabla.rows[0].cells[0]
    celda.width = Inches(6.4)
    sombrear_celda(celda, "F5F7FA")
    celda.text = ""

    parrafo = celda.paragraphs[0]
    parrafo.paragraph_format.space_before = Pt(4)
    parrafo.paragraph_format.space_after = Pt(4)
    parrafo.paragraph_format.line_spacing = 1.0

    for linea in codigo.split("\n"):
        run = parrafo.add_run(linea)
        run.font.name = "Consolas"
        run.font.size = Pt(8.5)
        run._element.rPr.rFonts.set(qn("w:eastAsia"), "Consolas")
        parrafo.add_run().add_break()

    documento.add_paragraph()


def agregar_figura(documento, ruta, ancho, leyenda):
    parrafo = documento.add_paragraph()
    parrafo.alignment = WD_ALIGN_PARAGRAPH.CENTER
    parrafo.paragraph_format.space_before = Pt(6)
    parrafo.paragraph_format.space_after = Pt(2)
    parrafo.add_run().add_picture(str(ruta), width=Inches(ancho))

    caption = documento.add_paragraph()
    caption.alignment = WD_ALIGN_PARAGRAPH.CENTER
    caption.paragraph_format.space_after = Pt(10)
    run = caption.add_run(leyenda)
    run.italic = True
    run.font.size = Pt(9)
    run.font.color.rgb = GRIS


def agregar_titulo(documento, texto, nivel=1):
    parrafo = documento.add_heading(texto, level=nivel)
    for run in parrafo.runs:
        run.font.color.rgb = AZUL if nivel == 1 else VERDE
    return parrafo


def agregar_parrafo(documento, texto, negrita=False, cursiva=False, tamano=11):
    parrafo = documento.add_paragraph()
    parrafo.paragraph_format.space_after = Pt(6)
    parrafo.paragraph_format.line_spacing = 1.15
    run = parrafo.add_run(texto)
    run.bold = negrita
    run.italic = cursiva
    run.font.size = Pt(tamano)
    return parrafo


def agregar_bullet(documento, texto):
    parrafo = documento.add_paragraph(style="List Bullet")
    parrafo.paragraph_format.space_after = Pt(3)
    run = parrafo.add_run(texto)
    run.font.size = Pt(11)


def numero_pagina(parrafo):
    run = parrafo.add_run()
    fld_begin = OxmlElement("w:fldChar")
    fld_begin.set(qn("w:fldCharType"), "begin")
    instr = OxmlElement("w:instrText")
    instr.set(qn("xml:space"), "preserve")
    instr.text = "PAGE"
    fld_end = OxmlElement("w:fldChar")
    fld_end.set(qn("w:fldCharType"), "end")
    run._r.append(fld_begin)
    run._r.append(instr)
    run._r.append(fld_end)
    run.font.size = Pt(9)
    run.font.color.rgb = GRIS


documento = Document()

seccion = documento.sections[0]
seccion.page_width = Inches(8.5)
seccion.page_height = Inches(11)
seccion.top_margin = Inches(1)
seccion.bottom_margin = Inches(1)
seccion.left_margin = Inches(1)
seccion.right_margin = Inches(1)
seccion.different_first_page_header_footer = True

estilo_normal = documento.styles["Normal"]
estilo_normal.font.name = "Calibri"
estilo_normal.font.size = Pt(11)
estilo_normal.paragraph_format.space_after = Pt(6)

for nivel in (1, 2):
    estilo = documento.styles[f"Heading {nivel}"]
    estilo.font.name = "Calibri"
    estilo.font.color.rgb = AZUL if nivel == 1 else VERDE
    estilo.font.size = Pt(15 if nivel == 1 else 12.5)
    estilo.paragraph_format.space_before = Pt(12 if nivel == 1 else 8)
    estilo.paragraph_format.space_after = Pt(4)

encabezado = seccion.header.paragraphs[0]
encabezado.alignment = WD_ALIGN_PARAGRAPH.RIGHT
run = encabezado.add_run("Actividad 1 | Extracción ETL")
run.font.size = Pt(9)
run.font.color.rgb = GRIS

pie = seccion.footer.paragraphs[0]
pie.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = pie.add_run("Eduardo Liriano | 20231017 | ")
run.font.size = Pt(9)
run.font.color.rgb = GRIS
numero_pagina(pie)

# Portada
for _ in range(5):
    documento.add_paragraph()

titulo = documento.add_paragraph()
titulo.alignment = WD_ALIGN_PARAGRAPH.LEFT
run = titulo.add_run("Actividad 1")
run.bold = True
run.font.size = Pt(30)
run.font.color.rgb = VERDE
run.font.name = "Calibri"

subtitulo = documento.add_paragraph()
run = subtitulo.add_run("Arquitectura y proceso de extracción ETL")
run.font.size = Pt(22)
run.font.color.rgb = AZUL
run.font.name = "Calibri"

proyecto = documento.add_paragraph()
run = proyecto.add_run("Sistema de Análisis de Opiniones de Clientes")
run.font.size = Pt(14)
run.font.color.rgb = GRIS

documento.add_paragraph()
linea = documento.add_paragraph()
linea.paragraph_format.space_after = Pt(18)
p_pr = linea._p.get_or_add_pPr()
borde = OxmlElement("w:pBdr")
inferior = OxmlElement("w:bottom")
inferior.set(qn("w:val"), "single")
inferior.set(qn("w:sz"), "12")
inferior.set(qn("w:space"), "1")
inferior.set(qn("w:color"), "34A853")
borde.append(inferior)
p_pr.append(borde)

for etiqueta, valor in (
    ("Estudiante", "Eduardo Liriano"),
    ("Matrícula", "20231017"),
    ("Profesor", "Francis Ramírez"),
    ("Asignatura", "Big Data"),
    ("Institución", "ITLA"),
    ("Fecha", "12 de noviembre de 2026"),
):
    parrafo = documento.add_paragraph()
    parrafo.paragraph_format.space_after = Pt(4)
    run = parrafo.add_run(f"{etiqueta}: ")
    run.bold = True
    run.font.size = Pt(11)
    run.font.color.rgb = AZUL
    run = parrafo.add_run(valor)
    run.font.size = Pt(11)

documento.add_page_break()

# Contenido
agregar_titulo(documento, "Contenido")
for item in (
    "1. Introducción",
    "2. Arquitectura de la solución",
    "3. Flujo del proceso de extracción",
    "4. Implementación del proceso",
    "5. Atributos de calidad",
    "6. Justificación técnica",
    "7. Resultados de la ejecución",
    "8. Conclusiones",
):
    parrafo = documento.add_paragraph()
    parrafo.paragraph_format.space_after = Pt(3)
    run = parrafo.add_run(item)
    run.font.size = Pt(11)

documento.add_page_break()

# 1. Introducción
agregar_titulo(documento, "1. Introducción")
agregar_parrafo(
    documento,
    "Este documento presenta la arquitectura y la implementación de la etapa de extracción "
    "del proyecto Sistema de Análisis de Opiniones de Clientes. El alcance corresponde a la "
    "E del proceso ETL: obtener los datos de las tres fuentes del proyecto, validar su "
    "estructura y guardarlos en archivos temporales para la etapa de transformación y carga.",
)
agregar_parrafo(
    documento,
    "Las fuentes del proyecto son un archivo CSV con encuestas internas de satisfacción, una "
    "base de datos relacional con reseñas publicadas en el sitio web y una API REST con "
    "comentarios de redes sociales. Para esta actividad se desarrolló un Worker Service en "
    ".NET 8 y una API REST propia que expone los comentarios sociales.",
)
agregar_parrafo(
    documento,
    "La solución no carga todavía las tablas analíticas. El resultado de la extracción son "
    "archivos JSON por fuente y un manifiesto con los totales, los errores y los tiempos de "
    "cada extractor. Esa salida es la entrada de la siguiente etapa del pipeline.",
)

# 2. Arquitectura
agregar_titulo(documento, "2. Arquitectura de la solución")
agregar_parrafo(
    documento,
    "La arquitectura se organizó en capas para separar el dominio, la orquestación, la "
    "infraestructura de acceso a las fuentes y el punto de entrada del Worker Service. La "
    "Figura 1 muestra los componentes principales y el recorrido de los datos.",
)
agregar_figura(
    documento,
    DIAGRAMAS / "arquitectura.png",
    6.3,
    "Figura 1. Arquitectura de la solución de extracción.",
)

documento.add_page_break()

agregar_titulo(documento, "2.1 Proyectos de la solución", nivel=2)
tabla_encabezado(
    documento,
    ("Proyecto", "Responsabilidad"),
    (
        ("Opiniones.Extraction.Domain", "Modelos, enumeraciones, IExtractor e IDataLoader."),
        ("Opiniones.Extraction.Application", "Orquestador, opciones de ejecución y coordinación de los extractores."),
        ("Opiniones.Extraction.Infrastructure", "CsvExtractor, DatabaseExtractor, ApiExtractor, JsonDataLoader y LoggerService."),
        ("Opiniones.Extraction.Worker", "Worker Service .NET 8, configuración, inyección de dependencias y arranque."),
        ("Opiniones.Api", "API REST .NET 8 con los comentarios sociales y documentación Swagger."),
    ),
    (2.2, 4.2),
)

agregar_titulo(documento, "2.2 Componentes", nivel=2)
tabla_encabezado(
    documento,
    ("Componente", "Función"),
    (
        ("Worker Service", "Proceso de larga duración que ejecuta la extracción una vez y finaliza."),
        ("Orquestador", "Invoca los extractores en paralelo, recolecta resultados y mide los tiempos."),
        ("CsvExtractor", "Lee el CSV de encuestas con CsvHelper y valida encabezados y filas."),
        ("DatabaseExtractor", "Consulta las reseñas de OpinionesWebDB con ADO.NET."),
        ("ApiExtractor", "Consume la API REST con HttpClient, timeout y reintentos."),
        ("JsonDataLoader", "Guarda un JSON por fuente y un manifiesto con los totales."),
        ("LoggerService", "Registra eventos y errores con ILogger y Serilog."),
    ),
    (2.0, 4.4),
)

agregar_parrafo(
    documento,
    "El Worker Service es el punto de entrada. El orquestador recibe las implementaciones de "
    "IExtractor mediante inyección de dependencias y las ejecuta con Task.WhenAll. Cada "
    "extractor conoce una sola fuente y devuelve un objeto ExtraccionResult con los registros, "
    "los errores y la duración. El DataLoader escribe la salida temporal y el LoggerService "
    "centraliza el registro de la ejecución.",
)
agregar_parrafo(
    documento,
    "La API REST se implementó como un proyecto independiente. Expone los comentarios sociales "
    "en el endpoint /api/comentarios, permite filtrar por plataforma y publica la documentación "
    "interactiva en Swagger. El ApiExtractor la consume por HTTP, de modo que la fuente de "
    "comentarios se comporta como un servicio externo real.",
)

# 3. Flujo
agregar_titulo(documento, "3. Flujo del proceso de extracción")
agregar_parrafo(
    documento,
    "El flujo comienza cuando el Worker Service carga la configuración y construye el "
    "orquestador. Después se ejecutan los tres extractores en paralelo y se recolectan sus "
    "resultados. Al final se escriben los archivos temporales, se registra el resumen y el "
    "proceso termina.",
)
agregar_figura(
    documento,
    DIAGRAMAS / "flujo.png",
    6.3,
    "Figura 2. Flujo del proceso de extracción.",
)
for paso in (
    "Se cargan las rutas, la URL de la API y la cadena de conexión desde appsettings.json y las variables de entorno.",
    "El orquestador ejecuta CsvExtractor, DatabaseExtractor y ApiExtractor con Task.WhenAll.",
    "Cada extractor captura sus errores y devuelve un resultado independiente, sin detener a los demás.",
    "El orquestador reúne los resultados y calcula los totales y la duración de la ejecución.",
    "JsonDataLoader escribe encuestas.json, resenas_web.json, comentarios_sociales.json y manifest.json.",
    "LoggerService registra el resumen y el proceso termina.",
):
    agregar_bullet(documento, paso)

# 4. Implementación
agregar_titulo(documento, "4. Implementación del proceso")

agregar_titulo(documento, "4.1 Contrato de extracción y composición", nivel=2)
agregar_parrafo(
    documento,
    "Todos los extractores implementan la interfaz IExtractor. Esto permite agregar una nueva "
    "fuente sin modificar el orquestador, siempre que la nueva clase respete el contrato.",
)
agregar_codigo(
    documento,
    """public interface IExtractor
{
    FuenteDatos Fuente { get; }
    string Nombre { get; }
    Task<ExtraccionResult> ExtraerAsync(CancellationToken cancellationToken);
}""",
)
agregar_parrafo(
    documento,
    "El registro de las implementaciones se realiza en el Program del Worker Service. El "
    "orquestador recibe IEnumerable<IExtractor> y el DataLoader se registra como singleton.",
)
agregar_codigo(
    documento,
    """builder.Services.AddTransient<IExtractor, CsvExtractor>();
builder.Services.AddTransient<IExtractor, DatabaseExtractor>();
builder.Services.AddTransient<IExtractor, ApiExtractor>();
builder.Services.AddSingleton<IDataLoader, JsonDataLoader>();
builder.Services.AddSingleton<IOrquestadorExtraccion, OrquestadorExtraccion>();
builder.Services.AddHttpClient("comentarios", (sp, cliente) =>
{
    var opciones = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
    cliente.BaseAddress = new Uri(opciones.BaseUrl);
    cliente.Timeout = TimeSpan.FromSeconds(opciones.TimeoutSegundos);
});""",
)

agregar_titulo(documento, "4.2 Extracción del CSV", nivel=2)
agregar_parrafo(
    documento,
    "CsvExtractor usa CsvHelper para leer data/input/encuestas.csv. Antes de procesar las "
    "filas valida que el encabezado tenga las ocho columnas esperadas y que los nombres "
    "coincidan. Cada fila se convierte a un objeto EncuestaCsv y luego a un JsonObject. Los "
    "errores de una fila se registran sin detener el archivo.",
)

agregar_titulo(documento, "4.3 Extracción de la base de datos", nivel=2)
agregar_parrafo(
    documento,
    "DatabaseExtractor abre una conexión con Microsoft.Data.SqlClient y ejecuta una consulta "
    "fija sobre OpinionesWebDB. La consulta une Resena con Cliente y Producto para devolver "
    "los datos planos que necesita la etapa de transformación.",
)
agregar_codigo(
    documento,
    """SELECT r.IdResena,
       c.Email,
       p.Codigo,
       r.Calificacion,
       r.Titulo,
       r.Comentario,
       r.FechaPublicacion,
       r.Estado
  FROM dbo.Resena r
  INNER JOIN dbo.Cliente c  ON c.IdCliente = r.IdCliente
  INNER JOIN dbo.Producto p ON p.IdProducto = r.IdProducto
 ORDER BY r.IdResena;""",
)
agregar_parrafo(
    documento,
    "La cadena de conexión se obtiene de ConnectionStrings__OpinionesWebDB o de la "
    "configuración del entorno. No está escrita en el código ni se incluye en el repositorio.",
)

agregar_titulo(documento, "4.4 Extracción de la API REST", nivel=2)
agregar_parrafo(
    documento,
    "La API REST es un proyecto ASP.NET Core en .NET 8. Carga los 200 comentarios sociales "
    "del proyecto, los expone en /api/comentarios y documenta los endpoints con Swagger. "
    "ApiExtractor usa IHttpClientFactory, valida la respuesta HTTP, interpreta el arreglo JSON "
    "y reintenta hasta tres veces con espera exponencial si hay un fallo temporal.",
)
agregar_codigo(
    documento,
    """using var respuesta = await cliente.GetAsync(_opciones.Endpoint, cancellationToken);
respuesta.EnsureSuccessStatusCode();

var contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);
var nodo = JsonNode.Parse(contenido);

if (nodo is not JsonArray arreglo)
    throw new InvalidOperationException("La API no devolvio un arreglo JSON.");""",
)
agregar_figura(
    documento,
    DIAGRAMAS / "swagger.png",
    5.9,
    "Figura 3. Documentación interactiva de la API de comentarios en Swagger.",
)

agregar_titulo(documento, "4.5 Salida temporal y registros", nivel=2)
agregar_parrafo(
    documento,
    "JsonDataLoader escribe un archivo por fuente y un manifiesto. El nombre de cada archivo "
    "depende de la fuente: encuestas.json, resenas_web.json y comentarios_sociales.json. El "
    "manifiesto incluye la fecha, el total de registros, el total de errores y el detalle de "
    "cada fuente con su duración.",
)
agregar_parrafo(
    documento,
    "LoggerService utiliza ILogger con Serilog como proveedor. La salida se escribe en la "
    "consola y en un archivo rotativo dentro de logs/. Cada extracción registra el inicio, el "
    "número de registros, los errores y la duración.",
)

agregar_titulo(documento, "4.6 Configuración y seguridad", nivel=2)
agregar_parrafo(
    documento,
    "La configuración no está quemada en el código. Las rutas, la URL de la API, los tiempos "
    "de espera y los reintentos se leen de appsettings.json. La cadena de conexión se inyecta "
    "mediante variables de entorno, de modo que el repositorio no contiene credenciales. La "
    "consulta SQL es fija y no concatena valores de entrada, lo que evita inyección SQL.",
)

# 5. Atributos de calidad
agregar_titulo(documento, "5. Atributos de calidad")
tabla_encabezado(
    documento,
    ("Atributo", "Implementación", "Evidencia"),
    (
        (
            "Rendimiento",
            "Extracción asíncrona y en paralelo con Task.WhenAll. Medición con Stopwatch.",
            "900 registros en 321 ms; cada fuente reporta su tiempo.",
        ),
        (
            "Escalabilidad",
            "Interfaz IExtractor, inyección de dependencias y configuración externa.",
            "Agregar una fuente requiere una clase nueva y su registro, sin tocar el orquestador.",
        ),
        (
            "Seguridad",
            "Credenciales por variables de entorno, consulta SQL fija, timeout y validación de respuestas.",
            "La cadena de conexión no está en el código ni en appsettings.json.",
        ),
        (
            "Mantenibilidad",
            "Separación en capas, interfaces, responsabilidades únicas y logs estructurados.",
            "Dominio, aplicación, infraestructura, Worker y API son proyectos independientes.",
        ),
    ),
    (1.2, 3.0, 2.2),
)
agregar_parrafo(
    documento,
    "El rendimiento se apoya en operaciones asíncronas y en la ejecución simultánea de las "
    "tres fuentes. La fuente más lenta fue la base de datos, con 301 ms, mientras que el CSV "
    "y la API terminaron en 37 ms y 18 ms. El tiempo total de la ejecución fue de 321 ms.",
)
agregar_parrafo(
    documento,
    "La escalabilidad se logra con el contrato IExtractor y el registro en el contenedor de "
    "dependencias. Una fuente nueva, por ejemplo un archivo Excel o un servicio de mensajería, "
    "solo necesita una implementación nueva y su configuración.",
)

# 6. Justificación
agregar_titulo(documento, "6. Justificación técnica")
agregar_parrafo(
    documento,
    "La separación en capas se eligió para que el Worker Service no dependa de los detalles de "
    "cada fuente. El dominio define los contratos, la aplicación coordina y la infraestructura "
    "implementa el acceso a CSV, SQL Server y HTTP. Esta separación facilita las pruebas y "
    "permite cambiar una fuente sin reescribir el proceso completo.",
)
agregar_parrafo(
    documento,
    "La extracción en paralelo reduce el tiempo total cuando las fuentes son independientes. "
    "Cada extractor tiene su propio manejo de errores y su propio resultado, de modo que un "
    "fallo en la base de datos no impide que el CSV y la API terminen. El orquestador conserva "
    "el error en el resultado y el proceso continúa.",
)
agregar_parrafo(
    documento,
    "La API REST se construyó como un servicio independiente porque el proyecto necesita una "
    "fuente de comentarios sociales que se comporte como un servicio externo. De esta forma el "
    "ApiExtractor usa HttpClient y no una lectura directa del archivo, que es lo que exige la "
    "actividad. Swagger permite probar los endpoints sin herramientas adicionales.",
)
agregar_parrafo(
    documento,
    "Los archivos JSON temporales se eligieron porque la actividad se concentra en la "
    "extracción. La etapa de carga puede leerlos y aplicar las transformaciones y validaciones "
    "del data warehouse. El manifiesto deja trazabilidad de los totales y los errores de cada "
    "fuente.",
)

# 7. Resultados
agregar_titulo(documento, "7. Resultados de la ejecución")
agregar_parrafo(
    documento,
    "La ejecución se realizó con el Worker Service en .NET 8, la base OpinionesWebDB en "
    "SQL Server y la API local en el puerto 5080. Las tres fuentes respondieron correctamente.",
)
tabla_encabezado(
    documento,
    ("Fuente", "Tecnología", "Registros", "Errores", "Tiempo"),
    (
        ("Encuestas internas", "CsvHelper", "500", "0", "37 ms"),
        ("Reseñas web", "ADO.NET", "200", "0", "301 ms"),
        ("Comentarios sociales", "HttpClient", "200", "0", "18 ms"),
        ("Total", "", "900", "0", "321 ms"),
    ),
    (1.7, 1.4, 1.0, 0.8, 0.9),
)
agregar_parrafo(
    documento,
    "Los archivos generados quedaron en data/extracted. El manifiesto reporta 900 registros, "
    "0 errores y el detalle de las tres fuentes.",
)
agregar_codigo(
    documento,
    """Fuente                                   Registros   Errores   Duracion ms
Encuestas internas (CSV)                       500         0            37
Resenas web (base de datos)                    200         0           301
Comentarios sociales (API REST)                200         0            18
TOTAL                                          900         0           321""",
)
agregar_parrafo(
    documento,
    "La API respondió con 200 comentarios en /api/comentarios y el endpoint /health confirmó "
    "el estado del servicio. La base de datos devolvió 200 reseñas y el CSV entregó 500 "
    "encuestas. La salida temporal está lista para la siguiente etapa del pipeline.",
)

# 8. Conclusiones
agregar_titulo(documento, "8. Conclusiones")
agregar_parrafo(
    documento,
    "La actividad permitió implementar la etapa de extracción con tres fuentes reales y una "
    "arquitectura en capas. El Worker Service en .NET 8 ejecuta los extractores en paralelo, "
    "guarda los datos en archivos temporales y registra la ejecución.",
)
agregar_parrafo(
    documento,
    "La API REST propia completa la fuente de comentarios sociales y permite que el "
    "ApiExtractor use HttpClient. El resultado es una base técnica clara para continuar con "
    "la transformación, la carga al data warehouse y el dashboard del proyecto.",
)

documento.save(DOCX)
print(f"Documento generado: {DOCX}")
