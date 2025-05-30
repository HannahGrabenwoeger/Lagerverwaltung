using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class ProductsCreateDto
    {
        [Required(ErrorMessage = "Porduct name cannot be empty.")]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }
        
        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }
        public string? Unit { get; set; }
    }
}