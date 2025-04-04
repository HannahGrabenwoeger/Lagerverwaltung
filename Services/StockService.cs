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
        

        public async Task<bool> UpdateStock(Guid productId, int quantity, string movementType, string user)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            if (movementType == "IN") 
            {
                product.Quantity += quantity;
            }
            else if (movementType == "OUT") 
            {
                product.Quantity -= quantity;
            }
            else
            {
                throw new ArgumentException("Invalid movement type");
            }

            var movement = new Movements
            {
                ProductId = productId,
                Quantity = quantity,
                User = user,
                Timestamp = DateTime.UtcNow
            };

            _context.Movements.Add(movement);

            if (product.Quantity < product.MinimumStock)
            {
                _context.RestockQueue.Add(new RestockQueue
                {
                    ProductId = productId,
                    Quantity = product.MinimumStock - product.Quantity,
                    Processed = false,
                    RequestedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}