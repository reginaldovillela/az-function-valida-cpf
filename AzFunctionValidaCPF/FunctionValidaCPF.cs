using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzFunctionValidaCPF;

public class FunctionValidaCPF(ILogger<FunctionValidaCPF> logger)
{
    [Function("FunctionValidaCPF")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        logger.LogInformation("Iniciando a validação do CPF");

        var body = await req.ReadFromJsonAsync<RequestModelFunctionValidaCPF>();

        if (body is null)
            return new BadRequestObjectResult("Por favor, informar o CPF");

        if (!ValidaCPF(body.Cpf))
            return new BadRequestObjectResult("CPF inválido");

        return new OkObjectResult("CPF válido e limpo na RFB");
    }

    private static bool ValidaCPF(string cpf)
    {
        if (string.IsNullOrEmpty(cpf))
            return false;

        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            return false;

        var multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var tempCpf = cpf[..9];
        var soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        var resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        var digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }

    public record RequestModelFunctionValidaCPF(string Cpf);
}