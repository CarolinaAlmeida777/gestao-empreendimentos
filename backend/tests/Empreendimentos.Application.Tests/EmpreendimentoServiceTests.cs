using Empreendimentos.Application.DTOs;
using Empreendimentos.Application.Interfaces;
using Empreendimentos.Application.Services;
using Empreendimentos.Domain.Entities;
using Empreendimentos.Domain.Exceptions;
using Moq;
using Xunit;

namespace Empreendimentos.Application.Tests;

public class EmpreendimentoServiceTests
{
    private const string CnpjValido = "11.222.333/0001-81";
    private readonly Mock<IEmpreendimentoRepository> _repositoryMock;
    private readonly EmpreendimentoService _service;

    public EmpreendimentoServiceTests()
    {
        _repositoryMock = new Mock<IEmpreendimentoRepository>();
        _service = new EmpreendimentoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CriarAsync_ComCnpjJaExistente_DeveLancarConflictException()
    {
        _repositoryMock
            .Setup(r => r.ExisteCnpjAsync(It.IsAny<string>(), null, default))
            .ReturnsAsync(true);

        var dto = new CriarEmpreendimentoDto("Empreendimento Teste", CnpjValido, null);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CriarAsync(dto));

        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Empreendimento>(), default), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ComCnpjUnico_DeveAdicionarESalvar()
    {
        _repositoryMock
            .Setup(r => r.ExisteCnpjAsync(It.IsAny<string>(), null, default))
            .ReturnsAsync(false);

        var dto = new CriarEmpreendimentoDto("Empreendimento Teste", CnpjValido, "Endereço X");

        var resultado = await _service.CriarAsync(dto);

        Assert.Equal("Empreendimento Teste", resultado.Nome);
        Assert.Equal("Ativo", resultado.Status);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Empreendimento>(), default), Times.Once);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ComIdInexistente_DeveLancarNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Empreendimento?)null);

        var dto = new AtualizarEmpreendimentoDto("Nome Novo", CnpjValido, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarAsync(Guid.NewGuid(), dto));
    }

    [Fact]
    public async Task AtualizarAsync_ComEmpreendimentoInativo_DeveLancarDomainException()
    {
        var empreendimento = new Empreendimento("Nome", CnpjValido, null);
        empreendimento.Inativar();

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(empreendimento.Id, default))
            .ReturnsAsync(empreendimento);
        _repositoryMock
            .Setup(r => r.ExisteCnpjAsync(It.IsAny<string>(), empreendimento.Id, default))
            .ReturnsAsync(false);

        var dto = new AtualizarEmpreendimentoDto("Nome Novo", CnpjValido, null);

        await Assert.ThrowsAsync<DomainException>(() => _service.AtualizarAsync(empreendimento.Id, dto));
    }

    [Fact]
    public async Task InativarAsync_ComIdExistente_DeveInativarESalvar()
    {
        var empreendimento = new Empreendimento("Nome", CnpjValido, null);

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(empreendimento.Id, default))
            .ReturnsAsync(empreendimento);

        var resultado = await _service.InativarAsync(empreendimento.Id);

        Assert.Equal("Inativo", resultado.Status);
        _repositoryMock.Verify(r => r.SalvarAlteracoesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarResultadoMapeado()
    {
        var empreendimento = new Empreendimento("Nome", CnpjValido, null);

        _repositoryMock
            .Setup(r => r.ListarAsync(It.IsAny<FiltroEmpreendimentoDto>(), default))
            .ReturnsAsync(new ResultadoPaginado<Empreendimento>
            {
                Itens = new List<Empreendimento> { empreendimento },
                Pagina = 1,
                TamanhoPagina = 10,
                TotalItens = 1
            });

        var resultado = await _service.ListarAsync(new FiltroEmpreendimentoDto());

        Assert.Single(resultado.Itens);
        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal("Nome", resultado.Itens[0].Nome);
    }
}
