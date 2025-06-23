using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly AppDbContext _context;

    public WarehouseController(AppDbContext context)
    {
        _context = context;
    }

    private async Task<string?> GetUserRoleAsync()
    {
        var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(uid)) return null;

        return await _context.UserRoles
            .Where(r => r.FirebaseUid == uid)
            .Select(r => r.Role)
            .FirstOrDefaultAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWarehouseById(Guid id)
    {
        var warehouse = await _context.Warehouses
            .Include(w => w.Products)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse == null)
            return NotFound(new { message = "Warehouse not found." });

        var result = new WarehouseDto
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Location = warehouse.Location,
            Products = warehouse.Products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                WarehouseId = p.WarehouseId,
                WarehouseName = warehouse.Name
            }).ToList()
        };

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _context.Warehouses
            .Include(w => w.Products)
            .ToListAsync();

        var result = warehouses.Select(w => new WarehouseDto
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
        }).ToList();

        return Ok(result);
    }

    [HttpGet("products/{warehouseId}")]
    public async Task<IActionResult> GetProductsByWarehouse(Guid warehouseId)
    {
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
        if (!warehouseExists)
            return NotFound(new { message = "Warehouse not found." });

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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto dto)
    {
        var role = await GetUserRoleAsync();
        if (role != "manager")
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Location))
        {
            return BadRequest(new { message = "Name and location are required." });
        }

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Location = dto.Location
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWarehouses), new { id = warehouse.Id }, warehouse);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] UpdateWarehouseDto dto)
    {
        var role = await GetUserRoleAsync();
        if (role != "manager")
            return Forbid();

        var warehouse = await _context.Warehouses.FindAsync(id);
        if (warehouse == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Location))
        {
            return BadRequest(new { message = "Name and location are required." });
        }

        warehouse.Name = dto.Name;
        warehouse.Location = dto.Location;

        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWarehouse(Guid id)
    {
        var role = await GetUserRoleAsync();
        if (role != "manager")
            return Forbid();

        var warehouse = await _context.Warehouses
            .Include(w => w.Products)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (warehouse == null)
            return NotFound();

        if (warehouse.Products.Any())
            return BadRequest(new { message = "Cannot delete a warehouse that contains products." });

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}