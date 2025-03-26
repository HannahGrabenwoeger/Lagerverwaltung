using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly FirestoreDb _firestoreDb;

    public WarehouseController(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetWarehouses()
    {
        try
        {
            var snapshot = await _firestoreDb.Collection("warehouses").GetSnapshotAsync();

            var warehouses = snapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                Name = doc.GetValue<string>("Name"),
                Location = doc.GetValue<string>("Location"),
                Products = doc.ContainsField("Products")
                    ? doc.GetValue<List<Dictionary<string, object>>>("Products").Select(p => new
                    {
                        Id = p.ContainsKey("Id") ? p["Id"]?.ToString() : "",
                        Name = p.ContainsKey("Name") ? p["Name"]?.ToString() : "",
                        Quantity = p.ContainsKey("Quantity") ? Convert.ToInt32(p["Quantity"]) : 0
                    }).Cast<object>().ToList()
                    : new List<object>()
            }).ToList();

            if (!warehouses.Any())
                return NotFound(new { message = "No warehouses found" });

            return Ok(warehouses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error has occurred", error = ex.Message });
        }
    }

    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouseId(string warehouseId)
    {
        try
        {
            var docRef = _firestoreDb.Collection("warehouses").Document(warehouseId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists || !snapshot.ContainsField("Products"))
                return NotFound(new { message = "No products found in this warehouse" });

            var productsRaw = snapshot.GetValue<List<Dictionary<string, object>>>("Products");

            var products = productsRaw.Select(p => new
            {
                Id = p.ContainsKey("Id") ? p["Id"]?.ToString() : "",
                Name = p.ContainsKey("Name") ? p["Name"]?.ToString() : "",
                Quantity = p.ContainsKey("Quantity") ? Convert.ToInt32(p["Quantity"]) : 0,
                MinimumStock = p.ContainsKey("MinimumStock") ? Convert.ToInt32(p["MinimumStock"]) : 0
            }).ToList();

            if (!products.Any())
                return NotFound(new { message = "No products found in this warehouse" });

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error has occurred", error = ex.Message });
        }
    }
}