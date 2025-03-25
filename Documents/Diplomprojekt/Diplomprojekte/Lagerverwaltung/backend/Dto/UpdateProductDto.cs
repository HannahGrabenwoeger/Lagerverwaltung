using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs
{
    public class UpdateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public int MinimumStock { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}