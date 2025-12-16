using System;
using System.Collections.Generic;
using System.Linq;

namespace Dominus.Domain.DTOs.CartDTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public string UserId { get; set; } = null!;
        public List<CartItemDto> Items { get; set; } = new();

        public decimal GrandTotal => Items.Sum(i => i.Total);
    }
}
