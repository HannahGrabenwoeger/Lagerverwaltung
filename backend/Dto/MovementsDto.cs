public class MovementsDto
{
    public Guid ProductsId { get; set; }  // ❌ Vorher int, jetzt Guid
    public Guid FromWarehouseId { get; set; }  // ❌ Vorher int, jetzt Guid
    public Guid ToWarehouseId { get; set; }  // ❌ Vorher int, jetzt Guid
    public int Quantity { get; set; }
    public DateTime MovementsDate { get; set; }
}