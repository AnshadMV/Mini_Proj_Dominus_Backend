namespace Dominus.Domain.DTOs.OrderDTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int ColorId { get; set; }
        public string ColorName { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal Total => Price * Quantity;
    }
}
