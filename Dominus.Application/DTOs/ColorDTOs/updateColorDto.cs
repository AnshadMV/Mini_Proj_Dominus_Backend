using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.DTOs.ColorDTOs
{
    public class UpdateColorDto
    {
        [DefaultValue("CategoryId")]
        [Required]

        public int Id { get; set; }

        [DefaultValue("Updatedname")]
        [Required]
        [RegularExpression(
            @"^(?! )(?!.*  )[A-Za-z]+( [A-Za-z]+)*$",
            ErrorMessage = "Category name must contain only letters and spaces"
        )]
        public string? Name { get; set; } = null!;

        [Required]
        public string? HexCode { get; set; } = null!;

        public bool? IsActive { get; set; }
    }

}
