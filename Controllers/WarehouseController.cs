using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;

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
        var warehouses = await _context.Warehouses
            .Include(w => w.Products)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name,
                Location = w.Location,
                Products = w.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    MinimumStock = p.MinimumStock,
                    WarehouseId = p.WarehouseId,
                    WarehouseName = w.Name
                }).ToList()
            }).ToListAsync();

        return Ok(warehouses);
    }

    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouse(Guid warehouseId)
    {
        try
        {
            var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
            if (!warehouseExists)
                return NotFound(new { message = "Lager nicht gefunden." });

            var products = await _context.Products
                .Where(p => p.WarehouseId == warehouseId)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    MinimumStock = p.MinimumStock,
                    WarehouseId = p.WarehouseId,
                    WarehouseName = p.Warehouse != null ? p.Warehouse.Name : null
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Fehler beim Abrufen der Produkte", details = ex.Message });
        }
    }
}