using Microsoft.EntityFrameworkCore;

namespace Waybon.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    
}