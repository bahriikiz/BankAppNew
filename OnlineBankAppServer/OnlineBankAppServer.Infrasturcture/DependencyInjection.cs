using Microsoft.Extensions.DependencyInjection;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Infrastructure.Services;
using OnlineBankAppServer.Application.Integration.Vakifbank;

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