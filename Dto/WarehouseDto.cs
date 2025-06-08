namespace Backend.Dtos
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<ProductDto>? Products { get; set; }
    }
}