namespace Test.Service;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Models;
using MinimalApi.Infraestrutura.Db;

using MinimalApi.Dominio.Service;

[TestClass]
public class AdmistradorService
{
    private DbContexto CriarContexto()
    {
        var assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath!, @"..\..\..\"));
    
        var builder = new ConfigurationBuilder()
            .SetBasePath(path!)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContexto(configuration);
    }
    [TestMethod]
    public void SalvarAdm()
    {
        // Arrange
        var contexto = CriarContexto();
        contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores;");
        var adm = new Administrador();

        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "SenhaSegura123!";
        adm.Perfil = "adm";

        // Act
        var service = new AdministradorService(contexto);

        // Assert
        service.Criar(adm);
        Assert.AreEqual(1, service.ObterTodos(1).Count());
    }

    [TestMethod]
    public void ObterPorId()
    {
        // Arrange
        var contexto = CriarContexto();
        var service = new AdministradorService(contexto);

        // Act
        var administrador = service.ObterPorId(1);

        // Assert
        Assert.IsNotNull(administrador);
        Assert.AreEqual(1, administrador!.Id);
    }
}
