using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Moq;

public class RestockQueueControllerTests
{
    private (RestockQueueController controller, Backend.Data.AppDbContext context) CreateController()
    {
        var options = new DbContextOptionsBuilder<Backend.Data.AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var fakeDbContext = new Backend.Data.AppDbContext(options);
        var controller = new RestockQueueController(fakeDbContext);
        return (controller, fakeDbContext);
    }

    [Fact]
    public async Task RequestRestock_AddsToQueue()
    {
        var (controller, fakeDbContext) = CreateController();

        var productId = Guid.NewGuid();
        fakeDbContext.Products.Add(new Product { Id = productId, Name = "Produkt A" });
        await fakeDbContext.SaveChangesAsync();

        var request = new RestockRequestDto { ProductId = productId, Quantity = 5 };
        var result = await controller.RequestRestock(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var restock = Assert.IsType<RestockQueue>(okResult.Value);
        Assert.Equal(5, restock.Quantity);
    }

    [Fact]
    public async Task RequestRestock_ReturnsNotFound_IfProductMissing()
    {
        var controller = CreateController().controller;

        var request = new RestockRequestDto { ProductId = Guid.NewGuid(), Quantity = 5 };        
        var result = await controller.RequestRestock(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ProcessRestock_UpdatesProcessedFlag()
    {
        var (controller, fakeDbContext) = CreateController();

        var restockId = Guid.NewGuid();
        fakeDbContext.RestockQueue.Add(new RestockQueue { Id = restockId, Quantity = 3, Processed = false });
        await fakeDbContext.SaveChangesAsync();

        var result = await controller.ProcessRestock(restockId);
        var okResult = Assert.IsType<OkObjectResult>(result);

        var restock = await fakeDbContext.RestockQueue.FindAsync(restockId);

        Assert.NotNull(restock);
        Assert.True(restock.Processed);
    }

    [Fact]
    public async Task GetAllRestocks_ReturnsRestocksWithProductName()
    {
        var (controller, fakeDbContext) = CreateController();

        var productId = Guid.NewGuid();
        var restockId = Guid.NewGuid();

        fakeDbContext.Products.Add(new Product { Id = productId, Name = "Produkt X" });
        fakeDbContext.RestockQueue.Add(new RestockQueue
        {
            Id = restockId,
            ProductId = productId,
            Quantity = 10,
            RequestedAt = DateTime.UtcNow
        });
        await fakeDbContext.SaveChangesAsync();

        var result = await controller.GetAllRestocks();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.NotEmpty(list);
    }
}