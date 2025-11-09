namespace MinimalApi.Dominio.Views;

public struct ErrosDeValidacao
{
    
    public List<string> Erros { get; set; }

    public ErrosDeValidacao()
    {
        Erros = new List<string>();
    }
}
