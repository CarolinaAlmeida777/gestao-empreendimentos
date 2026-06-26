using Empreendimentos.Domain.Entities;
using Empreendimentos.Domain.Exceptions;
using Xunit;

namespace Empreendimentos.Application.Tests;

public class EmpreendimentoTests
{
    private const string CnpjValido = "11.222.333/0001-81"; // CNPJ matematicamente válido para testes

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarComStatusAtivo()
    {
        var empreendimento = new Empreendimento("Residencial Jardins", CnpjValido, "Rua A, 123");

        Assert.Equal(StatusEmpreendimento.Ativo, empreendimento.Status);
        Assert.Equal("Residencial Jardins", empreendimento.Nome);
        Assert.Equal("11222333000181", empreendimento.Cnpj);
        Assert.NotEqual(Guid.Empty, empreendimento.Id);
        Assert.True((DateTime.UtcNow - empreendimento.DataCriacao) < TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("  ")]
    public void Criar_ComNomeMenorQue3Caracteres_DeveLancarDomainException(string nomeInvalido)
    {
        Assert.Throws<DomainException>(() => new Empreendimento(nomeInvalido, CnpjValido, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("11.222.333/0001-00")] // dígito verificador errado
    [InlineData("00000000000000")]
    public void Criar_ComCnpjInvalido_DeveLancarDomainException(string cnpjInvalido)
    {
        Assert.Throws<DomainException>(() => new Empreendimento("Nome Válido", cnpjInvalido, null));
    }

    [Fact]
    public void Criar_SemEndereco_DeveAceitarEnderecoNulo()
    {
        var empreendimento = new Empreendimento("Nome Válido", CnpjValido, null);
        Assert.Null(empreendimento.Endereco);
    }

    [Fact]
    public void Atualizar_ComEmpreendimentoAtivo_DeveAtualizarDados()
    {
        var empreendimento = new Empreendimento("Nome Original", CnpjValido, "Endereço Original");

        empreendimento.Atualizar("Nome Novo", CnpjValido, "Endereço Novo");

        Assert.Equal("Nome Novo", empreendimento.Nome);
        Assert.Equal("Endereço Novo", empreendimento.Endereco);
        Assert.NotNull(empreendimento.DataAtualizacao);
    }

    [Fact]
    public void Atualizar_ComEmpreendimentoInativo_DeveLancarDomainException()
    {
        var empreendimento = new Empreendimento("Nome", CnpjValido, null);
        empreendimento.Inativar();

        var ex = Assert.Throws<DomainException>(
            () => empreendimento.Atualizar("Novo Nome", CnpjValido, null));

        Assert.Contains("inativo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Inativar_ComEmpreendimentoAtivo_DeveMudarStatusParaInativo()
    {
        var empreendimento = new Empreendimento("Nome", CnpjValido, null);

        empreendimento.Inativar();

        Assert.Equal(StatusEmpreendimento.Inativo, empreendimento.Status);
        Assert.NotNull(empreendimento.DataAtualizacao);
    }

    [Fact]
    public void Inativar_ComEmpreendimentoJaInativo_DeveLancarDomainException()
    {
        var empreendimento = new Empreendimento("Nome", CnpjValido, null);
        empreendimento.Inativar();

        Assert.Throws<DomainException>(() => empreendimento.Inativar());
    }
}
