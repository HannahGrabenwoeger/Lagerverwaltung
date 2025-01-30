namespace Backend.Dtos
{
    public class MovementsDto
    {
        public int ProductsId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int Quantity { get; set; }
        public DateTime MovementsDate { get; set; }
    }
}