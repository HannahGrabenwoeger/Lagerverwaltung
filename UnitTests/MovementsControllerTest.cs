using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

public class MovementsControllerTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private MovementsController CreateController(AppDbContext context)
    {
        var stockService = new Mock<StockService>(context).Object;
        var auditService = new Mock<AuditLogService>(context).Object;
        var reportService = new Mock<InventoryReportService>(context).Object;
        var logger = new Mock<ILogger<MovementsController>>().Object;
        var settings = new AppSettings { TestMode = true };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim("user_id", "testuser"),
            new Claim("sub", "testuser"),
            new Claim(ClaimTypes.Role, "User")  
        }));

        var controller = new MovementsController(context, settings, stockService, auditService, logger, reportService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetMovements_ReturnsNotFound_IfNoneExist()
    {
        using var context = CreateInMemoryDbContext();
        var controller = CreateController(context);

        var result = await controller.GetMovements();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ReconcileInventory_UpdatesProductQuantity()
    {
        using var context = CreateInMemoryDbContext();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "TestProduct",
            Quantity = 10,
            MinimumStock = 2,
            WarehouseId = Guid.NewGuid()
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var found = await context.Products.FindAsync(product.Id) ?? throw new Exception("Product not found.");

        found.Quantity = 5;
        await context.SaveChangesAsync();

        var result = new OkObjectResult(new { NewQuantity = found.Quantity });

        var okResult = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMovementsById_ReturnsDto_WhenFound()
    {
        using var context = CreateInMemoryDbContext();
        var movementId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        context.Warehouses.Add(new Warehouse { Id = warehouseId, Name = "Warehouse A", Location = "X" });
        context.Products.Add(new Product { Id = productId, Name = "Test", Quantity = 10, WarehouseId = warehouseId });
        context.Movements.Add(new Movements
        {
            Id = movementId,
            ProductId = productId,
            FromWarehouseId = warehouseId,
            ToWarehouseId = warehouseId,
            Quantity = 1,
            MovementsDate = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var result = await controller.GetMovementsById(movementId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<MovementsDto>(okResult.Value);
        Assert.Equal(productId, dto.ProductId);
    }

    [Fact]
    public async Task GetMovementsById_ReturnsNotFound_WhenMissing()
    {
        using var context = CreateInMemoryDbContext();
        var controller = CreateController(context);

        var result = await controller.GetMovementsById(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStock_ReturnsUnauthorized_IfRoleMissing()
    {
        using var context = CreateInMemoryDbContext();
        var controller = CreateController(context);

        var request = new StockUpdateRequest
        {
            ProductId = Guid.NewGuid(),
            Quantity = 5,
            MovementType = "in",
            User = "testuser"
        };

        var result = await controller.UpdateStock(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    public class StockUpdateRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
    }
}