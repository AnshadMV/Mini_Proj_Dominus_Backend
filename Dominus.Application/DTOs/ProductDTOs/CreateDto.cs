using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.ProductDTOs
{
    public class CreateProductDto
    {
        // ================= BASIC INFO =================
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        // ================= CATEGORY =================
        public int CategoryId { get; set; }

        // ================= STOCK =================
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int CurrentStock { get; set; }

        // ================= COLORS =================
        [MinLength(1, ErrorMessage = "At least one color is required")]
        public List<int> ColorIds { get; set; } = new();

        // ================= FLAGS (NO VALIDATION) =================
        public bool IsActive { get; set; } = true;
        public bool InStock { get; set; } = true;
        public bool TopSelling { get; set; } = false;
        public bool Status { get; set; } = true;

        // ================= OPTIONAL =================
        public string? Warranty { get; set; }
    }
}
