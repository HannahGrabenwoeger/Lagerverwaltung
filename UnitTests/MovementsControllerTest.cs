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

    private IHttpContextAccessor CreateHttpContextAccessor(string uid)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, uid)
        }, "mock"));

        var context = new DefaultHttpContext { User = user };

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(context);
        return accessor.Object;
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

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "TestProdukt",
            Quantity = 10,
            MinimumStock = 0,
            WarehouseId = Guid.NewGuid()
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = new MovementsController(
            context,
            new AppSettings(),
            new StockService(context),
            new AuditLogService(context),
            new Mock<ILogger<MovementsController>>().Object,
            new Mock<InventoryReportService>(context).Object,
            CreateHttpContextAccessor("testuser")
        );

        var request = new StockUpdateRequest
        {
            ProductId = product.Id,
            Quantity = 5,
            MovementType = "In"
        };

        var result = await controller.UpdateStock(request);

        var updatedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(15, updatedProduct?.Quantity);
        Assert.IsType<OkObjectResult>(result);
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

        var controller = new MovementsController(
            context,
            new AppSettings(),
            new StockService(context),
            new AuditLogService(context),
            new Mock<ILogger<MovementsController>>().Object,
            new Mock<InventoryReportService>(context).Object,
            CreateHttpContextAccessor("testuser")
        );

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