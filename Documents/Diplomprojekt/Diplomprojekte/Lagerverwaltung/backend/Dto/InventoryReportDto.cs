using System;
namespace Backend.Dtos{
    public class InventoryReportDto
    {
        public required string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public int TotalMovements { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
