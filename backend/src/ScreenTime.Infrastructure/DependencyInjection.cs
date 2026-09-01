using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScreenTime.Application.Contracts.Authentication;
using ScreenTime.Infrastructure.Authentication;
using ScreenTime.Infrastructure.Data;

namespace ScreenTime.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing from configuration.");

        services.AddDbContext<ScreenTimeDbContext>(options => options.UseSqlite(connectionString));

        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Register JWT token service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Note: Identity services are registered in the WebApi project (Program.cs)
        // because AddIdentity is an ASP.NET Core-specific extension method

        return services;
    }
}
