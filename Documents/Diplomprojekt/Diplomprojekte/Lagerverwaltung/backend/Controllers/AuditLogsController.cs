using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs()
        {
            var logs = await _context.AuditLogs
                .Include(l => l.Product)
                .Select(l => new
                {
                    l.Id,
                    l.Entity,
                    l.Action,
                    l.ProductId,
                    ProductName = l.Product!.Name, 
                    l.QuantityChange,
                    l.User,
                    l.Timestamp
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}