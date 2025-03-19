using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserQueryService _userQuery;

        public ReportsController(AppDbContext context, UserQueryService userQuery)
        {
            _context = context;
            _userQuery = userQuery;
        }

        [HttpGet("stock-summary")]
        public IActionResult GetStockSummary()
        {
            var summary = _context.Products
                .Include(p => p.Warehouse) 
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Quantity,
                    Warehouse = p.Warehouse != null ? p.Warehouse.Name : "Unbekannt"
                })
                .ToList();
            if (!summary.Any())
            {
                return NotFound(new { message = "Keine Bestände gefunden." });
            }
            return Ok(summary);
        }

        [HttpGet("reports")]
        public IActionResult GetReports()
        {
            return Ok(new { message = "Reports erfolgreich geladen!" });
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
    public async Task<IActionResult> GetRestocksByPeriod(string period)
    {
        if (string.IsNullOrEmpty(period))
        {
            return BadRequest("Period parameter is required.");
        }

        var query = _context.RestockQueue
            .Include(r => r.Product)
            .Where(r => r.Quantity > 0)
            .AsQueryable();

        var groupedData = query
            .GroupBy(r => new 
            { 
                Year = r.RequestedAt.Year
            })
            .Select(g => new 
            {
                Period = g.Key.Year.ToString(),
                TotalRestocks = g.Count(),
                Products = g.Select(r => new 
                {
                    ProductId = r.ProductId,
                    Name = r.Product.Name,
                    QuantityRestocked = r.Quantity
                }).ToList()
            });

        var result = await groupedData.ToListAsync();
        return Ok(result);
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

        [HttpGet("find-user")]
        public async Task<IActionResult> FindUser(string username)
        {
            var user = await _userQuery.FindUserAsync(username);
            if (user == null)
            {
                return NotFound(new { message = "Benutzer nicht gefunden" });
            }

            return Ok(new { username = user.UserName, email = user.Email });
        }
    }
}