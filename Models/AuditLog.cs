using System;

namespace Backend.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Entity { get; set; } = string.Empty;  
        public string Action { get; set; } = string.Empty;  
        public Guid? ProductId { get; set; }  
        public Product? Product { get; set; }
        public int QuantityChange { get; set; } 
        public string User { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}