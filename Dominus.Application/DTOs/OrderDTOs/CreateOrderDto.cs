using System.Collections.Generic;

namespace Dominus.Domain.DTOs.OrderDTOs
{
    public class CreateOrderDto
    {
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
