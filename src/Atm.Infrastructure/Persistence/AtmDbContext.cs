using Atm.Domain.Accounts;
using Atm.Domain.Operations;
using Atm.Domain.Sessions;
using Atm.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

public sealed class AtmDbContext : DbContext
{
    public AtmDbContext(DbContextOptions<AtmDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<AccountHistoryEntry> AccountHistoryEntries => Set<AccountHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtmDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateFinancialChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ValidateFinancialChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateFinancialChanges()
    {
        ChangeTracker.DetectChanges();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Account && entry.State == EntityState.Deleted)
                throw new InvalidOperationException("Physical account deletion is not supported.");
            if (entry.Entity is BankTransaction or AccountHistoryEntry or CommissionSnapshot
                && entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new InvalidOperationException("Persisted financial records are immutable.");
            }
        }
    }
}
