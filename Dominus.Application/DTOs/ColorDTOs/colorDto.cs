using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.ColorDTOs
{
    
    
    public class ColorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public bool IsActive { get; set; }
    }

}
