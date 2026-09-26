using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Waybon.Application.Auth.Abstractions;
using Waybon.Application.Common.Abstractions;
using Waybon.Application.Emails.Abstractions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Users.Abstractions;
using Waybon.Infrastructure.Auth;
using Waybon.Infrastructure.Common;
using Waybon.Infrastructure.Emails;
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
        services.AddDbContextPool<AppDbContext>
        (
            options => options
                .UseNpgsql
                (
                    connectionString,
                    npgsql => npgsql.EnableRetryOnFailure
                    (
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null
                    )
                )
                .UseSnakeCaseNamingConvention()
        );


        // ===================================
        // Services
        // ===================================

        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenGenerator, SecureTokenGenerator>();


        // ===================================
        // Email
        // ===================================

        services.AddOptions<BrevoOptions>()
            .Bind(configuration.GetSection(BrevoOptions.SectionName))
            .Validate
            (
                brevo => !string.IsNullOrWhiteSpace(brevo.ApiKey)
                    && !string.IsNullOrWhiteSpace(brevo.SenderEmail)
                    && !string.IsNullOrWhiteSpace(brevo.SenderName),
                "The 'Brevo' settings (ApiKey, SenderEmail, SenderName) are not fully configured."
            )
            .ValidateOnStart();

        services.AddHttpClient<IEmailSender, BrevoEmailSender>((serviceProvider, client) =>
        {
            var brevo = serviceProvider.GetRequiredService<IOptions<BrevoOptions>>().Value;

            client.BaseAddress = new Uri("https://api.brevo.com/v3/");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("api-key", brevo.ApiKey);
        });

        services.AddMemoryCache();
        services.AddSingleton<IEmailSendLimiter, MemoryEmailSendLimiter>();

        services.AddSingleton<EmailQueue>();
        services.AddSingleton<IEmailQueue>(serviceProvider => serviceProvider.GetRequiredService<EmailQueue>());
        services.AddHostedService<EmailQueueProcessor>();

        return services;
    }
}