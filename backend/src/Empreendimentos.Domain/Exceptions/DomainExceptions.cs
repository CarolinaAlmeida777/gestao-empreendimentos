namespace Empreendimentos.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de negócio (invariante de domínio) é violada.
/// É capturada por um middleware na API e traduzida para HTTP 400.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>
/// Exceção lançada quando uma entidade não é encontrada.
/// Traduzida pelo middleware para HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Exceção lançada quando há conflito de unicidade (ex.: CNPJ duplicado).
/// Traduzida pelo middleware para HTTP 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
