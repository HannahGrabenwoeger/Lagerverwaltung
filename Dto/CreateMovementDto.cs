namespace Backend.Dtos
{
    public class CreateMovementDto
    {
        public Guid ProductId { get; set; }
        public Guid FromWarehouseId { get; set; }
        public Guid ToWarehouseId { get; set; }
        public int Quantity { get; set; }
        public DateTime MovementsDate { get; set; }
    }
}