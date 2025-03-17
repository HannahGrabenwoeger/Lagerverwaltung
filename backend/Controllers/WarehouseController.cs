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

    public WarehouseController(AppDbContext context)
    {
        _context = context;
    }

    // 📌 Alle Warehouses zurückgeben
    [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            try
            {
                var warehouses = await _context.Warehouses
                    .Include(w => w.Products)
                    .ToListAsync();

                if (!warehouses.Any())
                    return NotFound(new { message = "Keine Lager gefunden" });

                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ein Fehler ist aufgetreten", error = ex.Message });
            }
        }

    // 📌 Produkte eines bestimmten Lagers abrufen
    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouseId(Guid warehouseId)
    {
        try
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.Products)  // Produkte direkt mitladen
                .FirstOrDefaultAsync(w => w.Id == warehouseId);

            if (warehouse == null)
            {
                return NotFound(new { message = "Lager nicht gefunden" });
            }

            return Ok(new { warehouse, products = warehouse.Products ?? new List<Products>() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ein Fehler ist aufgetreten", error = ex.Message });
        }
    }
}