using Microsoft.EntityFrameworkCore;
using Newsletter.Api.Sagas;

namespace Newsletter.Api.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Subscriber> Subscribers { get; set; }
    public DbSet<NewsletterOnboardingSagaData> SagaData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewsletterOnboardingSagaData>()
            .HasKey(s => s.CorrelationId);
    }
}
