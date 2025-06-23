using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Unit { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }

        public Guid WarehouseId { get; set; }

        [JsonIgnore]
        public Warehouse? Warehouse { get; set; }

        public bool HasSentLowStockNotification { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
    }
}