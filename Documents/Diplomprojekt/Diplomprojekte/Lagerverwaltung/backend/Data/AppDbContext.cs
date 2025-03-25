using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System;

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

            Guid warehouseId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid warehouseId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            Guid productId1 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            Guid productId2 = Guid.Parse("44444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = warehouseId1, Name = "Warehouse A", Location = "Location A" },
                new Warehouse { Id = warehouseId2, Name = "Warehouse B", Location = "Location B" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = productId1, Name = "Product 1", Quantity = 100, WarehouseId = warehouseId1 },
                new Product { Id = productId2, Name = "Product 2", Quantity = 50, WarehouseId = warehouseId2 }
            );

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

            Console.WriteLine("Seed data and relationships were defined in OnModelCreating!");
        }
    }
}