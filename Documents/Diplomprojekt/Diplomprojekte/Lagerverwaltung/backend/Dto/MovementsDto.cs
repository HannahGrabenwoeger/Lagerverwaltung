namespace Backend.Dtos
{
    public class MovementsDto
    {
        public Guid ProductsId { get; set; }  
        public string ProductName { get; set; } = string.Empty;
        public Guid FromWarehouseId { get; set; }  
        public Guid ToWarehouseId { get; set; }  
        public int Quantity { get; set; }
        public DateTime MovementsDate { get; set; }
    }
}


