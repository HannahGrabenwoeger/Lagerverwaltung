using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FirestoreDb _db;
        private readonly IUserQueryService _userQueryService;


        public ReportsController(AppDbContext context, FirestoreDb db, IUserQueryService userQueryService)
        {
            _context = context;
             _db = db;
             _userQueryService = userQueryService;
        }
        
        [HttpGet("find-user/{username}")]
        public async Task<IActionResult> FindUser(string username)
        {
            var user = await _userQueryService.FindUserAsync(username);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [HttpGet("stock-summary")]
        public async Task<IActionResult> GetStockSummary()
        {
            var summary = _context.Products
                .Include(p => p.Warehouse) 
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Quantity,
                    Warehouse = p.Warehouse != null ? p.Warehouse.Name : "Unknown"
                })
                .ToList();

            if (!summary.Any())
                return NotFound(new { message = "No stocks found" });

            return Ok(summary);
        }

        [HttpGet("reports")]
        public IActionResult GetReports()
        {
            return Ok(new { message = "Reports loaded successfully!" });
        }

        [HttpGet("movements-per-day")]
        public async Task<IActionResult> GetMovementsPerDay()
        {
            var movements = await _context.Movements
                .GroupBy(m => new { m.MovementsDate.Date, m.ProductId })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    ProductId = g.Key.ProductId,
                    TotalMovements = g.Count(),
                    MovedQuantity = g.Sum(m => m.Quantity)
                })
                .ToListAsync();

            return Ok(movements);
        }

        [HttpGet("top-restock-products")]
        public async Task<IActionResult> GetTopRestockProducts()
        {
            var topRestocks = await _context.RestockQueue
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalRestocks = g.Count(),
                    TotalQuantity = g.Sum(r => r.Quantity)
                })
                .OrderByDescending(r => r.TotalRestocks)
                .Take(5)
                .ToListAsync();

            return Ok(topRestocks);
        }

        [HttpGet("restocks-by-period")]
        public async Task<IActionResult> GetRestocksByPeriod(string? period)
        {
            if (string.IsNullOrEmpty(period))
                return BadRequest("Period parameter is required");

            var restocks = await _context.RestockQueue
                .Include(r => r.Product)
                .Where(r => r.Quantity > 0 && r.Product != null)
                .ToListAsync();

            var groupedData = restocks
                .GroupBy(r => new { Year = r.RequestedAt.Year })
                .Select(g => new
                {
                    Year = g.Key.Year.ToString(),
                    RestockCount = g.Count(),
                    ProductDetails = g.Select(r => new
                    {
                        Id = r.Product!.Id,
                        Name = r.Product.Name,
                        QuantityAvailable = r.Product.Quantity,
                        MinimumQuantity = r.Product.MinimumStock,
                        WarehouseId = r.Product.WarehouseId,
                        RestockedQuantity = r.Quantity
                    }).ToList()
                })
                .ToList();

            return Ok(groupedData);
        }

        private int GetIsoWeek(DateTime date)
        {
            var day = (int)date.DayOfWeek;
            return ((date.DayOfYear - day + 10) / 7);
        }

        [HttpGet("low-stock-products")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.Quantity < p.MinimumStock)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Quantity,
                    p.MinimumStock
                })
                .ToListAsync();

            return Ok(lowStockProducts);
        }
    }
}