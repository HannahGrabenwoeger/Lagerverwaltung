using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Text.Json;
using System;

public class AuditLogsControllerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "AuditLogsTestDb_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsAuditLogs_WhenLogsExist()
    {
        using var context = CreateInMemoryContext();
        context.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), Action = "Test", User = "User" });
        await context.SaveChangesAsync();

        var controller = new AuditLogsController(context);
        var result = await controller.GetAuditLogs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Anonyme Objekte → JSON prüfen
        var json = JsonSerializer.Serialize(okResult.Value);
        var logs = JsonDocument.Parse(json).RootElement;
        Assert.True(logs.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsEmptyList_WhenNoLogsExist()
    {
        using var context = CreateInMemoryContext();
        var controller = new AuditLogsController(context);

        var result = await controller.GetAuditLogs();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var json = JsonSerializer.Serialize(okResult.Value);
        var logs = JsonDocument.Parse(json).RootElement;
        Assert.Equal(0, logs.GetArrayLength());
    }
}