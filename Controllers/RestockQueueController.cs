using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Dtos;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/restock")]
    public class RestockQueueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestockQueueController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestRestock([FromBody] RestockRequestDto request)
        {
            if (request == null || request.Quantity <= 0)
                return BadRequest(new { message = "Invalid reorder data" });

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
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

        [HttpPut("{id}/process")]
        public async Task<IActionResult> ProcessRestock(Guid id)
        {
            var restock = await _context.RestockQueue.FirstOrDefaultAsync(r => r.Id == id);

            if (restock == null)
                return NotFound(new { message = "Reorder not found" });

            restock.Processed = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reorder marked as completed" });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRestocks()
        {
            var restocks = await _context.RestockQueue
                .Include(r => r.Product)
                .Where(r => r.Product != null)
                .ToListAsync();

            var result = restocks.Select(r => new
            {
                r.Id,
                ProductName = r.Product!.Name,
                r.Quantity,
                r.Processed,
                r.RequestedAt
            });

            return Ok(result);
        }
    }
}