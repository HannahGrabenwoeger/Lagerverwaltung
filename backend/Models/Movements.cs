namespace Backend.Models
{
    public class Movements
{
    public Guid Id { get; set; }  
    public Guid ProductsId { get; set; }  
    public Guid FromWarehouseId { get; set; }  
    public Guid ToWarehouseId { get; set; }  
    public int Quantity { get; set; }
    public DateTime MovementsDate { get; set; }

    public Products? Products { get; set; }  // ✅ Nullable gemacht
    public Warehouse? FromWarehouse { get; set; }  // ✅ Nullable gemacht
    public Warehouse? ToWarehouse { get; set; }  // ✅ Nullable gemacht
}
}