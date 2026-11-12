using Microsoft.OpenApi.Models;
using Opiniones.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["Api:Url"] ?? "http://localhost:5080");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Opiniones API",
        Version = "v1",
        Description = "Comentarios de redes sociales del Sistema de Analisis de Opiniones de Clientes."
    });
});

builder.Services.AddSingleton<IComentarioRepositorio, CsvComentarioRepositorio>();
builder.Services.AddCors(opciones => opciones.AddDefaultPolicy(politica =>
    politica.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Opiniones API v1");
});

app.UseCors();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/health", (IComentarioRepositorio repositorio) => Results.Ok(new
{
    estado = "ok",
    servicio = "Opiniones API",
    totalComentarios = repositorio.Total
}));

app.MapGet("/api/comentarios", (IComentarioRepositorio repositorio, string? plataforma) =>
{
    var datos = repositorio.ObtenerTodos();

    if (!string.IsNullOrWhiteSpace(plataforma))
    {
        datos = datos
            .Where(c => string.Equals(c.Plataforma, plataforma, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    return Results.Ok(datos);
});

app.MapGet("/api/comentarios/{id}", (string id, IComentarioRepositorio repositorio) =>
{
    var comentario = repositorio.ObtenerPorId(id);
    return comentario is null
        ? Results.NotFound(new { mensaje = $"No existe el comentario {id}." })
        : Results.Ok(comentario);
});

app.Run();
