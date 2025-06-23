using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using Backend.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : RolesController
    {
        public ProductsController(AppDbContext dbContext, AppSettings settings)
            : base(dbContext, settings)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Warehouse)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Unit,
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
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(new
            {
                id = product.Id,
                name = product.Name,
                unit = product.Unit,
                quantity = product.Quantity,
                minimumStock = product.MinimumStock,
                warehouseId = product.WarehouseId,
                warehouseName = product.Warehouse?.Name ?? "Unknown",
                version    = product.Version
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return Unauthorized();

            var role = await GetUserRoleAsync();
            if (role != "manager") return Unauthorized();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid))
                return Unauthorized(new { message = "No UID found in token." });

            var role = await GetUserRoleAsync();
            if (role != "manager")
                return Unauthorized(new { message = "Unauthorized access." });

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            if (product.Version != dto.Version)
                return Conflict(new { message = "Concurrency conflict: The product was modified by another user." });

            product.Name = dto.Name;
            product.Unit = dto.Unit;
            product.Quantity = dto.Quantity;
            product.MinimumStock = dto.MinimumStock;

            product.Version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product updated successfully.",
                newVersion = product.Version
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductsCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Invalid Product" });

            var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == dto.WarehouseId);
            if (!warehouseExists)
                return BadRequest(new { message = "Warehouse does not exist." });

            var duplicate = await _context.Products
                .AnyAsync(p => p.Name == dto.Name && p.WarehouseId == dto.WarehouseId);
            if (duplicate)
                return Conflict(new { message = "Product with this name already exists in this warehouse." });

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Unit = dto.Unit,
                Quantity = dto.Quantity,
                MinimumStock = dto.MinimumStock,
                WarehouseId = dto.WarehouseId,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _context.Entry(product).Reference(p => p.Warehouse).LoadAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new
            {
                product.Id,
                product.Name,
                product.Unit,
                product.Quantity,
                product.MinimumStock,
                product.WarehouseId,
                WarehouseName = product.Warehouse?.Name ?? "Unknown",
                Version = product.Version
            });
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                var lowStock = await _context.Products
                    .Where(p => p.Quantity < p.MinimumStock)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Unit,
                        p.Quantity,
                        p.MinimumStock,
                        p.WarehouseId
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!lowStock.Any())
                    return NotFound(new { message = "No low stock products found" });

                return Ok(lowStock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal error while retrieving inventory", error = ex.Message });
            }
        }
    }
}