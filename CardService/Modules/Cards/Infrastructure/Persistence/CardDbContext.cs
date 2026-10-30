using TuyaCardFlow.Modules.Cards.Domain;
using Microsoft.EntityFrameworkCore;

namespace TuyaCardFlow.Modules.Cards.Infrastructure.Persistence;

public sealed class CardDbContext(DbContextOptions<CardDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CardDbContext).Assembly);
    }
}
