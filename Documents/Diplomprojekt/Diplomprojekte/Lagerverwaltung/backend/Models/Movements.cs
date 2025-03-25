using System;

namespace Backend.Models
{
        public class Movements
    {
        public Guid Id { get; set; }  
        public Guid ProductId { get; set; }  
        public Guid FromWarehouseId { get; set; }  
        public Guid ToWarehouseId { get; set; }  
        public int Quantity { get; set; }
        public DateTime MovementsDate { get; set; }  

        public string User { get; set; } = string.Empty;  
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;  

        public Product? Product { get; set; }
        public Warehouse? FromWarehouse { get; set; }  
        public Warehouse? ToWarehouse { get; set; }  
    }
}