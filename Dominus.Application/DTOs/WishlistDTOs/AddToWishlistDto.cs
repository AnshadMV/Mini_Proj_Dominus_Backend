using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.WishlistDTOs
{
    public class AddToWishlistDto
    {
        [Required]
        public int ProductId { get; set; }
    }
}
