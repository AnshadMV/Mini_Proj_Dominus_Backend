using System;
using System.Collections.Generic;

namespace Dominus.Application.DTOs.OrderDTOs
{
    public class OrderDto   
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public List<OrderItemDto> Items { get; set; } = new();

        public string? RazorOrderId { get; set; }   
        public string? RazorKey { get; set; }


    }
}
