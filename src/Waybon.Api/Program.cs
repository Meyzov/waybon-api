using Asp.Versioning;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using Waybon.Api.Exceptions;
using Waybon.Api.Logging;
using Waybon.Infrastructure;


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

builder.Services.AddSerilog(logger => logger
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: PastelConsoleTheme.Theme,
        outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] {SourceContext}{NewLine}----------------- {Message:lj}{NewLine}{Exception}{NewLine}",
        applyThemeToRedirectedOutput: true)
    );


// ===================================
// Services
// ===================================

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRouting(options => options.LowercaseUrls = true);


// ===================================
// API versioning
// ===================================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
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
    options.GetLevel = (httpContext, _, exception) => httpContext.Response.StatusCode switch
    {
        >= 500 => LogEventLevel.Error,
        >= 400 => LogEventLevel.Warning,
        _ when exception is not null => LogEventLevel.Error,
        _ => LogEventLevel.Information
    };
});
app.UseExceptionHandler();
app.MapControllers();
app.Run();