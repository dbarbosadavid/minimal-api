using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Models;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Service;   
public class VeiculoService : IVeiculoService
{
   public readonly DbContexto _contexto;
   public VeiculoService(DbContexto contexto)
   {
      _contexto = contexto;
   }
   public List<Veiculo> Todos(int? pagina = 1, string? marca = null, string? modelo = null)
   {
         var query = _contexto.Veiculos.AsQueryable();
   
         if (!string.IsNullOrEmpty(marca))
         {
            query = query.Where(v => v.Marca.Contains(marca));
         }

      if (!string.IsNullOrEmpty(modelo))
      {
         query = query.Where(v => v.Modelo.Contains(modelo));
      }
   
         int pageSize = 10;
         var veiculos = query
            .Skip(((int)pagina - 1) * pageSize)
            .Take(pageSize)
            .ToList();
   
         return veiculos;
    }

   public Veiculo ObterPorId(int id)
    {
         var veiculo = _contexto.Veiculos.Find(id);
         return veiculo!;
    }

   public void Adicionar(Veiculo veiculo)
   {
      _contexto.Veiculos.Add(veiculo);
      _contexto.SaveChanges();
   }

   public void Atualizar(Veiculo veiculo)
   {
      _contexto.Veiculos.Update(veiculo);
      _contexto.SaveChanges();
   }

   public void Deletar(Veiculo veiculo)
   {
      _contexto.Veiculos.Remove(veiculo);
      _contexto.SaveChanges();

   }
}