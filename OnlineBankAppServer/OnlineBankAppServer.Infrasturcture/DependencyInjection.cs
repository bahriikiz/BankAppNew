using Microsoft.Extensions.DependencyInjection;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Infrastructure.Services;

namespace OnlineBankAppServer.Infrasturcture;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrasturcture(this IServiceCollection services)
    {
        services.AddHttpClient<IExchangeService, ExchangeService>();
        return services;
    }
}