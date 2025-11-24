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
    if (ctx.Chamados.Any())
    {
        return Results.Ok(ctx.Chamados.ToList());
    }
    return Results.NotFound("Nenhum chamado encontrada");
});

//POST: http://localhost:5273/api/chamado/cadastrar
app.MapPost("/api/chamado/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Chamado? chamado) =>
{
    if (chamado is null) return Results.BadRequest("Chamado inválido");
    if (string.IsNullOrEmpty(chamado.ChamadoId)) chamado.ChamadoId = Guid.NewGuid().ToString();

    try
    {
        ctx.Chamados.Add(chamado);
        ctx.SaveChanges();
    }
    catch
    {
        return Results.NotFound("Ocorreu um erro ao salvar o chamado.");
    }

    return Results.Created($"/api/chamado/{chamado.ChamadoId}", chamado);
});

//PATCH: http://localhost:5273/api/chamado/alterar
app.MapPatch("/api/chamado/alterar",
 ([FromRoute] string id,
  [FromServices] AppDataContext ctx) =>
{
    if (string.IsNullOrEmpty(id)) return Results.NotFound();

    var existe = ctx.Chamados.FirstOrDefault(c => c.ChamadoId == id);
    if (existe is null) return Results.NotFound();

    if (existe.Status == "Aberto")
    {
        existe.Status = "Em atendimento";
        ctx.SaveChanges();
        return Results.Ok(existe);
    }

    if (existe.Status == "Em atendimento")
    {
        existe.Status = "Resolvido";
        ctx.SaveChanges();
        return Results.Ok(existe);
    }

    
    return Results.Ok(existe);
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
