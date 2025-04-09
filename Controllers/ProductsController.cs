using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using Backend.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : RolesController
    {
        public ProductsController(AppDbContext dbContext) : base(dbContext) { }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
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
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == id);

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
            var role = await GetUserRoleAsync();
            if (role != "Manager" && role != "Admin")
                return Unauthorized(new { message = "Only managers or admins can update products." });

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (dto.RowVersion == null || product.RowVersion == null || !dto.RowVersion.SequenceEqual(product.RowVersion))
            return Conflict(new { message = "The product has been changed in the meantime." });

            product.Name = dto.Name;
            product.Quantity = dto.Quantity;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated!" });
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductsCreateDto dto)
        {
            var role = await GetUserRoleAsync();
            if (role != "Manager" && role != "Employee")
                return Unauthorized(new { message = "Only managers or employees can add products." });

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Invalid product data." });

            var product = new Product
            {
                Id = Guid.NewGuid(), 
                Name = dto.Name,
                Quantity = dto.Quantity,
                MinimumStock = dto.MinimumStock,
                WarehouseId = dto.WarehouseId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _context.Entry(product).Reference(p => p.Warehouse).LoadAsync();

            var resultProduct = new
            {
                product.Id,
                product.Name,
                product.Quantity,
                product.MinimumStock,
                product.WarehouseId,
                WarehouseName = product.Warehouse != null ? product.Warehouse.Name : "Unknown"
            };

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, resultProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var role = await GetUserRoleAsync();
            if (role != "Manager")
                return Unauthorized(new { message = "Only managers can delete products." });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            var lowStock = await _context.Products
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
}