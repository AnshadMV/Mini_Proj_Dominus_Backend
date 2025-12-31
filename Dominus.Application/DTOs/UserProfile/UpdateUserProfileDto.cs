using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.UserProfile
{
    public class UpdateProfileRequestDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }

       
    }


}
