using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserQueryService _userQueryService;

        public ReportsController(AppDbContext context, IUserQueryService userQueryService)
        {
            _context = context;
            _userQueryService = userQueryService;
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
                        Warehouse = p.Warehouse != null ? p.Warehouse.Name : "Unknown"
                    })
                    .ToListAsync();

                if (!summary.Any())
                    return NotFound(new { message = "No stocks found" });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in stock-summary: " + ex.Message);
                return StatusCode(500, new { message = "Server error", details = ex.Message });
            }
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
                Console.WriteLine("Error in movements-per-day: " + ex.Message);
                return StatusCode(500, new { message = "Server error", details = ex.Message });
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
                Console.WriteLine("Error in top-restock-products: " + ex.Message);
                return StatusCode(500, new { message = "Server error", details = ex.Message });
            }
        }

        [HttpGet("restocks-by-period")]
        public async Task<IActionResult> GetRestocksByPeriod(string? period)
        {
            try
            {
                if (string.IsNullOrEmpty(period))
                    return BadRequest(new { message = "Period parameter is required (use 'd', 'w', 'm', 'y')" });

                DateTime fromDate = period switch
                {
                    "d" => DateTime.UtcNow.Date,                    // Heute
                    "w" => DateTime.UtcNow.Date.AddDays(-7),        // Letzte 7 Tage
                    "m" => DateTime.UtcNow.Date.AddMonths(-1),      // Letzter Monat
                    "y" => DateTime.UtcNow.Date.AddYears(-1),       // Letztes Jahr
                    _ => throw new ArgumentException("Invalid period value. Use 'd', 'w', 'm', or 'y'.")
                };

                var restocks = await _context.RestockQueue
                    .Include(r => r.Product)
                    .Where(r =>
                        r.Quantity > 0 &&
                        r.Product != null &&
                        r.RequestedAt >= fromDate)
                    .ToListAsync();

                var grouped = restocks
                    .GroupBy(r => r.RequestedAt.Year)
                    .Select(g => new
                    {
                        Year = g.Key.ToString(),
                        Restocks = g.Select(r => new
                        {
                            Date = r.RequestedAt.ToString("yyyy-MM-dd HH:mm"),
                            Product = new
                            {
                                Id = r.Product!.Id,
                                Name = r.Product.Name,
                                QuantityAvailable = r.Product.Quantity,
                                MinimumQuantity = r.Product.MinimumStock,
                                WarehouseId = r.Product.WarehouseId,
                                RestockedQuantity = r.Quantity
                            }
                        }).OrderBy(r => r.Date).ToList()
                    })
                    .OrderBy(g => g.Year)
                    .ToList();

                return Ok(grouped);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler in restocks-by-period: " + ex.Message);
                return StatusCode(500, new { message = "Serverfehler", details = ex.Message });
            }
        }
    }
}