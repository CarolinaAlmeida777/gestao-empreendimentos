using Empreendimentos.Application.DTOs;
using Empreendimentos.Application.Interfaces;
using Empreendimentos.Domain.Entities;
using Empreendimentos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Empreendimentos.Infrastructure.Repositories;

public class EmpreendimentoRepository : IEmpreendimentoRepository
{
    private readonly AppDbContext _context;

    public EmpreendimentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Empreendimento?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Empreendimentos.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExisteCnpjAsync(string cnpjNormalizado, Guid? idParaIgnorar = null, CancellationToken ct = default)
    {
        var query = _context.Empreendimentos.Where(e => e.Cnpj == cnpjNormalizado);

        if (idParaIgnorar.HasValue)
            query = query.Where(e => e.Id != idParaIgnorar.Value);

        return query.AnyAsync(ct);
    }

    public async Task<ResultadoPaginado<Empreendimento>> ListarAsync(FiltroEmpreendimentoDto filtro, CancellationToken ct = default)
    {
        var query = _context.Empreendimentos.AsQueryable();

        // --- Filtros ---
        if (!string.IsNullOrWhiteSpace(filtro.Nome))
            query = query.Where(e => e.Nome.Contains(filtro.Nome));

        if (filtro.Status.HasValue)
            query = query.Where(e => e.Status == filtro.Status.Value);

        var totalItens = await query.CountAsync(ct);

        // --- Ordenação ---
        query = filtro.OrdenarPor?.ToLowerInvariant() switch
        {
            "datacriacao" => filtro.Decrescente
                ? query.OrderByDescending(e => e.DataCriacao)
                : query.OrderBy(e => e.DataCriacao),
            _ => filtro.Decrescente
                ? query.OrderByDescending(e => e.Nome)
                : query.OrderBy(e => e.Nome),
        };

        // --- Paginação ---
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamanhoPagina = filtro.TamanhoPagina is < 1 or > 100 ? 10 : filtro.TamanhoPagina;

        var itens = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return new ResultadoPaginado<Empreendimento>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens
        };
    }

    public async Task AdicionarAsync(Empreendimento empreendimento, CancellationToken ct = default) =>
        await _context.Empreendimentos.AddAsync(empreendimento, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
