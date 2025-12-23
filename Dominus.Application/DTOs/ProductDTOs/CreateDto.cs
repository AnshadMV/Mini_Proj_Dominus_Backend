using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.ProductDTOs
{
    public class CreateProductDto
    {
        [DefaultValue("")]
        [StringLength(100, MinimumLength = 3)]
//        [RegularExpression(
//    @"^(?! )(?!.*  )[A-Za-z0-9][A-Za-z0-9 _-]*[A-Za-z0-9]$",
//    ErrorMessage = "Name cannot start/end with space or contain multiple spaces"
//)]
        //    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9 _-]*$",
        //ErrorMessage = "Product name must start with a letter or number.")]
        [Required(ErrorMessage = "This filed is required")]

        public string Name { get; set; } = null!;
        [DefaultValue("")]
        [RegularExpression(
    @"^[a-zA-Z0-9\s.,!?;:'""()\-_%%&+/]+$",
    ErrorMessage = "Description contains invalid characters."
)]
        [Required(ErrorMessage = "This filed is required")]
        public string Description { get; set; } = null!;
        [DefaultValue("")]

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        //public List<IFormFile> Images { get; set; } = new();
        public int CategoryId { get; set; }
        [DefaultValue("")]

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int CurrentStock { get; set; }

        [MinLength(1, ErrorMessage = "At least one color is required")]
        public List<int> ColorIds { get; set; } = new();

        public bool IsActive { get; set; } = true;
        public bool InStock { get; set; } = true;
        public bool TopSelling { get; set; } = false;
        public bool Status { get; set; } = true;

        [DefaultValue("")]
        public string? Warranty { get; set; }
    }
}
