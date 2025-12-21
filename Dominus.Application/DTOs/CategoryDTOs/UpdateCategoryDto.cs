using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.CategoryDTOs
{
    public class UpdateCategoryDto
    {
        [DefaultValue("Category Id")]
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; } = 1;

        [DefaultValue("Updated name")]
        [Required]
        [MaxLength(100)]
        [RegularExpression(
            @"^(?! )(?!.*  )[A-Za-z]+( [A-Za-z]+)*$",
            ErrorMessage = "Category name must contain only letters and spaces"
        )]
        public string Name { get; set; } = "Mobile";

        [MaxLength(500)]
        [DefaultValue("Updated description")]

        public string? Description { get; set; } = "It is device...";

        public bool IsActive { get; set; }
    }
}