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

            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

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

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = Guid.Parse("11111111-1111-0000-1111-111111111111"),
                    FirebaseUid = "test-user-1",
                    Role = "Manager"
                },
                new UserRole
                {
                    Id = Guid.Parse("22222222-2222-0000-2222-222222222222"),
                    FirebaseUid = "test-user-2",
                    Role = "Employee"
                }
            );

            var warehouseAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var warehouseBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = warehouseAId, Name = "Warehouse A", Location = "Location A" },
                new Warehouse { Id = warehouseBId, Name = "Warehouse B", Location = "Location B" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Product 1",
                    Quantity = 100,
                    MinimumStock = 10,
                    WarehouseId = warehouseAId,
                    RowVersion = new byte[8]
                },
                new Product
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Product 2",
                    Quantity = 50,
                    MinimumStock = 5,
                    WarehouseId = warehouseBId,
                    RowVersion = new byte[8]
                }
            );

            Console.WriteLine("Seed data and relationships were defined in OnModelCreating!");
        }
    }
}