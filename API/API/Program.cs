using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();

var app = builder.Build();

app.MapGet("/", () => "API de chamados");

//ENDPOINTS DE TAREFA
//GET: http://localhost:5273/api/chamado/listar
app.MapGet("/api/chamado/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Chamados.Any())
    {
        return Results.Ok(ctx.Chamados.ToList());
    }
    return Results.NotFound("Nenhum chamado encontrada");
});

//POST: http://localhost:5273/api/chamado/cadastrar
app.MapPost("/api/chamado/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Chamado chamado) =>
{
    ctx.Chamados.Add(chamado);
    ctx.SaveChanges();
    return Results.Created("", chamado);
});

//PATCH: http://localhost:5273/api/chamado/alterar
app.MapMethods("/api/chamado/alterar", new[] { "PATCH" }, ([FromServices] AppDataContext ctx, [FromBody] Chamado chamado) =>
{
    if (chamado is null || string.IsNullOrEmpty(chamado.ChamadoId))
    {
        return Results.NotFound();
    }

    // Find the existing chamado
    var existing = ctx.Chamados.FirstOrDefault(c => c.ChamadoId == chamado.ChamadoId);
    if (existing is null) return Results.NotFound();

    // Only change the Status field according to the requested flow:
    // "Aberto" -> "Em atendimento"
    // "Em atendimento" -> "Resolvido"
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

    // If none of the two transitions apply, return the resource unchanged.
    return Results.Ok(existing);
});


//GET: http://localhost:5273/chamado/naoconcluidas
app.MapGet("/api/chamado/naoresolvidos", ([FromServices] AppDataContext ctx) =>
{
    // Listar todos os chamados cujo status não é "Resolvido"
    var lista = ctx.Chamados.Where(c => c.Status != "Resolvido").ToList();
    if (lista.Any()) return Results.Ok(lista);
    return Results.NotFound("Nenhum chamado não resolvido encontrado");
});

//GET: http://localhost:5273/chamado/concluidas
app.MapGet("/api/chamado/resolvidos", ([FromServices] AppDataContext ctx) =>
{
    // Listar todos os chamados cujo status é "Resolvido"
    var lista = ctx.Chamados.Where(c => c.Status == "Resolvido").ToList();
    if (lista.Any()) return Results.Ok(lista);
    return Results.NotFound("Nenhum chamado resolvido encontrado");
});

app.Run();
