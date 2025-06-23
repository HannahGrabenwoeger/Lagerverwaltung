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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class MovementsControllerTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task UpdateStock_AddsQuantity_WhenMovementTypeIsIn()
    {
        using var context = CreateInMemoryDbContext();

        context.UserRoles.Add(new UserRole
        {
            FirebaseUid = "testuser",
            Role = "manager"
        });
        await context.SaveChangesAsync();

        var warehouseId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "TestProdukt",
            Quantity = 10,
            MinimumStock = 0,
            WarehouseId = warehouseId
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var stockService = new StockService(context);
        var auditLog = new Mock<AuditLogService>(context).Object;
        var reportService = new Mock<InventoryReportService>(context).Object;
        var logger = new Mock<ILogger<MovementsController>>().Object;

        var controller = new MovementsController(
            context,
            new AppSettings(),
            stockService,
            auditLog,
            logger,
            reportService
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "testuser")
                }, "mock"))
            }
        };

        var request = new StockUpdateRequest
        {
            ProductId = product.Id,
            Quantity = 5,
            MovementType = "In"
        };

        await controller.UpdateStock(request);

        var updatedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(15, updatedProduct?.Quantity ?? -1);
    }

    [Fact]
    public async Task UpdateStock_ReturnsNotFound_WhenProductDoesNotExist()
    {
        using var context = CreateInMemoryDbContext();

        context.UserRoles.Add(new UserRole
        {
            FirebaseUid = "testuser",
            Role = "manager"
        });
        await context.SaveChangesAsync();

        var stockService = new StockService(context);
        var auditLog = new Mock<AuditLogService>(context).Object;
        var reportService = new Mock<InventoryReportService>(context).Object;
        var logger = new Mock<ILogger<MovementsController>>().Object;

        var controller = new MovementsController(context, new AppSettings(), stockService, auditLog, logger, reportService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, "testuser")
                }, "mock"))
            }
        };

        var request = new StockUpdateRequest
        {
            ProductId = Guid.NewGuid(),
            Quantity = 10,
            MovementType = "In"
        };

        var result = await controller.UpdateStock(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}