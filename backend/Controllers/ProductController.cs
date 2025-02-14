using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using System;
using System.Linq;

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
    public IActionResult GetProducts()
    {
        var products = _dbContext.Products
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Quantity = p.Quantity,
                WarehouseId = p.WarehouseId
            })
            .AsNoTracking()
            .ToList();

        return Ok(products); 
    }

    [HttpGet("{id}")]
    public IActionResult GetProductsById(Guid id) 
    {
        var product = _dbContext.Products
                        .Include(p => p.Warehouse)
                        .FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new { message = "Produkt nicht gefunden" });
        }

        return Ok(new
        {
            Id = product.Id,
            Name = product.Name,
            Quantity = product.Quantity,
            WarehouseId = product.WarehouseId,
            WarehouseName = product.Warehouse?.Name ?? "Unbekannt"
        });
    }

    [HttpPost]
    public IActionResult AddProducts([FromBody] Products product)
    {
        if (product == null || string.IsNullOrEmpty(product.Name))
        {
            return BadRequest(new { message = "Ungültige Produktdaten" });
        }

        product.Id = Guid.NewGuid(); 

        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetProductsById), new { id = product.Id }, product);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProducts(Guid id) 
    {
        var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = "Produkt nicht gefunden" });
        }

        _dbContext.Products.Remove(product);
        _dbContext.SaveChanges();

        return NoContent();
    }
}