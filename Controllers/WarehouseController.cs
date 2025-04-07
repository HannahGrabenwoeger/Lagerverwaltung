using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly AppDbContext _context;

    public WarehouseController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetWarehouses()
    {
        try
        {
            var warehouses = await _context.Warehouses.ToListAsync();

            if (warehouses == null || !warehouses.Any())
                return NotFound(new { message = "Keine Lager gefunden." });

            return Ok(warehouses);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Laden der Warehouses:");
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { message = "Interner Serverfehler", details = ex.Message });
        }
    }

    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouseId(Guid warehouseId)
    {
        try
        {
            var products = await _context.Products
                .Where(p => p.WarehouseId == warehouseId)
                .ToListAsync();

            if (products == null || !products.Any())
                return NotFound(new { message = "Keine Produkte in diesem Lager gefunden." });

            return Ok(products);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Laden der Produkte:");
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { message = "Interner Serverfehler", details = ex.Message });
        }
    }
}