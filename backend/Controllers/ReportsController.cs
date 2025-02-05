using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // 📌 Gesamtbestand pro Lager
        [HttpGet("stock-summary")]
        [Authorize(Roles = "Admin, Lagerist")]
        public async Task<IActionResult> GetStockSummary()
        {
            var summary = await _context.Products
                .GroupBy(p => p.WarehouseId)
                .Select(g => new
                {
                    WarehouseId = g.Key,
                    TotalProducts = g.Count(),
                    TotalQuantity = g.Sum(p => p.Quantity)
                })
                .ToListAsync();

            return Ok(summary);
        }

        // 📌 Bestandsveränderungen pro Tag
        [HttpGet("movements-per-day")]
        public async Task<IActionResult> GetMovementsPerDay()
        {
            var movements = await _context.Movements
                .GroupBy(m => m.MovementsDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalMovements = g.Count(),
                    MovedQuantity = g.Sum(m => m.Quantity)
                })
                .ToListAsync();

            return Ok(movements);
        }

        // 📌 Top 5 nachbestellte Produkte
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

        // 📌 Nachbestellungen pro Zeitraum (Monat oder Woche) – **Korrektur**
        [HttpGet("restocks-per-period")]
        public async Task<IActionResult> GetRestocksPerPeriod([FromQuery] string period = "month")
        {
            var restocks = await _context.RestockQueue
                .ToListAsync(); // Hol alle Daten zuerst, da SQLite `DatePart` nicht unterstützt

            var groupedRestocks = restocks
                .GroupBy(r => period == "month" ? $"{r.RequestedAt:yyyy-MM}" : $"{r.RequestedAt:yyyy}-W{GetIsoWeek(r.RequestedAt)}")
                .Select(g => new
                {
                    Period = g.Key,
                    TotalRestocks = g.Count()
                })
                .ToList();

            return Ok(groupedRestocks);
        }

        // Hilfsfunktion, um die Kalenderwoche zu berechnen
        private int GetIsoWeek(DateTime date)
        {
            var day = (int)date.DayOfWeek;
            return ((date.DayOfYear - day + 10) / 7);
        }

        // 📌 Produkte unter Mindestbestand anzeigen
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