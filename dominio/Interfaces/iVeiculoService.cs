using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Models;

namespace MinimalApi.Dominio.Interfaces;   
public interface IVeiculoService
{
   public List<Veiculo> Todos(int? pagina = 1, string? marca = null, string? modelo = null);

   public Veiculo ObterPorId(int id);

   public void Adicionar(Veiculo veiculo);

   public void Atualizar(Veiculo veiculo);

   public void Deletar(Veiculo veiculo);
}