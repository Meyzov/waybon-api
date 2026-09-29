using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Waybon.Application.Roles.Abstractions;
using Waybon.Application.Roles.Dtos;
using Waybon.Infrastructure;

// ===================================
// Constants
// ===================================

const string AdminRoleName = "admin";
const string UserRoleName = "user";
const string RoleExistsMessage = "Role '{0}' already exists, skipping.";
const string RoleCreatedMessage = "Role '{0}' created on {1:yyyy-MM-dd HH:mm:ss} UTC.";
const string DefaultRoleSetMessage = "Role '{0}' set as the default role.";


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
using var scope = host.Services.CreateScope();
var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();


// ===================================
// Base roles
// ===================================

foreach (var name in new[] { AdminRoleName, UserRoleName })
{
    if (await roleService.GetByNameAsync(name) is not null)
    {
        Console.WriteLine(RoleExistsMessage, name);
        continue;
    }

    var role = await roleService.CreateAsync(new CreateRoleRequest { Name = name });
    Console.WriteLine(RoleCreatedMessage, role.Name, role.CreatedAt);
}


// ===================================
// Default role
// ===================================

if (await roleService.GetDefaultAsync() is null)
{
    var userRole = await roleService.GetByNameAsync(UserRoleName);
    if (userRole is not null)
    {
        await roleService.SetDefaultAsync(userRole.Id);
        Console.WriteLine(DefaultRoleSetMessage, userRole.Name);
    }
}