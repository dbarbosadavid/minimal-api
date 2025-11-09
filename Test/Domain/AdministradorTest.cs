using MinimalApi.Dominio.Models;

namespace Test.Domain;


[TestClass]
public class AdministradorTest
{
    [TestMethod]
    public void Criar()
    {
        var adm = new Administrador();
        // Arrange
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "SenhaSegura123!";
        adm.Perfil = "adm";

        // Act & Assert
        Assert.IsNotNull(adm);
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("SenhaSegura123!", adm.Senha);
        Assert.AreEqual("adm", adm.Perfil);
    }

}
