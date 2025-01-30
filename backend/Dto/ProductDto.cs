public class ProductsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty; // Optional
}