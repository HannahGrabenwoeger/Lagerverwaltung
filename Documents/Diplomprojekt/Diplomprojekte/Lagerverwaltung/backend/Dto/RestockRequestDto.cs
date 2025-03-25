using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class RestockRequestDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}