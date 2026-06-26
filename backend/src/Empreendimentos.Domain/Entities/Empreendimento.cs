using Empreendimentos.Domain.Exceptions;

namespace Empreendimentos.Domain.Entities;

public enum StatusEmpreendimento
{
    Ativo = 1,
    Inativo = 2
}

/// <summary>
/// Entidade que representa um empreendimento.
/// As regras de negócio centrais (invariantes) ficam encapsuladas aqui,
/// para garantir que o objeto nunca exista em um estado inválido,
/// independente de quem o está manipulando (API, serviço, teste, etc).
/// </summary>
public class Empreendimento
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cnpj { get; private set; } = string.Empty;
    public string? Endereco { get; private set; }
    public StatusEmpreendimento Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    // Construtor privado: EF Core usa via reflection.
    private Empreendimento() { }

    public Empreendimento(string nome, string cnpj, string? endereco)
    {
        ValidarNome(nome);
        ValidarCnpj(cnpj);

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Cnpj = NormalizarCnpj(cnpj);
        Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
        Status = StatusEmpreendimento.Ativo; // Regra: todo empreendimento nasce Ativo.
        DataCriacao = DateTime.UtcNow;
        DataAtualizacao = null;
    }

    public void Atualizar(string nome, string cnpj, string? endereco)
    {
        // Regra: empreendimentos inativos não podem ser editados.
        if (Status == StatusEmpreendimento.Inativo)
            throw new DomainException("Não é possível editar um empreendimento inativo.");

        ValidarNome(nome);
        ValidarCnpj(cnpj);

        Nome = nome.Trim();
        Cnpj = NormalizarCnpj(cnpj);
        Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Inativar()
    {
        if (Status == StatusEmpreendimento.Inativo)
            throw new DomainException("Empreendimento já está inativo.");

        Status = StatusEmpreendimento.Inativo;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        if (Status == StatusEmpreendimento.Ativo)
            throw new DomainException("Empreendimento já está ativo.");

        Status = StatusEmpreendimento.Ativo;
        DataAtualizacao = DateTime.UtcNow;
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            throw new DomainException("O nome deve possuir pelo menos 3 caracteres.");
    }

    private static void ValidarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            throw new DomainException("O CNPJ é obrigatório.");

        var digitos = NormalizarCnpj(cnpj);

        if (digitos.Length != 14)
            throw new DomainException("O CNPJ deve conter 14 dígitos.");

        if (!CnpjValido(digitos))
            throw new DomainException("O CNPJ informado é inválido.");
    }

    /// <summary>
    /// Remove qualquer formatação (pontos, barra, hífen), deixando só os dígitos.
    /// Guardamos o CNPJ normalizado no banco para que a verificação de unicidade
    /// não dependa de como o usuário formatou o campo.
    /// </summary>
    private static string NormalizarCnpj(string cnpj) =>
        new string(cnpj.Where(char.IsDigit).ToArray());

    /// <summary>
    /// Validação do dígito verificador do CNPJ (algoritmo módulo 11 oficial).
    /// </summary>
    private static bool CnpjValido(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false; // 00000000000000 etc.

        int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var numeros = cnpj.Substring(0, 12);
        int soma = 0;
        for (int i = 0; i < 12; i++)
            soma += (numeros[i] - '0') * mult1[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        numeros += digito1;
        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += (numeros[i] - '0') * mult2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cnpj.EndsWith($"{digito1}{digito2}");
    }
}
