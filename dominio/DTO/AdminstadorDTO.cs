using MinimalApi.Dominio.Enums;

namespace MinimalApi.Dominio.DTO;

public struct AdministradorDTO
{
    public Perfil? Perfil { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
}