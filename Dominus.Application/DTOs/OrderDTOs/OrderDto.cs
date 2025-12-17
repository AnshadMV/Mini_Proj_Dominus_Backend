using System;
using System.Collections.Generic;

namespace Dominus.Domain.DTOs.OrderDTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
