using Waybon.Infrastructure;
using Waybon.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", async (AppDbContext db) =>
{
	try
	{
		var connected = await db.Database.CanConnectAsync();
		return connected ? "Funciona" : "No funciona";
	}
	catch (Exception)
	{
		return "No funciona";
	}
});

app.Run();