using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Behaviors;
using OnlineBankAppServer.Application.Services;

namespace OnlineBankAppServer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IJwtProvider, JwtProvider>();


        return services;
    }
}