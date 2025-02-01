using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Data; 
using Backend.Models;

namespace backend.Services
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
                throw new ArgumentException("Ungültiger Bewegungstyp");
            }

            // Lagerbewegung speichern
            var movement = new Movements
            {
                ProductId = productId,
                Quantity = quantity,
                MovementType = movementType,
                User = user,
                Timestamp = DateTime.UtcNow
            };

            _context.Movements.Add(movement);

            // Nach jeder Lagerbewegung Audit-Log speichern
            _context.AuditLogs.Add(new AuditLog
            {
                Entity = "Product",
                Action = movementType == "IN" ? "Stock Added" : "Stock Removed",
                User = user,
                Timestamp = DateTime.UtcNow
            });

            // Falls Mindestbestand unterschritten wird → Nachbestellung auslösen
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