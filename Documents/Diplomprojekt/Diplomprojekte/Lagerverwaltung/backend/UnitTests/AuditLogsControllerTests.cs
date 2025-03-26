using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Google.Cloud.Firestore;

public class AuditLogsControllerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "AuditLogsTestDb_" + System.Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

   [Fact]
    public async Task GetAuditLogs_ReturnsAuditLogs_WhenLogsExist()
   {
        using var context = CreateInMemoryContext();
        context.AuditLogs.Add(new AuditLog { Entity = "Product", Action = "Test" });
        await context.SaveChangesAsync();

        var controller = new AuditLogsController(context);
        var result = await controller.GetAuditLogs();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var logs = Assert.IsType<List<AuditLog>>(okResult.Value);

        Assert.Single(logs);
        Assert.Equal("Test", logs[0].Action);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsEmptyList_WhenNoLogsExist()
    {
        using var context = CreateInMemoryContext();
        var controller = new AuditLogsController(context);

        var result = await controller.GetAuditLogs();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var logs = Assert.IsType<List<AuditLog>>(okResult.Value);

        Assert.Empty(logs);
    }
}
