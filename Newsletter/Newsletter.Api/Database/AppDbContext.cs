using Microsoft.EntityFrameworkCore;

namespace Newsletter.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    
}