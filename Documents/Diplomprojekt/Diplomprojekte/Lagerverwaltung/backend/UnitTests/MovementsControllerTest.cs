using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;

public class MovementsControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private (StockService?, AuditLogService?, InventoryReportService?) GetMockedServices(AppDbContext context)
    {
        var stockService = new Mock<StockService>(context).Object;
        var auditService = new Mock<AuditLogService>(context).Object;
        var reportService = new Mock<InventoryReportService>(context).Object;

        return (stockService, auditService, reportService);
    }

    private MovementsController CreateController(AppDbContext context)
    {
        var (stockService, auditService, reportService) = GetMockedServices(context);
        var logger = new Mock<ILogger<MovementsController>>().Object;

        return new MovementsController(context, stockService!, auditService!, logger, reportService!);
    }

    [Fact]
    public async Task GetMovements_ReturnsNotFound_WhenNoMovementsExist()
    {
        var context = GetDbContext();
        var controller = CreateController(context);

        var result = await controller.GetMovements();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ReconcileInventory()
    {
        var context = GetDbContext();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produkt A",
            Quantity = 10,
            MinimumStock = 3,
            WarehouseId = Guid.NewGuid()
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        await controller.ReconcileInventory(product.Id, 5);
    }

    [Fact]
    public async Task GetMovementsById_ReturnsMovement_WhenExists()
    {
        var context = GetDbContext();
        var movementId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        context.Products.Add(new Product { Id = productId, Name = "X", Quantity = 10, WarehouseId = warehouseId });
        context.Warehouses.Add(new Warehouse { Id = warehouseId, Name = "Lager A" });
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
        Assert.Equal(productId, dto.ProductsId);
    }

    [Fact]
    public async Task GetMovementsById_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext();
        var controller = CreateController(context);

        var result = await controller.GetMovementsById(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStock_ReturnsBadRequest_WhenInvalidRequest()
    {
        var context = GetDbContext();
        var controller = CreateController(context);

        var request = new MovementsController.StockUpdateRequest
        {
            ProductId = Guid.NewGuid(),
            Quantity = 5,
            MovementType = "in",
            User = "testuser"
        };

        var result = await controller.UpdateStock(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    public class ReconcileInventoryResponse
    {
        public int NewQuantity { get; set; }
    }
}