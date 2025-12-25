using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.DTOs.ColorDTOs
{
    public class CreateColorDto
    {
        [DefaultValue("Colorname")]
        [Required]
        //[RegularExpression(
        //    @"^(?! )(?!.*  )[A-Za-z]+( [A-Za-z]+)*$",
        //    ErrorMessage = "Color name must contain only letters and spaces"
        //)]
        public string Name { get; set; } = null!;
        [DefaultValue("#FFAA33")]

        [Required]
        public string HexCode { get; set; } = null!;
    }

}
