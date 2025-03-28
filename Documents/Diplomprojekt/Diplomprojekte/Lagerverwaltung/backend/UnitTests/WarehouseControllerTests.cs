using Xunit;
using Backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Moq;
using Backend.Services.Firestore;
using System.Collections.Generic;

public class WarehouseControllerTests
{
    [Fact]
    public async Task GetWarehouses_ReturnsWarehouses_WhenExist()
    {
        // Arrange
        var mockFirestore = new Mock<IFirestoreDbWrapper>();
        mockFirestore.Setup(f => f.GetWarehousesAsync())
            .ReturnsAsync(new List<object> { new { Id = "1", Name = "Lager A" } });

        var controller = new WarehouseController(mockFirestore.Object);

        // Act
        var result = await controller.GetWarehouses();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var warehouses = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.NotEmpty(warehouses);
    }

    [Fact]
    public async Task GetWarehouses_ReturnsNotFound_WhenEmpty()
    {
        // Arrange
        var mockFirestore = new Mock<IFirestoreDbWrapper>();
        mockFirestore.Setup(f => f.GetWarehousesAsync())
            .ReturnsAsync(new List<object>());

        var controller = new WarehouseController(mockFirestore.Object);

        // Act
        var result = await controller.GetWarehouses();

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetProductsByWarehouseId_ReturnsProducts()
    {
        // Arrange
        var mockFirestore = new Mock<IFirestoreDbWrapper>();
        var warehouseId = "warehouse1";
        mockFirestore.Setup(f => f.GetProductsByWarehouseIdAsync(warehouseId))
            .ReturnsAsync(new List<object> { new { Name = "Produkt X", Quantity = 5 } });

        var controller = new WarehouseController(mockFirestore.Object);

        // Act
        var result = await controller.GetProductsByWarehouseId(warehouseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetProductsByWarehouseId_ReturnsNotFound_WhenNoProducts()
    {
        // Arrange
        var mockFirestore = new Mock<IFirestoreDbWrapper>();
        var warehouseId = "warehouse1";
        mockFirestore.Setup(f => f.GetProductsByWarehouseIdAsync(warehouseId))
            .ReturnsAsync(new List<object>());

        var controller = new WarehouseController(mockFirestore.Object);

        // Act
        var result = await controller.GetProductsByWarehouseId(warehouseId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}