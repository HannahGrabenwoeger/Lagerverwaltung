using Xunit;
using Backend.Controllers;
using Backend.Models;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
            new Claim(ClaimTypes.NameIdentifier, "testuser")
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
    public async Task DeleteProduct_RemovesProduct_IfManager()
    {
        var context = CreateDbContext();

        context.UserRoles.Add(new UserRole
        {
            FirebaseUid = "testuser",
            Role = "Manager"
        });

        var product = new Product { Id = Guid.NewGuid(), Name = "Manager Delete" };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.DeleteProduct(product.Id);

        Assert.IsType<NoContentResult>(result);
        var deleted = await context.Products.FindAsync(product.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsUnauthorized_IfNotManager()
    {
        var context = CreateDbContext();

        context.UserRoles.Add(new UserRole
        {
            FirebaseUid = "testuser",
            Role = "Employee"
        });

        var product = new Product { Id = Guid.NewGuid(), Name = "Should Not Delete" };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = CreateController(context, "Employee");

        var result = await controller.DeleteProduct(product.Id);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}