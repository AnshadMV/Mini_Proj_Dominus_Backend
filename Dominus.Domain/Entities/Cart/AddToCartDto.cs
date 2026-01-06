using System.ComponentModel.DataAnnotations;

namespace Dominus.Application.DTOs.CartDTOs
{
    public class AddToCartDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }
}
