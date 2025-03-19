using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System.Threading.Tasks;
using System.Linq;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserQueryService _userQuery;
        private readonly RestockProcessor _restockProcessor;

        public TestController(AppDbContext context, UserQueryService userQuery, RestockProcessor restockProcessor)
        {
            _context = context;
            _userQuery = userQuery;
            _restockProcessor = restockProcessor;
        }

        [HttpGet("default")]
        public IActionResult Get() => Ok(new { Message = "Test funktioniert!" });

        [HttpGet("all-warehouses")]
        public async Task<IActionResult> GetAllWarehouses()
        {
            var warehouses = await _context.Warehouses.ToListAsync();
            if (warehouses == null || warehouses.Count == 0)
                return NotFound(new { Message = "Keine Lagerhäuser gefunden." });

            return Ok(warehouses);
        }

        [HttpGet("all-products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();
            if (products == null || products.Count == 0)
                return NotFound(new { Message = "Keine Produkte gefunden." });

            return Ok(products);
        }

        [HttpGet("debug-warehouses")]
        public IActionResult DebugWarehouses()
        {
            var warehouses = _context.Warehouses
                .Select(w => new
                {
                    WarehouseId = w.Id,
                    Name = w.Name,
                    Location = w.Location,
                    ProductCount = w.Products.Count,
                    HasProducts = w.Products.Any(), 
                    ProductIds = w.Products.Select(p => p.Id).ToList()
                })
                .ToList();

            var products = _context.Products
                .Select(p => new
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    WarehouseId = p.WarehouseId
                })
                .ToList();

            var orphanedProducts = products
                .Where(p => !warehouses.Any(w => w.WarehouseId == p.WarehouseId))
                .ToList();

            var emptyWarehouses = warehouses
                .Where(w => !w.HasProducts)
                .ToList();

            return Ok(new
            {
                Warehouses = warehouses,
                Products = products,
                OrphanedProducts = orphanedProducts,  
                EmptyWarehouses = emptyWarehouses  
            });
        }

        [HttpGet("test-restock-email/{productId}")]
        public async Task<IActionResult> TestRestockEmail(Guid productId)
        {
            await _restockProcessor.ProcessRestockAsync(productId);
            return Ok("Restock-Prozess wurde ausgelöst. Bitte Log und E-Mail-Postfach prüfen.");
        }
    }
}