using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Models;

namespace MinimalApi.Dominio.Interfaces;   
public interface IAdministradorService
{
   public Administrador? Login(LoginDTO loginDTO);

   public Administrador Criar(Administrador administrador);

   public List<Administrador> ObterTodos(int? pagina);
   
   public Administrador? ObterPorId(int id);
}