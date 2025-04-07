using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Backend.Dtos;
using Backend.Models.DTOs;
using Moq;
using System.Security.Claims;

public class ProductsControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private ProductsController CreateController(AppDbContext context)
    {
        return new ProductsController(context);
    }

    private ProductsController CreateControllerWithUser(AppDbContext context, string role)
    {
        var user = new Mock<ClaimsPrincipal>();
        user.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(new Claim(ClaimTypes.Role, role));

        var controller = new ProductsController(context)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user.Object }
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        var context = GetDbContext();
        context.Products.Add(new Product 
        { 
            Id = Guid.NewGuid(), 
            Name = "TestProdukt", 
            Quantity = 10, 
            MinimumStock = 2,
            Warehouse = new Warehouse { Id = Guid.NewGuid(), Name = "Lager 1" }
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetProducts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var json = JsonSerializer.Serialize(list);
        Assert.Contains("TestProdukt", json);
    }

    [Fact]
    public void GetProductById_ReturnsProduct_WhenExists()
    {
        var context = GetDbContext();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produkt A",
            Quantity = 5,
            Warehouse = new Warehouse { Name = "Lager 1" }
        };
        context.Products.Add(product);
        context.SaveChanges();

        var controller = CreateController(context);
        var result = controller.GetProductById(product.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);
        var data = JsonDocument.Parse(json).RootElement;

        Assert.Equal("Produkt A", data.GetProperty("Name").GetString());
        Assert.Equal("Lager 1", data.GetProperty("WarehouseName").GetString());
    }

    [Fact]
    public async Task DeleteProduct_ReturnsUnauthorized_WhenNoManagerRole()
    {
        var context = GetDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "Zu löschen" };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateControllerWithUser(context, "User");

        var result = await controller.DeleteProduct(product.Id);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}