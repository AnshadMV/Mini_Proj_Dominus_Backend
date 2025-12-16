using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.DTOs.ColorDTOs
{
    public class CreateColorDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string HexCode { get; set; } = null!;
    }

}
