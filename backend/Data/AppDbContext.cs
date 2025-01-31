using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Products> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Movements> Movements { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Console.WriteLine("OnModelCreating wird aufgerufen!");

            Guid warehouseId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid warehouseId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            Guid productId1 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            Guid productId2 = Guid.Parse("44444444-4444-4444-4444-444444444444");

            // 🔹 ZUERST: Lager speichern
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = warehouseId1, Name = "Lager A", Location = "Standort A" },
                new Warehouse { Id = warehouseId2, Name = "Lager B", Location = "Standort B" }
            );

            // 🔹 DANACH: Produkte speichern (nachdem Lager bereits existieren!)
            modelBuilder.Entity<Products>().HasData(
                new Products { Id = productId1, Name = "Produkt 1", Quantity = 100, WarehouseId = warehouseId1 },
                new Products { Id = productId2, Name = "Produkt 2", Quantity = 50, WarehouseId = warehouseId2 }
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

            Console.WriteLine("Seed-Daten und Beziehungen wurden in OnModelCreating definiert!");
        }
    }
}