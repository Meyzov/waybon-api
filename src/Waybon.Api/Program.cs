using Asp.Versioning;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using Waybon.Api.Exceptions;
using Waybon.Api.Logging;
using Waybon.Infrastructure;

// ===================================
// Constants
// ===================================

const string ForceConsoleColorsKey = "Console:ForceColors";
const string ConsoleOutputTemplate = "[{Timestamp:HH:mm:ss}] [{Level:u4}] {SourceContext}{NewLine}----------------- {Message:lj}{NewLine}{Exception}{NewLine}";
const string RootPath = "/";
const string HealthPath = "/health";
const int ApiMajorVersion = 1;
const int ApiMinorVersion = 0;


// ===================================
// Builder
// ===================================

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
builder.Configuration.AddUserSecrets<Program>();


// ===================================
// Logging
// ===================================

var forceConsoleColors = builder.Configuration.GetValue<bool>(ForceConsoleColorsKey);
var consoleTheme = forceConsoleColors ? RenderConsoleTheme.Theme : LocalConsoleTheme.Theme;

builder.Services.AddSerilog(logger => logger
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: consoleTheme,
        outputTemplate: ConsoleOutputTemplate,
        applyThemeToRedirectedOutput: forceConsoleColors)
    );


// ===================================
// Services
// ===================================

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHealthChecks();


// ===================================
// API versioning
// ===================================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(ApiMajorVersion, ApiMinorVersion);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc();


// ===================================
// Error handling
// ===================================

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


// ===================================
// Reverse proxy (Render)
// ===================================

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});


// ===================================
// Pipeline
// ===================================

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, exception) =>
    {
        var statusCode = httpContext.Response.StatusCode;
        var path = httpContext.Request.Path;
        var isHealthCheck = path == RootPath || path.StartsWithSegments(HealthPath);

        if (isHealthCheck && statusCode < 400) return LogEventLevel.Verbose;

        return statusCode switch
        {
            >= 500 => LogEventLevel.Error,
            >= 400 => LogEventLevel.Warning,
            _ when exception is not null => LogEventLevel.Error,
            _ => LogEventLevel.Information
        };
    };
});
app.UseExceptionHandler();
app.MapHealthChecks(RootPath);
app.MapHealthChecks(HealthPath);
app.MapControllers();
app.Run();