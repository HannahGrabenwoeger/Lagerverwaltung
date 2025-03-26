using Xunit;
using Backend.Controllers;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Google.Cloud.Firestore;

public class WarehouseControllerTests
{
    private FirestoreDb GetFirestoreDb()
    {
        return FirestoreDb.Create("your-project-id");
    }

    [Fact]
    public async Task GetWarehouses_ReturnsWarehouses_WhenExist()
    {
        var db = GetFirestoreDb();
        var warehousesCollection = db.Collection("warehouses");

        await warehousesCollection.Document("warehouse1").SetAsync(new
        {
            Name = "Lager 1",
            Location = "Berlin",
            Products = new List<object>
            {
                new { Name = "Produkt A", Quantity = 10 }
            }
        });

        var controller = new WarehouseController(db);
        var result = await controller.GetWarehouses();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetWarehouses_ReturnsNotFound_WhenEmpty()
    {
        var db = GetFirestoreDb();
        var controller = new WarehouseController(db); 

        var result = await controller.GetWarehouses();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetProductsByWarehouseId_ReturnsProducts()
    {
        var db = GetFirestoreDb();
        var warehousesCollection = db.Collection("warehouses");
        var warehouseId = "warehouse1";

        // Add test data to Firestore
        await warehousesCollection.Document(warehouseId).SetAsync(new
        {
            Products = new List<object>
            {
                new { Name = "Produkt X", Quantity = 5, MinimumStock = 2 }
            }
        });

        var controller = new WarehouseController(db); 
        var result = await controller.GetProductsByWarehouseId(warehouseId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetProductsByWarehouseId_ReturnsNotFound_WhenNoProducts()
    {
        var db = GetFirestoreDb();
        var controller = new WarehouseController(db); 

        var result = await controller.GetProductsByWarehouseId("nonexistent-warehouse-id");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}