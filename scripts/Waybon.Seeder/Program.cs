using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Waybon.Application.Common.Exceptions;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Roles.Dtos;
using Waybon.Infrastructure;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddInfrastructure(builder.Configuration);

using var host = builder.Build();
var roleService = host.Services.GetRequiredService<IRoleService>();

foreach (var name in new[] { "admin", "user" })
{
    try
    {
        var role = await roleService.CreateAsync(new CreateRoleRequest { Name = name });
        Console.WriteLine($"Rol '{role.Name}' creado el {role.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC.");
    }
    catch (ConflictException)
    {
        Console.WriteLine($"Rol '{name}' ya existe, se omite.");
    }
}