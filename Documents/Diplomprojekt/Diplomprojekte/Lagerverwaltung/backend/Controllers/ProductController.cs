using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models.DTOs;
using Backend.Dtos;

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
                p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.Name : "Unknown"
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
            return NotFound(new { message = "Product not found" });

        return Ok(new
        {
            product.Id,
            product.Name,
            product.Quantity,
            product.WarehouseId,
            WarehouseName = product.Warehouse?.Name ?? "Unknown"
        });
    }

    [HttpPost("update-product")]
    public async Task<IActionResult> UpdateProduct(Guid productId, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (!dto.RowVersion.SequenceEqual(product.RowVersion))
                return Conflict(new { message = "The product has been changed in the meantime" });

            product.Name = dto.Name;
            product.Quantity = dto.Quantity;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Product updated!" });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Conflict(new { message = "Concurrency conflict occurred!", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Unexpected error", error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult AddProducts([FromBody] ProductsCreateDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Quantity = dto.Quantity,
            WarehouseId = dto.WarehouseId
        };

        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetProductsById), new { id = product.Id }, product);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProducts(Guid id)
    {
        var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

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
            return NotFound(new { message = "No products with low stock found" });

        return Ok(lowStock);
    }
}