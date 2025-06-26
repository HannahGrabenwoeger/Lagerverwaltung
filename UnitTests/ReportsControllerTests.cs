#nullable enable
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ReportsControllerTests
{
    private class FakeUserQueryService : IUserQueryService
    {
        private readonly Dictionary<string, UserRole> _users = new();

        public void AddUser(string uid, UserRole role) => _users[uid] = role;

        public Task<UserRole?> FindUserAsync(string uid) =>
            Task.FromResult(_users.TryGetValue(uid, out var role) ? role : null);

        public Task<string?> GetUserRoleAsync()
        {
            // Für einfache Tests kann ein fixer Wert wie "manager" zurückgegeben werden
            return Task.FromResult<string?>("manager");
        }
    }

    private ReportsController CreateController(AppDbContext dbContext, IUserQueryService? userService = null)
    {
        userService ??= new FakeUserQueryService();
        return new ReportsController(dbContext, userService);
    }

    [Fact]
    public async Task GetStockSummary_ReturnsProductList()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var warehouseId = Guid.NewGuid();

        dbContext.Warehouses.Add(new Warehouse
        {
            Id = warehouseId,
            Name = "Lager A",
            Location = "Ort A"
        });

        dbContext.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produkt 1",
            Quantity = 5,
            MinimumStock = 2,
            WarehouseId = warehouseId
        });

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        // Act
        var result = await controller.GetStockSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetMovementsPerDay_ReturnsGroupedData()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();

        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        dbContext.Warehouses.Add(new Warehouse { Id = warehouseId, Name = "W1", Location = "X" });

        dbContext.Products.Add(new Product
        {
            Id = productId,
            Name = "P1",
            Quantity = 10,
            MinimumStock = 2,
            WarehouseId = warehouseId
        });

        dbContext.Movements.AddRange(
            new Movements
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                FromWarehouseId = warehouseId,
                ToWarehouseId = warehouseId,
                Quantity = 5,
                MovementsDate = DateTime.UtcNow.Date
            },
            new Movements
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                FromWarehouseId = warehouseId,
                ToWarehouseId = warehouseId,
                Quantity = 3,
                MovementsDate = DateTime.UtcNow.Date
            }
        );

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        // Act
        var result = await controller.GetMovementsPerDay();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Single(data);
    }
}