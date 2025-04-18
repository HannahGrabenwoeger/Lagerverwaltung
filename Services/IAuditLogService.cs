namespace Backend.Services
{
    public interface IAuditLogService
    {
        Task<bool> LogActionAsync(string entity, string action, Guid? productId, int quantityChange, string user);
        Task<bool> LogSimpleAsync(string entity, string action, string user);
    }
}