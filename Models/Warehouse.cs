using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Warehouse
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Warehouse name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [JsonIgnore] 
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}