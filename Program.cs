var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "123456")
    {
        return Results.Ok(new { Token = "fake-jwt-token" });
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.Run();

public class LoginDTO
{
    public string Email { get; set; }
    public string Senha { get; set; }
}