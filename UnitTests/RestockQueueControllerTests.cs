using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class RestockQueueControllerTests
{
    private (RestockQueueController controller, Backend.Data.AppDbContext context) CreateTestController()
    {
        var options = new DbContextOptionsBuilder<Backend.Data.AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var context = new Backend.Data.AppDbContext(options);
        var controller = new RestockQueueController(context);
        return (controller, context);
    }

    [Fact]
    public async Task RequestRestock_ShouldAddItemToQueue()
    {
        var (controller, context) = CreateTestController();
        var productId = Guid.NewGuid();

        context.Products.Add(new Product { Id = productId, Name = "Product A" });
        await context.SaveChangesAsync();

        var request = new RestockRequestDto { ProductId = productId, Quantity = 5 };
        var result = await controller.RequestRestock(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var restock = Assert.IsType<RestockQueue>(okResult.Value);

        Assert.Equal(5, restock.Quantity);
        Assert.Equal(productId, restock.ProductId);
    }

    [Fact]
    public async Task RequestRestock_ShouldReturnNotFound_WhenProductMissing()
    {
        var (controller, _) = CreateTestController();
        var request = new RestockRequestDto { ProductId = Guid.NewGuid(), Quantity = 5 };

        var result = await controller.RequestRestock(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task MarkAsProcessed_ShouldMarkRestockAsProcessed()
    {
        var (controller, context) = CreateTestController();
        var restockId = Guid.NewGuid();

        context.RestockQueue.Add(new RestockQueue { Id = restockId, Quantity = 3, Processed = false });
        await context.SaveChangesAsync();

        var result = await controller.MarkAsProcessed(restockId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = await context.RestockQueue.FindAsync(restockId);

        Assert.NotNull(updated);
        Assert.True(updated!.Processed);
    }

    [Fact]
    public async Task GetAllRestocks_ShouldReturnListWithProductNames()
    {
        var (controller, context) = CreateTestController();
        var productId = Guid.NewGuid();
        var restockId = Guid.NewGuid();

        context.Products.Add(new Product { Id = productId, Name = "Product X" });
        context.RestockQueue.Add(new RestockQueue
        {
            Id = restockId,
            ProductId = productId,
            Quantity = 10,
            RequestedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var result = await controller.GetAllRestocks();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var restocks = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

        Assert.NotEmpty(restocks);
    }

    [Fact]
    public async Task MarkAsProcessed_Twice_StaysProcessed()
    {
        var (controller, context) = CreateTestController();
        var restockId = Guid.NewGuid();

        context.RestockQueue.Add(new RestockQueue { Id = restockId, Quantity = 3, Processed = false });
        await context.SaveChangesAsync();

        await controller.MarkAsProcessed(restockId);
        var result = await controller.MarkAsProcessed(restockId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = await context.RestockQueue.FindAsync(restockId);

        Assert.NotNull(updated);
        Assert.True(updated!.Processed);
    }
}