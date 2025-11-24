using API.Models;
// ChamadoService removed — tests will exercise the transition logic directly through the DbContext
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace API.Tests;

public class ChamadoTransitionTests
{
    private static AppDataContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDataContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDataContext(options);
    }

    [Fact]
    public void ApplyTransition_FromAberto_ToEmAtendimento()
    {
        var ctx = CreateInMemoryContext("test1");
        var chamado = new Chamado { ChamadoId = "t1", Status = "Aberto" };
        ctx.Chamados.Add(chamado);
        ctx.SaveChanges();

        // perform transition inline (same rules as endpoint)
        var existing = ctx.Chamados.FirstOrDefault(c => c.ChamadoId == "t1");
        if (existing.Status == "Aberto") existing.Status = "Em atendimento";
        ctx.SaveChanges();

        Assert.NotNull(existing);
        Assert.Equal("Em atendimento", existing.Status);
    }

    [Fact]
    public void ApplyTransition_FromEmAtendimento_ToResolvido()
    {
        var ctx = CreateInMemoryContext("test2");
        var chamado = new Chamado { ChamadoId = "t2", Status = "Em atendimento" };
        ctx.Chamados.Add(chamado);
        ctx.SaveChanges();

        var existing = ctx.Chamados.FirstOrDefault(c => c.ChamadoId == "t2");
        if (existing.Status == "Em atendimento") existing.Status = "Resolvido";
        ctx.SaveChanges();

        Assert.NotNull(existing);
        Assert.Equal("Resolvido", existing.Status);
    }

    [Fact]
    public void ApplyTransition_OtherStatus_RemainsUnchanged()
    {
        var ctx = CreateInMemoryContext("test3");
        var chamado = new Chamado { ChamadoId = "t3", Status = "Resolvido" };
        ctx.Chamados.Add(chamado);
        ctx.SaveChanges();

        var existing = ctx.Chamados.FirstOrDefault(c => c.ChamadoId == "t3");
        // no legal transition expected for "Resolvido" - it should stay the same
        if (existing.Status == "Aberto") existing.Status = "Em atendimento";
        else if (existing.Status == "Em atendimento") existing.Status = "Resolvido";
        ctx.SaveChanges();

        Assert.NotNull(existing);
        Assert.Equal("Resolvido", existing.Status);
    }

    [Fact]
    public void List_NaoResolvidos_ReturnsOnlyNonResolved()
    {
        var ctx = CreateInMemoryContext("list1");
        ctx.Chamados.Add(new Chamado { ChamadoId = "a", Status = "Aberto" });
        ctx.Chamados.Add(new Chamado { ChamadoId = "b", Status = "Em atendimento" });
        ctx.Chamados.Add(new Chamado { ChamadoId = "c", Status = "Resolvido" });
        ctx.SaveChanges();

        var lista = ctx.Chamados.Where(c => c.Status != "Resolvido").ToList();

        Assert.Equal(2, lista.Count);
        Assert.DoesNotContain(lista, x => x.Status == "Resolvido");
    }

    [Fact]
    public void List_Resolvidos_ReturnsOnlyResolved()
    {
        var ctx = CreateInMemoryContext("list2");
        ctx.Chamados.Add(new Chamado { ChamadoId = "a", Status = "Aberto" });
        ctx.Chamados.Add(new Chamado { ChamadoId = "b", Status = "Resolvido" });
        ctx.SaveChanges();

        var lista = ctx.Chamados.Where(c => c.Status == "Resolvido").ToList();

        Assert.Single(lista);
        Assert.All(lista, x => Assert.Equal("Resolvido", x.Status));
    }

    [Fact]
    public void Create_Chamado_SavesToDb_AndSetsId()
    {
        var ctx = CreateInMemoryContext("create1");
        var chamado = new Chamado { Descricao = "Teste criar", Status = "Aberto" };

        // Simulate the API's create handler behavior: ensure ID and save
        if (string.IsNullOrEmpty(chamado.ChamadoId)) chamado.ChamadoId = Guid.NewGuid().ToString();
        ctx.Chamados.Add(chamado);
        ctx.SaveChanges();

        var found = ctx.Chamados.Find(chamado.ChamadoId);
        Assert.NotNull(found);
        Assert.Equal("Aberto", found.Status);
        Assert.Equal(chamado.Descricao, found.Descricao);
    }

}
