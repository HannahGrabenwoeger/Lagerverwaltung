using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Dtos;
using Backend.Data;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovementsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovements([FromBody] MovementsDto movementsDto)
        {
            if (movementsDto == null)
            {
                return BadRequest(new { message = "Ungültige Bewegungsdaten." });
            }

            Guid productId = Guid.Parse(movementsDto.ProductsId.ToString());
            Guid fromWarehouseId = Guid.Parse(movementsDto.FromWarehouseId.ToString());
            Guid toWarehouseId = Guid.Parse(movementsDto.ToWarehouseId.ToString());

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            var fromWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == fromWarehouseId);
            var toWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == toWarehouseId);
            
            if (product == null || fromWarehouse == null || toWarehouse == null)
            {
                return BadRequest(new { message = "Produkt oder Lager nicht gefunden." });
            }

            // Prüfe, ob genug Bestand vorhanden ist
            if (product.Quantity < movementsDto.Quantity)
            {
                return BadRequest(new { message = "Nicht genügend Bestand im Ursprungslager." });
            }

            // Bestand im Ausgangslager reduzieren
            product.Quantity -= movementsDto.Quantity;

            // Versuche, das Produkt im Ziel-Lager zu finden
            var targetProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Name == product.Name && p.WarehouseId == toWarehouseId);

            if (targetProduct != null)
            {
                // Falls das Produkt im Ziel-Lager existiert, Menge erhöhen
                targetProduct.Quantity += movementsDto.Quantity;
            }
            else
            {
                // Falls das Produkt noch nicht existiert, neues Produkt anlegen
                var newProduct = new Products
                {
                    Id = Guid.NewGuid(),
                    Name = product.Name,
                    Quantity = movementsDto.Quantity,
                    WarehouseId = Guid.Parse(movementsDto.ToWarehouseId.ToString()),
                };
                await _context.Products.AddAsync(newProduct);
            }

            // Bewegung speichern
            var movement = new Movements
            {
                Id = Guid.NewGuid(),  // ✅ Neue UUID für die Bewegung generieren
                ProductsId = Guid.Parse(movementsDto.ProductsId.ToString()),  // ✅ Sicherstellen, dass ProductsId ein Guid ist
                FromWarehouseId = Guid.Parse(movementsDto.FromWarehouseId.ToString()),  // ✅ Sicherstellen, dass FromWarehouseId ein Guid ist
                ToWarehouseId = Guid.Parse(movementsDto.ToWarehouseId.ToString()),  // ✅ Sicherstellen, dass ToWarehouseId ein Guid ist
                Quantity = movementsDto.Quantity,
                MovementsDate = movementsDto.MovementsDate
            };

            await _context.Movements.AddAsync(movement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovementsById), new { id = movement.Id }, movement);
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
                ProductsId = movement.ProductsId,
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
    }
}