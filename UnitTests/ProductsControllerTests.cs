using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims;

public class ProductsControllerTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ProductsController CreateController(AppDbContext context, string role = "Manager")
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, role),
            new Claim("user_id", "testuser")
        }));

        var settings = new AppSettings { TestMode = true };

        return new ProductsController(context, settings)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        var context = CreateDbContext();
        var warehouse = new Warehouse { Id = Guid.NewGuid(), Name = "Warehouse 1", Location = "Location A" };
        context.Warehouses.Add(warehouse);

        context.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Quantity = 10,
            MinimumStock = 2,
            WarehouseId = warehouse.Id,
            Warehouse = warehouse
        });

        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var result = await controller.GetProducts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var json = JsonSerializer.Serialize(list);
        Assert.Contains("Test Product", json);
    }

    [Fact]
    public async Task GetProductById_ReturnsCorrectProduct()
    {
        var context = CreateDbContext();
        var warehouse = new Warehouse { Id = Guid.NewGuid(), Name = "Warehouse X", Location = "Berlin" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product A",
            Quantity = 5,
            WarehouseId = warehouse.Id,
            Warehouse = warehouse
        };

        context.Warehouses.Add(warehouse);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var result = await controller.GetProductById(product.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);
        var data = JsonDocument.Parse(json).RootElement;

        Assert.Equal("Product A", data.GetProperty("name").GetString());
        Assert.Equal("Warehouse X", data.GetProperty("warehouseName").GetString());
    }

    [Fact]
    public async Task DeleteProduct_ReturnsUnauthorized_IfNotManager()
    {
        var context = CreateDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Delete Me" };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context, role: "Employee");
        var result = await controller.DeleteProduct(product.Id);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_RemovesProduct_IfManager()
    {
        var context = CreateDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Manager Delete" };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context, role: "Manager");
        var result = await controller.DeleteProduct(product.Id);

        Assert.IsType<NoContentResult>(result);
        var deleted = await context.Products.FindAsync(product.Id);
        Assert.Null(deleted);
    }
}