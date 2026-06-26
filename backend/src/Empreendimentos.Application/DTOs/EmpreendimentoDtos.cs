using Empreendimentos.Domain.Entities;

namespace Empreendimentos.Application.DTOs;

/// <summary>DTO usado para criar um novo empreendimento.</summary>
public record CriarEmpreendimentoDto(string Nome, string Cnpj, string? Endereco);

/// <summary>DTO usado para atualizar um empreendimento existente.</summary>
public record AtualizarEmpreendimentoDto(string Nome, string Cnpj, string? Endereco);

/// <summary>DTO de saída, representando o empreendimento retornado pela API.</summary>
public record EmpreendimentoDto(
    Guid Id,
    string Nome,
    string Cnpj,
    string? Endereco,
    string Status,
    DateTime DataCriacao,
    DateTime? DataAtualizacao)
{
    public static EmpreendimentoDto DeEntidade(Empreendimento e) => new(
        e.Id,
        e.Nome,
        FormatarCnpj(e.Cnpj),
        e.Endereco,
        e.Status.ToString(),
        e.DataCriacao,
        e.DataAtualizacao);

    private static string FormatarCnpj(string cnpj)
    {
        // Formata 14 dígitos puros como XX.XXX.XXX/XXXX-XX para exibição.
        if (cnpj.Length != 14) return cnpj;
        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }
}

/// <summary>Parâmetros de filtro, ordenação e paginação para a listagem.</summary>
public class FiltroEmpreendimentoDto
{
    public string? Nome { get; set; }
    public StatusEmpreendimento? Status { get; set; }
    public string OrdenarPor { get; set; } = "nome"; // "nome" | "dataCriacao"
    public bool Decrescente { get; set; } = false;
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
}

/// <summary>Envelope de resultado paginado, genérico para reuso.</summary>
public class ResultadoPaginado<T>
{
    public List<T> Itens { get; set; } = new();
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
}
