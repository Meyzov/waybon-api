using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waybon.Infrastructure.Persistence;

namespace Waybon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("The 'DefaultConnection' connection string is not configured.");
        services.AddDbContext<AppDbContext>
        (
            options => options.UseNpgsql(connectionString)
        );

        return services;
    }
}