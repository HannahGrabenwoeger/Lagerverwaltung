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

    [HttpGet]
    public async Task<IActionResult> GetWarehouses()
    {
        try
        {
            var warehouses = await _context.Warehouses
                .Include(w => w.Products)
                .Select(w => new
                {
                    w.Id,
                    w.Name,
                    w.Location,
                    Products = w.Products.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Quantity,
                    }).ToList()
                })
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

    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouseId(Guid warehouseId)
    {
        try
        {
            var products = await _context.Products
                .Where(p => p.WarehouseId == warehouseId)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Quantity,
                    p.MinimumStock,
                })
                .ToListAsync();

            if (!products.Any())
            {
                return NotFound(new { message = "Keine Produkte in diesem Lager gefunden" });
            }

            return Ok(products);  
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ein Fehler ist aufgetreten", error = ex.Message });
        }
    }
}