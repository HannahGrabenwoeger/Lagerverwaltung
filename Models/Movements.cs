using System.Text.Json.Serialization;

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
        public string PerformedBy { get; set; } = string.Empty;  
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;  
        
        [JsonIgnore]
        public Product? Product { get; set; }

        [JsonIgnore]
        public Warehouse? FromWarehouse { get; set; }

        [JsonIgnore]
        public Warehouse? ToWarehouse { get; set; }
    }
}