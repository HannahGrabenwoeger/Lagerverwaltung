using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Google.Cloud.Firestore;
using System.Collections.Generic;

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

    private Mock<FirestoreDb> GetMockFirestoreDb()
    {
        var mockDb = new Mock<FirestoreDb>();
        var mockCollection = new Mock<CollectionReference>();

        mockDb.Setup(db => db.Collection(It.IsAny<string>())).Returns(mockCollection.Object);
        return mockDb;
    }

    private ReportsController CreateController(FirestoreDb db, IUserQueryService userService = null!)
    {
        userService ??= new FakeUserQueryService();
        return new ReportsController(null!, db, userService);
    }

    [Fact]
    public async Task GetStockSummary_ReturnsProducts()
    {
        // Use the mock firestore db instead of real one
        var db = GetMockFirestoreDb().Object;
        var collection = db.Collection("products");

        // Add test data to Firestore
        await collection.Document("product1").SetAsync(new { Name = "Testprodukt", Quantity = 10, WarehouseName = "Lager A" });

        var controller = CreateController(db);
        var result = await controller.GetStockSummary();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotEmpty((IEnumerable<object>)okResult.Value);
    }
}