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
        var result = controller.GetProductsById(product.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);
        var data = JsonDocument.Parse(json).RootElement;

        Assert.Equal("Produkt A", data.GetProperty("Name").GetString());
        Assert.Equal("Lager 1", data.GetProperty("WarehouseName").GetString());
    }

    [Fact]
    public void AddProducts_ReturnsCreatedResult()
    {
        var context = GetDbContext();
        var controller = CreateController(context);

        var productDto = new ProductsCreateDto 
        { 
            Name = "Neues Produkt", 
            Quantity = 5, 
            WarehouseId = Guid.NewGuid()
        };

        var result = controller.AddProducts(productDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var product = Assert.IsType<Product>(createdResult.Value);
        Assert.Equal("Neues Produkt", product.Name);
    }

    [Fact]
    public async Task UpdateProduct_UpdatesSuccessfully()
    {
        var context = GetDbContext();
        var product = new Product
        { 
            Id = Guid.NewGuid(), 
            Name = "Alt", 
            Quantity = 1, 
            MinimumStock = 1,
            RowVersion = new byte[] { 1, 2, 3 } 
        };
        context.Products.Add(product);
        context.SaveChanges();

        var controller = CreateController(context);

        var dto = new UpdateProductDto
        {
            Name = "Neu",
            Quantity = product.Quantity,
            RowVersion = product.RowVersion 
        };

        var result = await controller.UpdateProduct(product.Id, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Product updated!", okResult.Value!.ToString());
    }

    [Fact]
    public async Task DeleteProduct_RemovesProduct()
    {
        var context = GetDbContext();
        var product = new Product { Id = Guid.NewGuid(), Name = "To delete" };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context);
        var result = await controller.DeleteProducts(product.Id);  

        Assert.IsType<NoContentResult>(result);
        Assert.False(await context.Products.AnyAsync(p => p.Id == product.Id));  
    }

    [Fact]
    public async Task GetLowStockProducts_ReturnsLowStock()
    {
        var context = GetDbContext();
        context.Products.Add(new Product { Id = Guid.NewGuid(), Name = "Low", Quantity = 1, MinimumStock = 5 });
        context.SaveChanges();

        var controller = CreateController(context);
        var result = await controller.GetLowStockProducts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var json = JsonSerializer.Serialize(list);
        Assert.Contains("Low", json);
    }
}