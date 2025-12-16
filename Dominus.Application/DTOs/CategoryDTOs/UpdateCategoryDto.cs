using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.CategoryDTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}