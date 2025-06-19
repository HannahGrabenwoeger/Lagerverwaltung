#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using Backend.Dtos;
using Backend.Services;
using static MovementsControllerTests;
using System;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/movements")]
    public class MovementsController : RolesController
    {
        private readonly StockService _stockService;
        private readonly AuditLogService _auditLogService;
        private readonly ILogger<MovementsController> _logger;

        private readonly AppSettings _settings;

        public MovementsController(AppDbContext context, AppSettings settings, StockService stockService, AuditLogService auditLogService, ILogger<MovementsController> logger, InventoryReportService reportService)
            : base(context, settings)
        {
            _stockService = stockService;
            _auditLogService = auditLogService;
            _logger = logger;
            _settings = settings;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMovements([FromBody] MovementsDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid input" });

            if (!TryParseGuid(dto.ProductId, out Guid productId) ||
                !TryParseGuid(dto.FromWarehouseId, out Guid fromWarehouseId) ||
                !TryParseGuid(dto.ToWarehouseId, out Guid toWarehouseId))
            {
                return BadRequest(new { message = "Invalid IDs" });
            }

            var lastMovement = await _context.Movements
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.MovementsDate)
                .FirstOrDefaultAsync();

            if (lastMovement != null &&
                (dto.MovementsDate - lastMovement.MovementsDate).TotalSeconds < 30)
            {
                return BadRequest(new { message = "Duplicate detected" });
            }

            try
            {
                var product = await _context.Products.FindAsync(productId);
                var fromWarehouse = await _context.Warehouses.FindAsync(fromWarehouseId);
                var toWarehouse = await _context.Warehouses.FindAsync(toWarehouseId);

                if (product == null || fromWarehouse == null || toWarehouse == null)
                    return NotFound(new { message = "Product or warehouse not found" });

                if (dto.Quantity <= 0)
                    return BadRequest(new { message = "Quantity must be greater than 0" });

                if (product.Quantity < dto.Quantity)
                    return BadRequest(new { message = "Insufficient stock" });

                product.Quantity -= dto.Quantity;

                var targetProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == product.Name && p.WarehouseId == toWarehouseId);

                if (targetProduct != null)
                {
                    targetProduct.Quantity += dto.Quantity;
                }
                else
                {
                    targetProduct = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = product.Name,
                        Quantity = dto.Quantity,
                        MinimumStock = product.MinimumStock,
                        WarehouseId = toWarehouseId
                    };
                    _context.Products.Add(targetProduct);
                }

                var movement = new Movements
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    FromWarehouseId = fromWarehouseId,
                    ToWarehouseId = toWarehouseId,
                    Quantity = dto.Quantity,
                    MovementsDate = dto.MovementsDate,
                    PerformedBy = User?.Identity?.Name ?? "Unknown"
                };

                await _context.Movements.AddAsync(movement);

                await _auditLogService.LogActionAsync("Movement", "Out", product.Id, -dto.Quantity, movement.PerformedBy);
                await _auditLogService.LogActionAsync("Movement", "In", targetProduct.Id, dto.Quantity, movement.PerformedBy);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Movement saved: {MovementId}", movement.Id);

                return CreatedAtAction(nameof(GetMovementsById), new { id = movement.Id }, movement);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error: {Message}", dbEx.InnerException?.Message);
                return StatusCode(500, new { message = $"Database error: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMovements()
        {
            var movements = await _context.Movements
                .Include(m => m.Product)
                .Include(m => m.FromWarehouse)
                .Include(m => m.ToWarehouse)
                .ToListAsync();

            if (!movements.Any())
                return NotFound(new { message = "No movements found" });

            var result = movements.Select(m => new MovementsDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product?.Name ?? "Unknown",
                FromWarehouseId = m.FromWarehouseId,
                ToWarehouseId = m.ToWarehouseId,
                Quantity = m.Quantity,
                MovementsDate = m.MovementsDate
            });

            return Ok(result);
        }
        
        [HttpPost("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] StockUpdateRequest request)
        {
            var role = await GetUserRoleAsync();
            if (role != "Manager" && role != "admin")
                return Unauthorized(new { message = "Insufficient permissions" });

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (request.MovementType.Equals("in", StringComparison.OrdinalIgnoreCase))
                product.Quantity += request.Quantity;
            else if (request.MovementType.Equals("out", StringComparison.OrdinalIgnoreCase))
                product.Quantity -= request.Quantity;
            else
                return BadRequest(new { message = "Invalid movement type" });

            await _context.SaveChangesAsync();
            return Ok(new { NewQuantity = product.Quantity });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovementsById(Guid id)
        {
            var m = await _context.Movements
                .Include(x => x.Product)
                .Include(x => x.FromWarehouse)
                .Include(x => x.ToWarehouse)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m == null)
                return NotFound(new { message = "Movement not found" });

            return Ok(new MovementsDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product?.Name ?? "",
                FromWarehouseId = m.FromWarehouseId,
                ToWarehouseId = m.ToWarehouseId,
                Quantity = m.Quantity,
                MovementsDate = m.MovementsDate
            });
        }

        private bool TryParseGuid(object input, out Guid result)
        {
            result = Guid.Empty;
            return input != null && Guid.TryParse(input.ToString(), out result);
        }
    }
}