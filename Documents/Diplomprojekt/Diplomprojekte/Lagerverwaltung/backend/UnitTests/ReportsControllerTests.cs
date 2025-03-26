using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Cloud.Firestore;

public class ReportsControllerTests
{
    private class FakeUserQueryService : IUserQueryService
    {
        private readonly Dictionary<string, UserRole> _users = new();

        public void AddUser(string username, UserRole user)
        {
            _users[username] = user;
        }

        public Task<UserRole?> FindUserAsync(string username)
        {
            _users.TryGetValue(username, out var user);
            return Task.FromResult<UserRole?>(user);
        }
    }

    private FirestoreDb GetFirestoreDb()
    {
        return FirestoreDb.Create("your-project-id");
    }

    private ReportsController CreateController(FirestoreDb db, IUserQueryService userService = null)
    {
        userService ??= new FakeUserQueryService();
        return new ReportsController(null!, db, userService);
    }

    [Fact]
    public async Task GetStockSummary_ReturnsProducts()
    {
        var db = GetFirestoreDb();
        var collection = db.Collection("products");

        // Add test data to Firestore
        await collection.Document("product1").SetAsync(new { Name = "Testprodukt", Quantity = 10, WarehouseName = "Lager A" });

        var controller = CreateController(db);
        var result = await controller.GetStockSummary();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotEmpty((IEnumerable<object>)okResult.Value);
    }

    [Fact]
    public void GetReports_ReturnsSuccessMessage()
    {
        var db = GetFirestoreDb();
        var controller = CreateController(db);

        var result = controller.GetReports();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMovementsPerDay_ReturnsData()
    {
        var db = GetFirestoreDb();
        var collection = db.Collection("movements");

        // Add test data to Firestore
        await collection.Document("movement1").SetAsync(new { ProductId = "product1", MovementsDate = DateTime.UtcNow, Quantity = 5 });

        var controller = CreateController(db);

        var result = await controller.GetMovementsPerDay();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotEmpty((IEnumerable<object>)okResult.Value);
    }

    [Fact]
    public async Task GetTopRestockProducts_ReturnsTop5()
    {
        var db = GetFirestoreDb();
        var collection = db.Collection("restockQueue");

        // Add test data to Firestore
        for (int i = 0; i < 10; i++)
        {
            await collection.Document($"restock{i}").SetAsync(new { ProductId = "product1", Quantity = 5, RequestedAt = DateTime.UtcNow });
        }

        var controller = CreateController(db);

        var result = await controller.GetTopRestockProducts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotEmpty((IEnumerable<object>)okResult.Value);
    }

    [Fact]
    public async Task GetRestocksByPeriod_ReturnsGroupedData()
    {
        var db = GetFirestoreDb();
        var productCollection = db.Collection("products");
        var restockCollection = db.Collection("restockQueue");

        // Add test data to Firestore
        await productCollection.Document("productA").SetAsync(new { Name = "Produkt A" });
        await restockCollection.Document("restock1").SetAsync(new { ProductId = "productA", Quantity = 5, RequestedAt = DateTime.UtcNow });

        var controller = CreateController(db);

        var result = await controller.GetRestocksByPeriod("year");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotEmpty((IEnumerable<object>)okResult.Value);
    }

    [Fact]
    public async Task GetRestocksByPeriod_ReturnsBadRequest_IfNoPeriod()
    {
        var db = GetFirestoreDb();
        var controller = CreateController(db);

        var result = await controller.GetRestocksByPeriod(null);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetLowStockProducts_ReturnsLowStockList()
    {
        var db = GetFirestoreDb();
        var productCollection = db.Collection("products");

        // Add test data to Firestore
        await productCollection.Document("lowStockProduct").SetAsync(new { Name = "Kritisches Produkt", Quantity = 3, MinimumStock = 5 });

        var controller = CreateController(db);

        var result = await controller.GetLowStockProducts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotEmpty((IEnumerable<object>)okResult.Value);
    }

    [Fact]
    public async Task FindUser_ReturnsUser_WhenExists()
    {
        var db = GetFirestoreDb();
        var fakeUserService = new FakeUserQueryService();

        fakeUserService.AddUser("hannah", new UserRole { FirebaseUid = "some-uid", Role = "Employee" });
        var controller = CreateController(db, fakeUserService);

        var result = await controller.FindUser("hannah");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<UserRole>(okResult.Value);

        Assert.Equal("hannah", user.FirebaseUid);
        Assert.Equal("Employee", user.Role);
    }

    [Fact]
    public async Task FindUser_ReturnsNotFound_WhenUserNotExists()
    {
        var db = GetFirestoreDb();
        var fakeUserService = new FakeUserQueryService();
        var controller = new ReportsController(null, db, fakeUserService);

        var result = await controller.FindUser("ghost");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}