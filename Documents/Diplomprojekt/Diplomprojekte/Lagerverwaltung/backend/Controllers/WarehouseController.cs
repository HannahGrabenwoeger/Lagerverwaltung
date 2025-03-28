using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Services.Firestore;
using Google.Cloud.Firestore;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IFirestoreDbWrapper _firestoreDbWrapper;

    public WarehouseController(IFirestoreDbWrapper firestoreDbWrapper)
    {
        _firestoreDbWrapper = firestoreDbWrapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _firestoreDbWrapper.GetWarehousesAsync();

        if (!warehouses.Any())
            return NotFound(new { message = "No warehouses found" });

        return Ok(warehouses);
    }

    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouseId(string warehouseId)
    {
        var products = await _firestoreDbWrapper.GetProductsByWarehouseIdAsync(warehouseId);

        if (!products.Any())
            return NotFound(new { message = "No products found in this warehouse" });

        return Ok(products);
    }
}
