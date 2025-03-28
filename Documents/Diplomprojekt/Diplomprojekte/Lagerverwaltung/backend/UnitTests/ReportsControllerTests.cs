using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Backend.Services.Firestore;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Linq;

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

    private ReportsController CreateController(IFirestoreDbWrapper firestoreWrapper, IUserQueryService? userService = null)
    {
        userService ??= new FakeUserQueryService();
        var dbContext = TestDbContextFactory.Create(); 
        return new ReportsController(dbContext, firestoreWrapper, userService);
    }

    [Fact]
    public async Task GetStockSummary_ReturnsProducts()
    {
        // Arrange
        var mockFirestore = new Mock<IFirestoreDbWrapper>();

        var controller = CreateController(mockFirestore.Object);

        var dbContext = TestDbContextFactory.Create();
        dbContext.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produkt 1",
            Quantity = 5,
            MinimumStock = 2,
            Warehouse = new Warehouse { Name = "Lager A" }
        });
        await dbContext.SaveChangesAsync();

        var controllerWithDb = new ReportsController(dbContext, mockFirestore.Object, new FakeUserQueryService());

        // Act
        var result = await controllerWithDb.GetStockSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.NotEmpty(products);
    }
}