using Xunit;
using Microsoft.AspNetCore.Mvc;
using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Backend.Data;

public class ReportsControllerTests
{
    private class FakeUserQueryService : IUserQueryService
    {
        private readonly Dictionary<string, UserRole> _users = new();

        public void AddUser(string uid, UserRole role) => _users[uid] = role;

        public Task<UserRole?> FindUserAsync(string uid) =>
            Task.FromResult(_users.TryGetValue(uid, out var role) ? role : null);
    }

    private ReportsController CreateController(AppDbContext dbContext, IUserQueryService? userService = null)
    {
        userService ??= new FakeUserQueryService();
        return new ReportsController(dbContext, userService);
    }

    [Fact]
    public async Task GetStockSummary_ReturnsProductList()
    {
        var dbContext = TestDbContextFactory.Create();
        dbContext.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produkt 1",
            Quantity = 5,
            MinimumStock = 2,
            Warehouse = new Warehouse { Id = Guid.NewGuid(), Name = "Lager A", Location = "Ort A" }
        });

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        var result = await controller.GetStockSummary();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.NotEmpty(products);
    }
}