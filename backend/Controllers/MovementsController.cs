using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using System;
using System.Threading.Tasks;
using backend.Services;
using Backend.Services;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly StockService _stockService; 
        private readonly AuditLogService _auditLogService;
        private readonly ILogger<MovementsController> _logger;

        public MovementsController(AppDbContext context, StockService stockService, AuditLogService auditLogService, ILogger<MovementsController> logger)
        {
            _context = context;
            _stockService = stockService;
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "WarehouseManager, Admin")]
        public async Task<IActionResult> CreateMovements([FromBody] MovementsDto movementsDto)
        {
            if (movementsDto == null)
            {
                return BadRequest(new { message = "Ungültige Bewegungsdaten." });
            }

            if (!TryParseGuid(movementsDto.ProductsId, out Guid productId) ||
                !TryParseGuid(movementsDto.FromWarehouseId, out Guid fromWarehouseId) ||
                !TryParseGuid(movementsDto.ToWarehouseId, out Guid toWarehouseId))
            {
                return BadRequest(new { message = "Ungültige Lager- oder Produkt-IDs." });
            }

            try
            {
                var product = await _context.Products.FindAsync(productId);
                var fromWarehouse = await _context.Warehouses.FindAsync(fromWarehouseId);
                var toWarehouse = await _context.Warehouses.FindAsync(toWarehouseId);

                if (product == null || fromWarehouse == null || toWarehouse == null)
                {
                    return NotFound(new { message = "Produkt oder Lager nicht gefunden." });
                }

                if (product.Quantity < movementsDto.Quantity)
                {
                    return BadRequest(new { message = "Nicht genügend Bestand im Ursprungslager." });
                }

                // Bestandsänderung im Ausgangslager
                product.Quantity -= movementsDto.Quantity;

                var targetProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == product.Name && p.WarehouseId == toWarehouseId);

                if (targetProduct != null)
                {
                    targetProduct.Quantity += movementsDto.Quantity;
                }
                else
                {
                    targetProduct = new Products
                    {
                        Id = Guid.NewGuid(),
                        Name = product.Name,
                        Quantity = movementsDto.Quantity,
                        WarehouseId = toWarehouseId,
                    };
                    await _context.Products.AddAsync(targetProduct);
                }

                var movement = new Movements
                {
                    Id = Guid.NewGuid(),  
                    ProductId = productId,  
                    FromWarehouseId = fromWarehouseId,  
                    ToWarehouseId = toWarehouseId,  
                    Quantity = movementsDto.Quantity,
                    MovementsDate = movementsDto.MovementsDate
                };

                await _context.Movements.AddAsync(movement);

                string currentUser = User?.Identity?.Name ?? "System";

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
                _logger.LogInformation("Bestandsbewegung erfolgreich gespeichert: {MovementId}", movement.Id);

                return CreatedAtAction(nameof(GetMovementsById), new { id = movement.Id }, movement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler bei der Bestandsbewegung");
                return StatusCode(500, new { message = "Ein interner Fehler ist aufgetreten." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMovements()
        {
            var movements = await _context.Movements
                .Include(m => m.Products)
                .Include(m => m.FromWarehouse)
                .Include(m => m.ToWarehouse)
                .ToListAsync();

            if (!movements.Any())
            {
                return NotFound(new { message = "Keine Bewegungen gefunden." });
            }

            var movementDtos = movements.Select(m => new MovementsDto
            {
                ProductsId = m.ProductId,
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
                .Include(m => m.Products)
                .Include(m => m.FromWarehouse)
                .Include(m => m.ToWarehouse)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movement == null)
            {
                return NotFound(new { message = "Bewegung nicht gefunden." });
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

        [HttpGet("all-warehouses")]
        public async Task<IActionResult> GetAllWarehouses()
        {
            var warehouses = await _context.Warehouses.ToListAsync();
            return Ok(warehouses);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateStock([FromBody] StockUpdateRequest request)
        {
            bool success = await _stockService.UpdateStock(request.ProductId, request.Quantity, request.MovementType, request.User);
            if (!success) return BadRequest("Fehler: Produkt nicht gefunden oder ungültige Daten.");

            return Ok("Bestand erfolgreich aktualisiert.");
        }

        /// <summary>
        /// Helfer-Methode zum sicheren Parsen von GUIDs.
        /// </summary>
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