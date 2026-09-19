using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Application.Common.Interfaces;
using Services.Infrastructure.Persistence.Scaffolded;

namespace Services.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Registrar tu DbContext scaffolded
        services.AddDbContext<ServicesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // 2. Mapear la interfaz de Aplicación hacia la infraestructura concreta
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ServicesDbContext>());

        return services;
    }
}