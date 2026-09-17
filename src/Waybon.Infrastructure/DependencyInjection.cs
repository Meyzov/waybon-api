using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waybon.Application.Common.Abstractions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Users.Abstractions;
using Waybon.Infrastructure.Common;
using Waybon.Infrastructure.Persistence;
using Waybon.Infrastructure.Roles;
using Waybon.Infrastructure.Users;

namespace Waybon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ===================================
        // Connection string
        // ===================================

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("The 'DefaultConnection' connection string is not configured.");
        services.AddDbContext<AppDbContext>
        (
            options => options.UseNpgsql(connectionString)
        );


        // ===================================
        // Services
        // ===================================

        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
}