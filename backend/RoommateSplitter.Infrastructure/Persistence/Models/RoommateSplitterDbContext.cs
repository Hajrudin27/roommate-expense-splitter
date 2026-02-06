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
    public DbSet<UserRow> Users => Set<UserRow>();
    public DbSet<GroupMemberRow> GroupMembers => Set<GroupMemberRow>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GroupMemberRow>()
            .HasKey(m => new { m.GroupId, m.UserId });

        modelBuilder.Entity<UserRow>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<GroupMemberRow>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
