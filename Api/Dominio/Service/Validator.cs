using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Views;

namespace MinimalApi.Dominio.Service;
public class Validator
{
    public ErrosDeValidacao validar(VeiculoDTO veiculoDTO)
    {

        ErrosDeValidacao validacao = new ErrosDeValidacao();

        if (string.IsNullOrEmpty(veiculoDTO.Marca))
            validacao.Erros.Add("A marca do veículo é obrigatória.");
        

        if (string.IsNullOrEmpty(veiculoDTO.Modelo))
            validacao.Erros.Add("O modelo do veículo é obrigatório.");

        if (veiculoDTO.Ano < 1886 || veiculoDTO.Ano > DateTime.Now.Year + 1)
            validacao.Erros.Add("O ano do veículo é inválido.");

        return validacao;

    }
}