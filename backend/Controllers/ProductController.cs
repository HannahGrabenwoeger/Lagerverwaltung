using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _dbContext.Products
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Quantity,
                p.MinimumStock,
                p.WarehouseId
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public IActionResult GetProductsById(Guid id)
    {
        var product = _dbContext.Products
            .Include(p => p.Warehouse)
            .FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Produkt nicht gefunden" });

        return Ok(new
        {
            product.Id,
            product.Name,
            product.Quantity,
            product.WarehouseId,
            WarehouseName = product.Warehouse?.Name ?? "Unbekannt"
        });
    }

    [HttpPost]
    public IActionResult AddProducts([FromBody] Products product)
    {
        if (product == null || string.IsNullOrEmpty(product.Name))
            return BadRequest(new { message = "Ungültige Produktdaten" });

        product.Id = Guid.NewGuid();
        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetProductsById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Products updated)
    {
        if (updated == null || id != updated.Id)
            return BadRequest(new { message = "Ungültige Produktdaten oder ID stimmt nicht" });

        var product = await _dbContext.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Produkt nicht gefunden" });

        product.Name = updated.Name;
        product.Quantity = updated.Quantity;
        product.MinimumStock = updated.MinimumStock;
        product.WarehouseId = updated.WarehouseId;

        await _dbContext.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProducts(Guid id)
    {
        var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = "Produkt nicht gefunden" });

        _dbContext.Products.Remove(product);
        _dbContext.SaveChanges();
        return NoContent();
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var lowStock = await _dbContext.Products
            .Where(p => p.Quantity < p.MinimumStock)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Quantity,
                p.MinimumStock,
                p.WarehouseId
            })
            .AsNoTracking()
            .ToListAsync();

        if (!lowStock.Any())
            return NotFound(new { message = "Keine Produkte mit niedrigem Bestand gefunden." });

        return Ok(lowStock);
    }
}