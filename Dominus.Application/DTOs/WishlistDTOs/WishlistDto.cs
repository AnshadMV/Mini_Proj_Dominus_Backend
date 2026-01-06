using System.Collections.Generic;

namespace Dominus.Application.DTOs.WishlistDTOs
{
    public class WishlistDto
    {
        public int WishlistId { get; set; }
        public string UserId { get; set; } = null!;
        public List<WishlistItemDto> Items { get; set; } = new();
    }
}
