using System.Net;
using System.Text.Json;
using Empreendimentos.Domain.Exceptions;

namespace Empreendimentos.Api.Middlewares;

/// <summary>
/// Middleware central de tratamento de erros. Evita poluir os controllers com
/// try/catch repetitivo e garante um formato de resposta de erro consistente
/// em toda a API, incluindo mensagens claras para o frontend exibir.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, titulo) = exception switch
        {
            DomainException => (HttpStatusCode.BadRequest, "Requisição inválida"),
            NotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            ConflictException => (HttpStatusCode.Conflict, "Conflito de dados"),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Erro não tratado ao processar {Path}", context.Request.Path);
        else
            _logger.LogWarning("{Tipo}: {Mensagem}", exception.GetType().Name, exception.Message);

        var resposta = new
        {
            status = (int)statusCode,
            titulo,
            mensagem = statusCode == HttpStatusCode.InternalServerError
                ? "Ocorreu um erro inesperado. Tente novamente mais tarde."
                : exception.Message,
            timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(resposta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
