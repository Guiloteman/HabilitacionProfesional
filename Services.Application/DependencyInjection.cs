using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Services.Application.Common.Behaviors;
using System.Reflection;

namespace Services.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Forma moderna y nativa de registrar AutoMapper en versiones recientes:
        services.AddAutoMapper(cfg => {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}