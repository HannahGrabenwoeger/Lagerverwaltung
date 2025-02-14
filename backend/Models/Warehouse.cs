using System;
using System.Collections.Generic;

namespace Backend.Models
{
    public class Warehouse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public ICollection<Products> Products { get; set; } = new List<Products>();
}
}