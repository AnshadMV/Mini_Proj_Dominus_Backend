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

        public string? CurrentPassword { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(50, ErrorMessage = "Password cannot exceed 50 characters")]
        [RegularExpression(
            @"^(?=(?:.*[A-Z]))(?=(?:.*[a-z]))(?=(?:.*\d))(?=(?:.*[@$!%*?&]))(?!.*\s)(?!.*(.)\1\1).{8,50}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, one special character, no spaces, and no repeated characters."
        )]
        public string? NewPassword { get; set; }
    }


}
