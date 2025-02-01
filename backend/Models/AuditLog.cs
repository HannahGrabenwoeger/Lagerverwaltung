using System;

namespace Backend.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Entity { get; set; } = string.Empty;  // Z. B. "Product", "Movement"
        public string Action { get; set; } = string.Empty;  // "Stock Added", "Stock Removed"
        public string User { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}