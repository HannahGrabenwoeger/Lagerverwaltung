using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Services;
using Backend.Services.Firestore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFirestoreDbWrapper _firestoreDbWrapper;
        private readonly IUserQueryService _userQueryService;

        public ReportsController(AppDbContext context, IFirestoreDbWrapper firestoreDbWrapper, IUserQueryService userQueryService)
        {
            _context = context;
            _firestoreDbWrapper = firestoreDbWrapper;
            _userQueryService = userQueryService;
        }

        [HttpGet("find-user/{username}")]
        public async Task<IActionResult> FindUser(string username)
        {
            try
            {
                var user = await _userQueryService.FindUserAsync(username);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler in find-user: " + ex.Message);
                return StatusCode(500, new { message = "Serverfehler", details = ex.Message });
            }
        }

        [HttpGet("stock-summary")]
        public async Task<IActionResult> GetStockSummary()
        {
            try
            {
                var summary = await _context.Products
                    .Include(p => p.Warehouse)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Quantity,
                        Warehouse = p.Warehouse != null ? p.Warehouse.Name : "Unbekannt"
                    })
                    .ToListAsync();

                if (!summary.Any())
                    return NotFound(new { message = "No stocks found" });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler in stock-summary: " + ex.Message);
                return StatusCode(500, new { message = "Serverfehler", details = ex.Message });
            }
        }

        [HttpGet("reports")]
        public IActionResult GetReports()
        {
            return Ok(new { message = "Reports loaded successfully!" });
        }

        [HttpGet("movements-per-day")]
        public async Task<IActionResult> GetMovementsPerDay()
        {
            try
            {
                var movements = await _context.Movements
                    .GroupBy(m => new { Date = m.MovementsDate.Date, m.ProductId })
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
            catch (Exception ex)
            {
                Console.WriteLine("Fehler in movements-per-day: " + ex.Message);
                return StatusCode(500, new { message = "Serverfehler", details = ex.Message });
            }
        }

        [HttpGet("top-restock-products")]
        public async Task<IActionResult> GetTopRestockProducts()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine("Fehler in top-restock-products: " + ex.Message);
                return StatusCode(500, new { message = "Serverfehler", details = ex.Message });
            }
        }

        [HttpGet("restocks-by-period")]
        public async Task<IActionResult> GetRestocksByPeriod(string? period)
        {
            try
            {
                if (string.IsNullOrEmpty(period))
                    return BadRequest(new { message = "Period parameter is required" });

                var restocks = await _context.RestockQueue
                    .Include(r => r.Product)
                    .Where(r => r.Quantity > 0 && r.Product != null)
                    .ToListAsync();

                var groupedData = restocks
                    .GroupBy(r => r.RequestedAt.Year)
                    .Select(g => new
                    {
                        Year = g.Key.ToString(),
                        RestockCount = g.Count(),
                        ProductDetails = g.Select(r => new
                        {
                            Id = r.Product.Id,
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
            catch (Exception ex)
            {
                Console.WriteLine("Fehler in restocks-by-period: " + ex.Message);
                return StatusCode(500, new { message = "Serverfehler", details = ex.Message });
            }
        }

        [HttpGet("low-stock-products")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine("Fehler in low-stock-products: " + ex.Message);
                return StatusCode(500, new { message = "Serverfehler", details = ex.Message });
            }
        }
    }
}