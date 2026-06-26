using Empreendimentos.Application.DTOs;
using Empreendimentos.Domain.Entities;

namespace Empreendimentos.Application.Interfaces;

/// <summary>
/// Porta de saída (Clean Architecture): a camada Application define o contrato,
/// e a Infrastructure o implementa com EF Core + MySQL. Isso permite trocar o
/// banco/ORM sem alterar regra de negócio nenhuma.
/// </summary>
public interface IEmpreendimentoRepository
{
    Task<Empreendimento?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteCnpjAsync(string cnpjNormalizado, Guid? idParaIgnorar = null, CancellationToken ct = default);
    Task<ResultadoPaginado<Empreendimento>> ListarAsync(FiltroEmpreendimentoDto filtro, CancellationToken ct = default);
    Task AdicionarAsync(Empreendimento empreendimento, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
