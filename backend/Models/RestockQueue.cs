using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class RestockQueue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Products? Product { get; set; }  // <-- nullable gemacht

    public int Quantity { get; set; }
    public bool Processed { get; set; } = false;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
}