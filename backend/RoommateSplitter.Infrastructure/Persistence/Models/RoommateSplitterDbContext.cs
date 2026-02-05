using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Infrastructure.Persistence.Models;

namespace RoommateSplitter.Infrastructure.Persistence;

public class RoommateSplitterDbContext : DbContext
{
    public RoommateSplitterDbContext(DbContextOptions<RoommateSplitterDbContext> options)
        : base(options)
    {
    }

    public DbSet<GroupRow> Groups => Set<GroupRow>();
    public DbSet<ExpenseRow> Expenses => Set<ExpenseRow>();
    public DbSet<ExpenseShareRow> ExpenseShares => Set<ExpenseShareRow>();
    public DbSet<PaymentRow> Payments => Set<PaymentRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExpenseRow>()
            .HasMany(e => e.Shares)
            .WithOne()
            .HasForeignKey(s => s.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
