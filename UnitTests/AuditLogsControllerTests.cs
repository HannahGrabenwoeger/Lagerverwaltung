using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AuditLogsControllerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AuditLogsTestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsAuditLogs_WhenLogsExist()
    {
        using var context = CreateInMemoryContext();
        var testLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "Create",
            Entity = "Product",
            ProductId = Guid.NewGuid(),
            QuantityChange = 5,
            User = "test-user",
            Timestamp = DateTime.UtcNow
        };

        context.AuditLogs.Add(testLog);
        await context.SaveChangesAsync();

        var controller = new AuditLogsController(context);
        var result = await controller.GetAuditLogs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var logs = Assert.IsAssignableFrom<IEnumerable<AuditLog>>(okResult.Value);

        Assert.Single(logs);
        var returnedLog = Assert.Single(logs);
        Assert.Equal("Create", returnedLog.Action);
        Assert.Equal("Product", returnedLog.Entity);
        Assert.Equal("test-user", returnedLog.User);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsEmpty_WhenNoLogsExist()
    {
        using var context = CreateInMemoryContext();
        var controller = new AuditLogsController(context);

        var result = await controller.GetAuditLogs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var logs = Assert.IsAssignableFrom<IEnumerable<AuditLog>>(okResult.Value);
        Assert.Empty(logs);
    }
}