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
        [DefaultValue("ColorId")]
        [Required]

        public int Id { get; set; }

        [DefaultValue("Updatedname")]
        [Required]
        //[RegularExpression(
        //    @"^(?! )(?!.*  )[A-Za-z]+( [A-Za-z]+)*$",
        //    ErrorMessage = "Collor name must contain only letters and spaces"
        //)]
        public string? Name { get; set; } = null!;

        [Required]
        [DefaultValue("#FFAA33")]

        public string? HexCode { get; set; } = null!;

        public bool? IsActive { get; set; }
    }

}
