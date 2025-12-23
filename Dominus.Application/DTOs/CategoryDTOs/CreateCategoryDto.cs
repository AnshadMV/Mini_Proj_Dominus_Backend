using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.CategoryDTOs
{
    public class CreateCategoryDto
    {
        [DefaultValue("Mobile/Accessories")]
        [Required]
        [MaxLength(20)]
        [RegularExpression(
            @"^(?! )(?!.*  )[A-Za-z]+( [A-Za-z]+)*$",
            ErrorMessage = "Category name must contain only letters and spaces"
        )]
        public string Name { get; set; } = null!;

        [DefaultValue("Definition....")]
        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}