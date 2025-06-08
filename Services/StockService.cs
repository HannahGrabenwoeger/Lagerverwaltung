using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Services
{
    public class StockService
    {
        private readonly AppDbContext _context;

        public StockService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateStock(Guid productId, int quantity, string movementType, string performedBy)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            if (movementType == "IN")
            {
                product.Quantity += quantity;
            }
            else if (movementType == "OUT")
            {
                if (product.Quantity < quantity)
                    return false;

                product.Quantity -= quantity;
            }
            else
            {
                throw new ArgumentException("Invalid movement type. Use 'IN' or 'OUT'.");
            }

            var movement = new Movements
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = quantity,
                MovementsDate = DateTime.UtcNow,
                FromWarehouseId = product.WarehouseId, 
                ToWarehouseId = product.WarehouseId,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow
            };

            _context.Movements.Add(movement);

            if (product.Quantity < product.MinimumStock)
            {
                var alreadyQueued = await _context.RestockQueue
                    .AnyAsync(r => r.ProductId == productId && !r.Processed);

                if (!alreadyQueued)
                {
                    _context.RestockQueue.Add(new RestockQueue
                    {
                        ProductId = productId,
                        Quantity = product.MinimumStock - product.Quantity,
                        RequestedAt = DateTime.UtcNow,
                        Processed = false
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}