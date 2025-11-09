using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Models;
using MinimalApi.Dominio.Service;
using MinimalApi.Dominio.Views;
using MinimalApi.Infraestrutura.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

#region builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();

if(string.IsNullOrEmpty(key))
    key = "123456";

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateAudience = false,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(key)
        ),
        RoleClaimType = ClaimTypes.Role,
    };
});
builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Minimal API - Gestão de Veículos",
        Version = "v1",
        Description = "API para gerenciamento de veículos com autenticação JWT"
    });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta forma: Bearer {seu token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion
app.MapGet("/debug/claims", (ClaimsPrincipal user) =>
{
    var claims = user.Claims.Select(c => new { c.Type, c.Value });
    return Results.Json(claims);
}).RequireAuthorization().WithTags("Debug");


#region Home API
app.MapGet("/", () => Results.Json(new HomeAPI())).AllowAnonymous().WithTags("Home");
#endregion

#region Administrador - Login
string GerarToken(Administrador administrador)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;

    var token = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
    var credenciais = new SigningCredentials(token, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, administrador.Email),
        new Claim("role", administrador.Perfil),
        new Claim("Perfil", administrador.Perfil)
    };

    var tokenDescriptor = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credenciais
    );

    return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
}


app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
{
    var adm = administradorService.Login(loginDTO);

    if (adm != null)
    {
        string token = GerarToken(adm);
        return Results.Ok(new AdmLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administrador");

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
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "adm"})
.WithTags("Administrador");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
{
    var administradores = administradorService.ObterTodos(pagina);
    return Results.Ok(administradores);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "adm"})
.WithTags("Administrador");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
{
    var administrador = administradorService.ObterPorId(id);
    if (administrador == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(administrador);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "adm"})
.WithTags("Administrador");
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
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "adm,editor"})
.WithTags("Veiculos");

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
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "adm,editor"})
.WithTags("Veiculos");

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

})
.RequireAuthorization(new AuthorizeAttribute { Roles = "adm"})
.WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.ObterPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    veiculoService.Deletar(veiculo);
    return Results.Ok("Veículo deletado com sucesso");
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "adm"})
.WithTags("Veiculos");
#endregion

#region APP
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
