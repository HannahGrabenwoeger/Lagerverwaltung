using Backend.Data;
using Backend.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(AppDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public AuditLogService(AppDbContext context)
            : this(context, NullLogger<AuditLogService>.Instance)
        {
        }

        public async Task<bool> LogActionAsync(string entity, string action, Guid? productId, int quantityChange, string user)
        {
            try
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

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while logging audit entry.");
                return false;
            }
        }

        public async Task<bool> LogSimpleAsync(string entity, string action, string user)
        {
            return await LogActionAsync(entity, action, null, 0, user);
        }
    }
}