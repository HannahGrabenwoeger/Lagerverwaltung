using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestockQueueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestockQueueController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRestocks()
        {
            var restocks = await _context.RestockQueue
                .Include(r => r.Product)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            var result = restocks.Select(r => new
            {
                r.Id,
                r.ProductId,
                ProductName = r.Product != null ? r.Product.Name : "Unknown",
                r.Quantity,
                r.Processed,
                r.RequestedAt
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestockById(Guid id)
        {
            var restock = await _context.RestockQueue
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restock == null)
                return NotFound(new { message = "Restock not found" });

            return Ok(restock);
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestRestock([FromBody] RestockRequestDto request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var restock = new RestockQueue
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Processed = false,
                RequestedAt = DateTime.UtcNow
            };

            await _context.RestockQueue.AddAsync(restock);
            await _context.SaveChangesAsync();

            return Ok(restock);
        }

        [HttpPost("mark-processed/{id}")]
        public async Task<IActionResult> MarkAsProcessed(Guid id)
        {
            var restock = await _context.RestockQueue.FindAsync(id);
            if (restock == null)
                return NotFound(new { message = "Restock not found" });

            restock.Processed = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Restock marked as processed" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestock(Guid id)
        {
            var restock = await _context.RestockQueue.FindAsync(id);
            if (restock == null)
                return NotFound(new { message = "Restock not found" });

            _context.RestockQueue.Remove(restock);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}