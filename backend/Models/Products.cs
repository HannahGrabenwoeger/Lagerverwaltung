using Backend.Models;
public class Products
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;  
    public int Quantity { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }  
}