using Backend.Data;
using Backend.Models;
using System;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class AuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAction(string entity, string action, Guid? productId, int quantityChange, string user)
        {
            var log = new AuditLog
            {
                Entity = entity,
                Action = action,
                ProductId = productId,
                QuantityChange = quantityChange,
                User = user,
                Timestamp = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}