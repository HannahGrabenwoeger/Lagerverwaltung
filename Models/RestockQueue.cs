using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class RestockQueue
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public bool Processed { get; set; } = false;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}