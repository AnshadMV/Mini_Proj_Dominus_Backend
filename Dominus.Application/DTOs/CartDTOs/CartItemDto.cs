namespace Dominus.Application.DTOs.CartDTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public List<string> Images { get; set; } = new();

    }
}
