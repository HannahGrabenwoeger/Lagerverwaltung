using System;
using Backend.Models;
public class Products
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;  
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }  
    public bool HasSentLowStockNotification { get; set; }
}