using System.ComponentModel.DataAnnotations;

namespace Dominus.Application.DTOs.CartDTOs
{
    public class UpdateCartItemDto
    {
        [Required]
        public int CartItemId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}
