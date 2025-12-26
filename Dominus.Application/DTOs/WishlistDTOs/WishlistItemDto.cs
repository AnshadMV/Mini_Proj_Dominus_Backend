namespace Dominus.Domain.DTOs.WishlistDTOs
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public List<string> Images { get; set; } = new();

    }
}
