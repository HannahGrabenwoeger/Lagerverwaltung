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
        var warehouses = await _context.Warehouses.ToListAsync();  
        return Ok(warehouses);
    }

    // 📌 Produkte nach WarehouseId abrufen
    [HttpGet("{warehouseId}")]
    public async Task<IActionResult> GetWarehouseById(Guid warehouseId)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == warehouseId);

        if (warehouse == null)
        {
            return NotFound(new { message = "Lager nicht gefunden" });
        }

        return Ok(warehouse);
    }
}