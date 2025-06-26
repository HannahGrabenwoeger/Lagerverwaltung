using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Movements> Movements { get; set; }
        public DbSet<RestockQueue> RestockQueue { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    Console.WriteLine("OnModelCreating is called!");

    modelBuilder.Entity<Product>()
        .HasOne(p => p.Warehouse)
        .WithMany(w => w.Products)
        .HasForeignKey(p => p.WarehouseId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Product>()
        .HasIndex(p => new { p.Name, p.WarehouseId })
        .IsUnique();

    modelBuilder.Entity<Movements>()
        .HasOne(m => m.FromWarehouse)
        .WithMany()
        .HasForeignKey(m => m.FromWarehouseId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Movements>()
        .HasOne(m => m.ToWarehouse)
        .WithMany()
        .HasForeignKey(m => m.ToWarehouseId)
        .OnDelete(DeleteBehavior.Restrict);
    
    modelBuilder.Entity<AuditLog>()
        .HasOne(a => a.Product)
        .WithMany()
        .HasForeignKey(a => a.ProductId)
        .OnDelete(DeleteBehavior.SetNull);

        Console.WriteLine("Seed data and relationships were defined in OnModelCreating!");
        }
    }
}