using Microsoft.Extensions.DependencyInjection;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Infrasturcture.Services;

namespace OnlineBankAppServer.Infrasturcture;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrasturcture(this IServiceCollection services)
    {
        services.AddHttpClient<IExchangeService, ExchangeService>();

        services.AddHttpClient<IVakifbankService, VakifbankService>();

        return services;
    }
}