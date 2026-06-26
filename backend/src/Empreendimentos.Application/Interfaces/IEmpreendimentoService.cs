using Empreendimentos.Application.DTOs;

namespace Empreendimentos.Application.Interfaces;

public interface IEmpreendimentoService
{
    Task<EmpreendimentoDto> CriarAsync(CriarEmpreendimentoDto dto, CancellationToken ct = default);
    Task<EmpreendimentoDto> AtualizarAsync(Guid id, AtualizarEmpreendimentoDto dto, CancellationToken ct = default);
    Task<EmpreendimentoDto> InativarAsync(Guid id, CancellationToken ct = default);
    Task<EmpreendimentoDto> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ResultadoPaginado<EmpreendimentoDto>> ListarAsync(FiltroEmpreendimentoDto filtro, CancellationToken ct = default);
}
