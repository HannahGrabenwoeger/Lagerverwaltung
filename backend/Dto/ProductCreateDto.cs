public class ProductsCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int WarehouseId { get; set; }
}