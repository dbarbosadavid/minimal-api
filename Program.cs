using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Models;
using MinimalApi.Dominio.Service;
using MinimalApi.Dominio.Views;
using MinimalApi.Infraestrutura.Db;

#region builder
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region Home API
app.MapGet("/", () => Results.Json(new HomeAPI())).WithTags("Home");
#endregion

#region Administrador - Login
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
{
    if (administradorService.Login(loginDTO) != null)
    {
        return Results.Ok(new { Token = "fake-jwt-token" });
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administrador");

app.MapPost("/administradores/criar", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) =>
{
    var validator = new Validator();
    var erros = new ErrosDeValidacao();
    if (administradorDTO.Perfil == null)
        erros.Erros.Add("O perfil do administrador é obrigatório.");
    if (string.IsNullOrEmpty(administradorDTO.Email))
        erros.Erros.Add("O email do administrador é obrigatório.");
    if (string.IsNullOrEmpty(administradorDTO.Senha))
        erros.Erros.Add("A senha do administrador é obrigatória.");
    if (erros.Erros.Count > 0)
        return Results.BadRequest(erros);

    var administrador = new Administrador
    {
        Perfil = administradorDTO.Perfil.ToString(),
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha
    };

    administradorService.Criar(administrador);
    // Aqui você pode adicionar lógica para validar e salvar o administrador
    return Results.Ok("Administrador criado com sucesso");
}).WithTags("Administrador");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
{
    var administradores = administradorService.ObterTodos(pagina);
    return Results.Ok(administradores);
}).WithTags("Administrador");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
{
    var administrador = administradorService.ObterPorId(id);
    if (administrador == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(administrador);
}).WithTags("Administrador");
#endregion 

#region Veiculo Endpoints
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var validator = new Validator();
    ErrosDeValidacao erros = validator.validar(veiculoDTO);

    if (erros.Erros.Count > 0)  
    {
        return Results.BadRequest(erros);
    }
    

    var veiculo = new Veiculo
    {
        Marca = veiculoDTO.Marca,
        Modelo = veiculoDTO.Modelo,
        Ano = veiculoDTO.Ano
    };
    veiculoService.Adicionar(veiculo);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] string? marca, string? modelo, int? pagina, IVeiculoService veiculoService) =>
{
    pagina = pagina ?? 1;
    var veiculos = veiculoService.Todos(pagina, marca, modelo);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.ObterPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var validator = new Validator();
    ErrosDeValidacao erros = validator.validar(veiculoDTO);

    if (erros.Erros.Count > 0)  
    {
        return Results.BadRequest(erros);
    }
    var veiculo = veiculoService.ObterPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Modelo = veiculoDTO.Modelo;
    veiculo.Ano = veiculoDTO.Ano;
    veiculoService.Atualizar(veiculo);
    return Results.Ok(veiculo);

}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.ObterPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    veiculoService.Deletar(veiculo);
    return Results.Ok("Veículo deletado com sucesso");
}).WithTags("Veiculos");
#endregion

#region APP
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
#endregion
