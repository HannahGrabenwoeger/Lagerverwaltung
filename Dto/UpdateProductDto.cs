using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class UpdateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum Stock cannot be negativ.")]
        public int MinimumStock { get; set; }
        public string? Unit { get; set; }
        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}