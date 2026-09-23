using Waybon.Api.Exceptions;
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
// Services
// ===================================

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);


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
// Pipeline
// ===================================

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();
app.Run();