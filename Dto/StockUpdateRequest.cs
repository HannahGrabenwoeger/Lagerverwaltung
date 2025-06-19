namespace Backend.Dtos
{
    public class StockUpdateRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string MovementType { get; set; } = "in";
        public string? User { get; set; }  
    }
}