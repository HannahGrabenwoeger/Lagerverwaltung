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

        public TestController(AppDbContext context, UserQueryService userQuery)
        {
            _context = context;
            _userQuery = userQuery;
        }

        // Test-Endpunkt, um sicherzustellen, dass der Controller funktioniert
        [HttpGet]
        public IActionResult Get() => Ok(new { Message = "Test funktioniert!" });

        // Endpunkt zum Abrufen aller Lagerhäuser
        [HttpGet("all-warehouses")]
        public async Task<IActionResult> GetAllWarehouses()
        {
            var warehouses = await _context.Warehouses.ToListAsync();
            if (warehouses == null || warehouses.Count == 0)
                return NotFound(new { Message = "Keine Lagerhäuser gefunden." });

            return Ok(warehouses);
        }

        // Endpunkt zum Abrufen aller Produkte
        [HttpGet("all-products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();
            if (products == null || products.Count == 0)
                return NotFound(new { Message = "Keine Produkte gefunden." });

            return Ok(products);
        }

        // Endpunkt für paginierte Abfrage von Produkten
        [HttpGet("paginated-products")]
        public async Task<IActionResult> GetPaginatedProducts(int page = 1, int pageSize = 10)
        {
            var products = await _context.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (products == null || products.Count == 0)
                return NotFound(new { Message = "Keine Produkte auf dieser Seite gefunden." });

            return Ok(products);
        }

        // Debug-Endpunkt für direkte Datenbankabfrage (nur für Entwicklung, später entfernen!)
        [HttpGet("debug-warehouses")]
        public IActionResult DebugContext()
        {
            var warehouses = _context.Warehouses.ToList();
            var products = _context.Products.ToList();
            return Ok(new { Warehouses = warehouses, Products = products });
        }
    }
}