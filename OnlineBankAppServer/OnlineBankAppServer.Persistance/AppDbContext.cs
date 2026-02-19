using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Entities;

namespace OnlineBankAppServer.Persistance;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<BankTransaction> BankTransactions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<Beneficiary> Beneficiaries { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Decimal hassasiyetleri
        modelBuilder.Entity<Account>()
            .Property(p => p.Balance)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<BankTransaction>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

        // İlişkiler
        modelBuilder.Entity<User>()
            .HasMany(u => u.Accounts)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId);
    }
}