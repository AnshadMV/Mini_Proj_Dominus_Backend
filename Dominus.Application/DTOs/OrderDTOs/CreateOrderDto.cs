using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.OrderDTOs
{
    public class CreateOrderDto
    {
        [DefaultValue("")]

        public string ShippingAddress { get; set; } = null!;

        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int ColorId { get; set; }
        [Range(1, 20, ErrorMessage = "Quantity must be between 1 and 20")]

        public int Quantity { get; set; }
    }
}
