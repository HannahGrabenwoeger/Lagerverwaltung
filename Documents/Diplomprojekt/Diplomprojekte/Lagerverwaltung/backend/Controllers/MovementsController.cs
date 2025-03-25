using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using System;
using System.Threading.Tasks;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Backend.Dtos;
using System.Linq;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/movements")]
    public class MovementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly StockService _stockService;
        private readonly AuditLogService _auditLogService;
        private readonly ILogger<MovementsController> _logger;
        private readonly InventoryReportService _inventoryReportService;

        public MovementsController(AppDbContext context, StockService stockService, AuditLogService auditLogService, ILogger<MovementsController> logger, InventoryReportService inventoryReportService)
        {
            _context = context;
            _stockService = stockService;
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _logger = logger;
            _inventoryReportService = inventoryReportService;
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateMovements([FromBody] MovementsDto movementsDto)  
        {
            if (movementsDto == null)
            {
                return BadRequest(new { message = "Invalid transaction data" });
            }

            if (!TryParseGuid(movementsDto.ProductsId, out Guid productId) ||
                !TryParseGuid(movementsDto.FromWarehouseId, out Guid fromWarehouseId) ||
                !TryParseGuid(movementsDto.ToWarehouseId, out Guid toWarehouseId))
            {
                return BadRequest(new { message = "Invalid warehouse or product IDs" });
            }

            var lastMovement = await _context.Movements
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.MovementsDate)
                .FirstOrDefaultAsync();
            if (lastMovement != null && (movementsDto.MovementsDate - lastMovement.MovementsDate).TotalSeconds < 30)
            {
                return BadRequest(new { message = "Duplicate scan detected" });
            }

            try
            {
                var product = await _context.Products.FindAsync(productId);
                var fromWarehouse = await _context.Warehouses.FindAsync(fromWarehouseId);
                var toWarehouse = await _context.Warehouses.FindAsync(toWarehouseId);

                if (product == null || fromWarehouse == null || toWarehouse == null)
                {
                    return NotFound(new { message = "Product or stock not found." });
                }

                if (movementsDto.Quantity <= 0)
                {
                    return BadRequest(new { message = "Die Menge muss größer als Null sein" });
                }

                if (product.Quantity < movementsDto.Quantity)
                {
                    return BadRequest(new { message = "The quantity must be greater than zero" });
                }

                product.Quantity -= movementsDto.Quantity;

                var targetProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == product.Name && p.WarehouseId == toWarehouseId);

                if (targetProduct != null)
                {
                    targetProduct.Quantity += movementsDto.Quantity; 
                }
                else
                {
                    targetProduct = new Product
                    {
                        Id = product.Id, 
                        Name = product.Name,
                        Quantity = movementsDto.Quantity,
                        WarehouseId = toWarehouseId,
                    };

                    _context.Entry(product).State = EntityState.Detached; 
                    _context.Products.Update(targetProduct);
                }

                var movement = new Movements
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    FromWarehouseId = fromWarehouseId,
                    ToWarehouseId = toWarehouseId,
                    Quantity = movementsDto.Quantity,
                    MovementsDate = movementsDto.MovementsDate,
                };

                await _context.Movements.AddAsync(movement);

                string currentUser = User?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value ?? "Unknown";
                
                await _auditLogService.LogAction(
                    entity: "Movement",
                    action: "Stock Moved",
                    productId: product.Id,
                    quantityChange: -movementsDto.Quantity,
                    user: currentUser
                );

                await _auditLogService.LogAction(
                    entity: "Movement",
                    action: "Stock Received",
                    productId: targetProduct.Id,
                    quantityChange: movementsDto.Quantity,
                    user: currentUser
                );

                await _context.SaveChangesAsync();
                _logger.LogInformation("Inventory movement successfully saved: {MovementId}", movement.Id);

                return CreatedAtAction(nameof(GetMovementsById), new { id = movement.Id }, movement);
            }
            catch (DbUpdateException dbEx)
{
    _logger.LogError(dbEx, "Database error during inventory movement: {ErrorMessage}", dbEx.InnerException?.Message);
    return StatusCode(500, new { message = $"Database error: {dbEx.InnerException?.Message}" });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in inventory movement: {ErrorMessage}", ex.Message);
    return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
}
        }

        [HttpPost("reconcile/{id}")]
        public async Task<IActionResult> ReconcileInventory(Guid id, [FromBody] int scannedQuantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { message = "Product not found" });

            int shrink = product.Quantity - scannedQuantity;
            product.Quantity = scannedQuantity;
            await _context.SaveChangesAsync();

            return Ok(new { productId = id, newQuantity = scannedQuantity, shrinkAmount = shrink });
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
            {
                return NotFound(new { message = "No movements found" });
            }

            var movementDtos = movements.Select(m => new MovementsDto
            {
                ProductsId = m.ProductId,
                ProductName = m.Product?.Name ?? "Unknown",
                FromWarehouseId = m.FromWarehouseId,
                ToWarehouseId = m.ToWarehouseId,
                Quantity = m.Quantity,
                MovementsDate = m.MovementsDate
            }).ToList();

            return Ok(movementDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovementsById(Guid id)
        {
            var movement = await _context.Movements
                .Include(m => m.Product)
                .Include(m => m.FromWarehouse)
                .Include(m => m.ToWarehouse)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movement == null)
            {
                return NotFound(new { message = "Movement not found" });
            }

            var movementDto = new MovementsDto
            {
                ProductsId = movement.ProductId,
                FromWarehouseId = movement.FromWarehouseId,
                ToWarehouseId = movement.ToWarehouseId,
                Quantity = movement.Quantity,
                MovementsDate = movement.MovementsDate
            };

            return Ok(movementDto);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateStock([FromBody] StockUpdateRequest request)
        {
            bool success = await _stockService.UpdateStock(request.ProductId, request.Quantity, request.MovementType, request.User);
            if (!success) return BadRequest("Error: Product not found or invalid data");

            return Ok("Stock updated successfully");
        }

        [HttpGet("inventory-report")]
        public async Task<IActionResult> GetInventoryReport()
        {
            var reportData = await _context.Products
                .Select(p => new InventoryReportDto
                {
                    ProductName = p.Name,
                    TotalQuantity = p.Quantity,
                    TotalMovements = _context.Movements.Count(m => m.ProductId == p.Id),
                    LastUpdated = _context.Movements
                        .Where(m => m.ProductId == p.Id)
                        .OrderByDescending(m => m.MovementsDate)
                        .Select(m => m.MovementsDate)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(reportData);
        }

        private bool TryParseGuid(object input, out Guid result)
        {
            result = Guid.Empty;
            return input != null && Guid.TryParse(input.ToString(), out result);
        }

        public class StockUpdateRequest
        {
            public Guid ProductId { get; set; } 
            public int Quantity { get; set; }
            public string MovementType { get; set; } = string.Empty; 
            public string User { get; set; } = string.Empty;
        }
    }
}