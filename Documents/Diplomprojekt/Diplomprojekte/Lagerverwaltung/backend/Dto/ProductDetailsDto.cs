namespace Backend.Dtos
{
    public class ProductsDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;

        public bool IsBelowMinimum => Quantity < MinimumStock;
    }
}