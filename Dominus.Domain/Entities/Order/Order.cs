using Dominus.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Dominus.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string? PaymentReference { get; set; }
        public DateTime? PaidOn { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.PendingPayment; 
        public string? UroPayOrderId { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
