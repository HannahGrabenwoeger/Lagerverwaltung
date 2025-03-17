using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Products> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Movements> Movements { get; set; }
        public DbSet<RestockQueue> RestockQueue { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Console.WriteLine("OnModelCreating wird aufgerufen!");

            Guid warehouseId1 = Guid.NewGuid();
            Guid warehouseId2 = Guid.NewGuid();

            Guid productId1 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            Guid productId2 = Guid.Parse("44444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = warehouseId1, Name = "Lager A", Location = "Standort A" },
                new Warehouse { Id = warehouseId2, Name = "Lager B", Location = "Standort B" }
            );

            modelBuilder.Entity<Products>().HasData(
                new Products { Id = productId1, Name = "Produkt 1", Quantity = 100, WarehouseId = Guid.NewGuid() },
                new Products { Id = productId2, Name = "Produkt 2", Quantity = 50, WarehouseId = Guid.NewGuid() }
            );

            modelBuilder.Entity<Products>()
                .HasOne(p => p.Warehouse)
                .WithMany(w => w.Products)
                .HasForeignKey(p => p.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

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

            var bossAdminRoleId = Guid.Parse("12345678-1234-1234-1234-123456789012");
            var employeeAdminRoleId = Guid.Parse("23456789-2345-2345-2345-234567890123");

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid> { Id = bossAdminRoleId, Name = "BossAdmin", NormalizedName = "BOSSADMIN" },
                new IdentityRole<Guid> { Id = employeeAdminRoleId, Name = "EmployeeAdmin", NormalizedName = "EMPLOYEEADMIN" }
            );

            Console.WriteLine("Seed-Daten und Beziehungen wurden in OnModelCreating definiert!");
        }
    }
}