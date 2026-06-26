using Empreendimentos.Application.DTOs;
using Empreendimentos.Application.Interfaces;
using Empreendimentos.Domain.Entities;
using Empreendimentos.Domain.Exceptions;

namespace Empreendimentos.Application.Services;

/// <summary>
/// Orquestra os casos de uso de Empreendimento. Regras de invariante (ex.: nome
/// mínimo, formato de CNPJ, transições de status) vivem na entidade de Domain;
/// aqui ficam regras que dependem de infraestrutura, como a unicidade de CNPJ
/// (que exige consultar o banco).
/// </summary>
public class EmpreendimentoService : IEmpreendimentoService
{
    private readonly IEmpreendimentoRepository _repository;

    public EmpreendimentoService(IEmpreendimentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmpreendimentoDto> CriarAsync(CriarEmpreendimentoDto dto, CancellationToken ct = default)
    {
        var empreendimento = new Empreendimento(dto.Nome, dto.Cnpj, dto.Endereco);

        // Regra: não pode existir mais de um empreendimento com o mesmo CNPJ.
        if (await _repository.ExisteCnpjAsync(empreendimento.Cnpj, ct: ct))
            throw new ConflictException($"Já existe um empreendimento cadastrado com o CNPJ {dto.Cnpj}.");

        await _repository.AdicionarAsync(empreendimento, ct);
        await _repository.SalvarAlteracoesAsync(ct);

        return EmpreendimentoDto.DeEntidade(empreendimento);
    }

    public async Task<EmpreendimentoDto> AtualizarAsync(Guid id, AtualizarEmpreendimentoDto dto, CancellationToken ct = default)
    {
        var empreendimento = await _repository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Empreendimento com Id '{id}' não foi encontrado.");

        // Verifica unicidade de CNPJ, ignorando o próprio registro sendo editado.
        var cnpjNormalizado = new string(dto.Cnpj.Where(char.IsDigit).ToArray());
        if (await _repository.ExisteCnpjAsync(cnpjNormalizado, id, ct))
            throw new ConflictException($"Já existe outro empreendimento cadastrado com o CNPJ {dto.Cnpj}.");

        empreendimento.Atualizar(dto.Nome, dto.Cnpj, dto.Endereco); // lança DomainException se inativo/inválido

        await _repository.SalvarAlteracoesAsync(ct);

        return EmpreendimentoDto.DeEntidade(empreendimento);
    }

    public async Task<EmpreendimentoDto> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var empreendimento = await _repository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Empreendimento com Id '{id}' não foi encontrado.");

        empreendimento.Inativar();

        await _repository.SalvarAlteracoesAsync(ct);

        return EmpreendimentoDto.DeEntidade(empreendimento);
    }

    public async Task<EmpreendimentoDto> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var empreendimento = await _repository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Empreendimento com Id '{id}' não foi encontrado.");

        return EmpreendimentoDto.DeEntidade(empreendimento);
    }

    public async Task<ResultadoPaginado<EmpreendimentoDto>> ListarAsync(FiltroEmpreendimentoDto filtro, CancellationToken ct = default)
    {
        var resultado = await _repository.ListarAsync(filtro, ct);

        return new ResultadoPaginado<EmpreendimentoDto>
        {
            Itens = resultado.Itens.Select(EmpreendimentoDto.DeEntidade).ToList(),
            Pagina = resultado.Pagina,
            TamanhoPagina = resultado.TamanhoPagina,
            TotalItens = resultado.TotalItens
        };
    }
}
