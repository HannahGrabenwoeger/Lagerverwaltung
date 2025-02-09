using System;
using System.Collections.Generic;

namespace Backend.Models
{
    public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    // Navigationseigenschaft für Produkte
    public ICollection<Products> Products { get; set; } = new List<Products>();
}
}