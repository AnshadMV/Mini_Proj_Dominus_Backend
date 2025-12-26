using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.UserProfile
{
    public class ResetPasswordRequestDto
    {
        public string Token { get; set; } = null!;
        [RegularExpression(
            @"^(?=(?:.*[A-Z]))(?=(?:.*[a-z]))(?=(?:.*\d))(?=(?:.*[@$!%*?&]))(?!.*\s)(?!.*(.)\1\1).{8,50}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, one special character, no spaces, and no repeated characters."
        )]
        public string NewPassword { get; set; } = null!;
    }

}
