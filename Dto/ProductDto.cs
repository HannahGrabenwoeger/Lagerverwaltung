namespace Backend.Dtos
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
        public Guid WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? Unit { get; set; }
    }
}