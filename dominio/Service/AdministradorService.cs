using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Models;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Service;   
public class AdministradorService : IAdministradorService
{
    public readonly DbContexto _contexto;
    public AdministradorService(DbContexto contexto)
    {
        _contexto = contexto;
    }
    public Administrador Login(LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();

        var resultado = adm;
        return resultado!;
    }

    public Administrador Criar(Administrador administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();
        return administrador;
    }

    public List<Administrador> ObterTodos(int? pagina)
    {
        int tamanhoPagina = 10;
        int numeroPagina = pagina ?? 1;

        var administradores = _contexto.Administradores
            .Skip((numeroPagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        return administradores;
    }
    
    public Administrador? ObterPorId(int id)
    {
        return _contexto.Administradores.Find(id);
    }
}