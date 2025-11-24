using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();

builder.Services.AddCors(options =>
    options.AddPolicy("Acesso Total",
        configs => configs
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod())
);
var app = builder.Build();

app.MapGet("/", () => "API de chamados");


//GET: http://localhost:5273/api/chamado/listar
app.MapGet("/api/chamado/listar", ([FromServices] AppDataContext ctx) =>
{
    var all = ctx.Chamados.ToList();
    if (all.Any()) return Results.Ok(all);
    return Results.NotFound("Nenhum chamado encontrado");
});


//POST: http://localhost:5273/api/chamado/cadastrar
app.MapPost("/api/chamado/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Chamado? chamado) =>
{
    if (chamado is null) return Results.BadRequest("Chamado inválido");
    if (string.IsNullOrEmpty(chamado.ChamadoId)) chamado.ChamadoId = Guid.NewGuid().ToString();
    if (chamado.CriadoEm == default) chamado.CriadoEm = DateTime.Now;
    if (string.IsNullOrEmpty(chamado.Status)) chamado.Status = "Aberto";

    try
    {
        ctx.Chamados.Add(chamado);
        ctx.SaveChanges();
    }
    catch (Exception)
    {
        return Results.Problem("Ocorreu um erro ao salvar o chamado.");
    }

    return Results.Created($"/api/chamado/{chamado.ChamadoId}", chamado);
});



//PATCH: http://localhost:5273/api/chamado/alterar
app.MapPatch("/api/chamado/alterar/{id}", ([FromRoute] string id, [FromServices] AppDataContext ctx) =>
{
    if (string.IsNullOrEmpty(id)) return Results.BadRequest("Id inválido");

    var existing = ctx.Chamados.FirstOrDefault(c => c.ChamadoId == id);
    if (existing is null) return Results.NotFound("Chamado não encontrado");

    if (existing.Status == "Aberto")
    {
        existing.Status = "Em atendimento";
        ctx.SaveChanges();
        return Results.Ok(existing);
    }

    if (existing.Status == "Em atendimento")
    {
        existing.Status = "Resolvido";
        ctx.SaveChanges();
        return Results.Ok(existing);
    }

    // Se status for outro (ex.: "Resolvido"), não modifica (retorna o recurso)
    return Results.Ok(existing);
});


//GET: http://localhost:5273/chamado/naoconcluidas
app.MapGet("/api/chamado/naoresolvidos", ([FromServices] AppDataContext ctx) =>
{
    var lista = ctx.Chamados.Where(c => c.Status != "Resolvido").ToList();
    if (lista.Any()) return Results.Ok(lista);
    return Results.NotFound("Nenhum chamado não resolvido encontrado");
});

//GET: http://localhost:5273/chamado/concluidas
app.MapGet("/api/chamado/resolvidos", ([FromServices] AppDataContext ctx) =>
{
    var lista = ctx.Chamados.Where(c => c.Status == "Resolvido").ToList();
    if (lista.Any()) return Results.Ok(lista);
    return Results.NotFound("Nenhum chamado resolvido encontrado");
});

app.UseCors("Acesso Total");
app.Run();
