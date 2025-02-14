using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly AppDbContext _context;

    // Konstruktor, der den DbContext injiziert
    public WarehouseController(AppDbContext context)
    {
        _context = context;
    }

    // 📌 Alle Warehouses zurückgeben
    [HttpGet]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _context.Warehouses.ToListAsync();  // Abfrage der Warehouses aus der DB
        return Ok(warehouses);
    }

    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouseId(Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == warehouseId);

        if (warehouse == null)
        {
            return NotFound(new { message = "Lager nicht gefunden" });
        }

        var products = await _context.Products
            .Where(p => p.WarehouseId == warehouseId)
            .ToListAsync();

        return Ok(new { warehouse, products });
    }
}