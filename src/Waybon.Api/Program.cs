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
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: PastelConsoleTheme.Theme,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] {SourceContext}{NewLine}       {Message:lj}{NewLine}{Exception}"));


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
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.MapControllers();
app.Run();