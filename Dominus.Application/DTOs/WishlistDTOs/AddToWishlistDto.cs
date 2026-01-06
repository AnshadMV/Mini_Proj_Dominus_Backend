using System.ComponentModel.DataAnnotations;

namespace Dominus.Application.DTOs.WishlistDTOs
{
    public class AddToWishlistDto
    {
        [Required]
        public int ProductId { get; set; }
    }
}
