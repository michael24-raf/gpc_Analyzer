using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User>           Users           => Set<User>();
    public DbSet<GcpAccount>     GcpAccounts     => Set<GcpAccount>();
    public DbSet<CostRecord>     CostRecords     => Set<CostRecord>();
    public DbSet<Budget>         Budgets         => Set<Budget>();
    public DbSet<Alert>          Alerts          => Set<Alert>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            e.Property(u => u.PasswordHash).IsRequired();
        });

        // GcpAccount
        modelBuilder.Entity<GcpAccount>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.ProjectId).IsRequired().HasMaxLength(100);
            e.Property(a => a.BillingAccountId).IsRequired().HasMaxLength(100);
            e.HasOne(a => a.User)
             .WithMany(u => u.GcpAccounts)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // CostRecord
        modelBuilder.Entity<CostRecord>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Amount).HasPrecision(18, 4);
            e.Property(c => c.Currency).HasMaxLength(10);
            e.HasOne(c => c.GcpAccount)
             .WithMany(a => a.CostRecords)
             .HasForeignKey(c => c.GcpAccountId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Budget
        modelBuilder.Entity<Budget>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Amount).HasPrecision(18, 4);
            e.Property(b => b.SpentAmount).HasPrecision(18, 4);
            e.Property(b => b.AlertThreshold).HasPrecision(5, 2);
            e.HasOne(b => b.GcpAccount)
             .WithMany(a => a.Budgets)
             .HasForeignKey(b => b.GcpAccountId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Alert
        modelBuilder.Entity<Alert>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.ThresholdValue).HasPrecision(18, 4);
            e.Property(a => a.ActualValue).HasPrecision(18, 4);
            e.HasOne(a => a.GcpAccount)
             .WithMany(acc => acc.Alerts)
             .HasForeignKey(a => a.GcpAccountId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Budget)
             .WithMany(b => b.Alerts)
             .HasForeignKey(a => a.BudgetId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Recommendation
        modelBuilder.Entity<Recommendation>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.EstimatedSavings).HasPrecision(18, 4);
            e.HasOne(r => r.GcpAccount)
             .WithMany(a => a.Recommendations)
             .HasForeignKey(r => r.GcpAccountId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}