using Empreendimentos.Application.Interfaces;
using Empreendimentos.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Empreendimentos.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmpreendimentoService, EmpreendimentoService>();
        return services;
    }
}
