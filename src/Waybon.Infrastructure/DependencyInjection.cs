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
    // ===================================
    // Constants
    // ===================================

    private const string ConnectionStringName = "DefaultConnection";
    private const int DatabaseMaxRetryCount = 3;
    private static readonly TimeSpan DatabaseMaxRetryDelay = TimeSpan.FromSeconds(5);
    private const string MissingConnectionStringMessage = $"The '{ConnectionStringName}' connection string is not configured.";
    private const string MissingBrevoSettingsMessage = $"The '{BrevoOptions.SectionName}' settings (ApiKey, SenderEmail, SenderName) are not fully configured.";
    private const string BrevoBaseAddress = "https://api.brevo.com/v3/";
    private const string BrevoApiKeyHeader = "api-key";
    private static readonly TimeSpan BrevoTimeout = TimeSpan.FromSeconds(10);


    // ===================================
    // AddInfrastructure
    // ===================================

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ===================================
        // Connection string
        // ===================================

        var connectionString = configuration.GetConnectionString(ConnectionStringName) ?? throw new InvalidOperationException(MissingConnectionStringMessage);
        services.AddDbContextPool<AppDbContext>
        (
            options => options
                .UseNpgsql
                (
                    connectionString,
                    npgsql => npgsql.EnableRetryOnFailure
                    (
                        maxRetryCount: DatabaseMaxRetryCount,
                        maxRetryDelay: DatabaseMaxRetryDelay,
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
                brevo => !string.IsNullOrWhiteSpace(brevo.ApiKey) && !string.IsNullOrWhiteSpace(brevo.SenderEmail) && !string.IsNullOrWhiteSpace(brevo.SenderName),
                MissingBrevoSettingsMessage
            )
            .ValidateOnStart();

        services.AddHttpClient<IEmailSender, BrevoEmailSender>((serviceProvider, client) =>
        {
            var brevo = serviceProvider.GetRequiredService<IOptions<BrevoOptions>>().Value;

            client.BaseAddress = new Uri(BrevoBaseAddress);
            client.Timeout = BrevoTimeout;
            client.DefaultRequestHeaders.Add(BrevoApiKeyHeader, brevo.ApiKey);
        });

        services.AddMemoryCache();
        services.AddSingleton<IEmailSendLimiter, MemoryEmailSendLimiter>();

        services.AddSingleton<EmailQueue>();
        services.AddSingleton<IEmailQueue>(serviceProvider => serviceProvider.GetRequiredService<EmailQueue>());
        services.AddHostedService<EmailQueueProcessor>();

        return services;
    }
}