using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Roles.Dtos;
using Waybon.Infrastructure;

// ===================================
// Host
// ===================================

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddInfrastructure(builder.Configuration);

using var host = builder.Build();
var roleService = host.Services.GetRequiredService<IRoleService>();


// ===================================
// Base roles
// ===================================

foreach (var name in new[] { "admin", "user" })
{
    if (await roleService.GetByNameAsync(name) is not null)
    {
        Console.WriteLine($"Rol '{name}' ya existe, se omite.");
        continue;
    }

    var role = await roleService.CreateAsync(new CreateRoleRequest { Name = name });
    Console.WriteLine($"Rol '{role.Name}' creado el {role.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC.");
}


// ===================================
// Default role
// ===================================

if (await roleService.GetDefaultAsync() is null)
{
    var userRole = await roleService.GetByNameAsync("user");
    if (userRole is not null)
    {
        await roleService.SetDefaultAsync(userRole.Id);
        Console.WriteLine($"Rol '{userRole.Name}' marcado como rol por defecto.");
    }
}