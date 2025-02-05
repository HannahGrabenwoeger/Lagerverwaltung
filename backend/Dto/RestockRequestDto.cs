using System;

namespace Backend.Dtos
{
    public class RestockRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}