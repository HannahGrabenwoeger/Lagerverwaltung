using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using Moq;

public class RestockQueueControllerTests
{
    private RestockQueueController CreateController()
    {
        var fakeDbContext = new Mock<Backend.Data.AppDbContext>().Object;
        // var fakeFirestoreDb = FirestoreDb.Create("your-project-id");
        return new RestockQueueController(fakeDbContext);
    }

    [Fact]
    public async Task RequestRestock_AddsToQueue()
    {
        var controller = CreateController();
        var productCollection = FirestoreDb.Create("your-project-id").Collection("products");
        var restockCollection = FirestoreDb.Create("your-project-id").Collection("restockQueue");

        // Add test product to Firestore
        var productId = Guid.NewGuid().ToString();
        await productCollection.Document(productId).SetAsync(new { Name = "Produkt A" });

        var request = new RestockRequestDto { ProductId = Guid.Parse(productId), Quantity = 5 };
        var result = await controller.RequestRestock(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var restock = Assert.IsType<RestockQueue>(okResult.Value);
        Assert.Equal(5, restock.Quantity);
    }

    [Fact]
    public async Task RequestRestock_ReturnsNotFound_IfProductMissing()
    {
        var controller = CreateController();

        var request = new RestockRequestDto { ProductId = Guid.NewGuid(), Quantity = 5 };        
        var result = await controller.RequestRestock(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ProcessRestock_UpdatesProcessedFlag()
    {
        var controller = CreateController();
        var restockCollection = FirestoreDb.Create("your-project-id").Collection("restockQueue");

        // Add test restock entry to Firestore
        var restockId = Guid.NewGuid().ToString();
        await restockCollection.Document(restockId).SetAsync(new { Quantity = 3, Processed = false });

        var result = await controller.ProcessRestock(Guid.Parse(restockId));
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        // Verify that the 'Processed' flag is now true
        var snapshot = await restockCollection.Document(restockId).GetSnapshotAsync();
        Assert.True(snapshot.GetValue<bool>("Processed"));
    }

    [Fact]
    public async Task GetAllRestocks_ReturnsRestocksWithProductName()
    {
        var controller = CreateController();
        var productCollection = FirestoreDb.Create("your-project-id").Collection("products");
        var restockCollection = FirestoreDb.Create("your-project-id").Collection("restockQueue");

        // Add test product and restock entry to Firestore
        var productId = Guid.NewGuid().ToString();
        var restockId = Guid.NewGuid().ToString();

        await productCollection.Document(productId).SetAsync(new { Name = "Produkt X" });
        await restockCollection.Document(restockId).SetAsync(new
        {
            ProductId = productId,
            Quantity = 10,
            RequestedAt = DateTime.UtcNow
        });

        var result = await controller.GetAllRestocks();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.NotEmpty(list);
    }
}