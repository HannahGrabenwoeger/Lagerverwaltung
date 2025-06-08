using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using Backend.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
   // [Authorize]
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
                id            = product.Id,
                name          = product.Name,
                unit          = product.Unit,
                quantity      = product.Quantity,
                minimumStock  = product.MinimumStock,
                warehouseId   = product.WarehouseId,
                warehouseName = product.Warehouse?.Name ?? "Unknown"
            });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            // UID aus JWT extrahieren
            var uid = User.FindFirst("sub")?.Value;
            Console.WriteLine($"[DEBUG] UID: {uid}");

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized(new { message = "Kein UID im Token gefunden." });
            }

            var role = await GetUserRoleAsync();
            Console.WriteLine($"[DEBUG] Rolle im DeleteProduct: {role}");

            if (role != "Manager" && role != "admin")
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Produkt nicht gefunden" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("update-product")]
        public async Task<IActionResult> UpdateProduct(Guid productId, [FromBody] UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            product.Name = dto.Name;
            product.Unit = dto.Unit;
            product.Quantity = dto.Quantity;
            product.MinimumStock = dto.MinimumStock;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Produkt aktualisiert" });
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductsCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Invalid Product" });

            var duplicate = await _context.Products
                .AnyAsync(p => p.Name == dto.Name && p.WarehouseId == dto.WarehouseId);
            if (duplicate)
                return Conflict(new { message = "Product with this name already exits in this warehouse." });

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Unit = dto.Unit,
                Quantity = dto.Quantity,
                MinimumStock = dto.MinimumStock,
                WarehouseId = dto.WarehouseId
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
                WarehouseName = product.Warehouse?.Name ?? "Unknown"
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
                    return NotFound(new { message = "Keine Produkte mit niedrigem Lagerbestand gefunden" });

                return Ok(lowStock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Interner Fehler beim Abrufen des Lagerbestands", error = ex.Message });
            }
        }
    }
}