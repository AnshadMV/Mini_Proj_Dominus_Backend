using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.AuthDTOs
{
    public class LoginRequestDto
    {
        [DefaultValue("yourEmail")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        [RegularExpression(
    @"^(?![.\s])(?!.*\.\.)([a-zA-Z0-9]+(\.[a-zA-Z0-9]+)*)@[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+$",
    ErrorMessage = "Email must be a valid email address"
)]

        public string Email { get; set; } = null!;

        [DefaultValue("yourPass")]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(50, ErrorMessage = "Password cannot exceed 50 characters")]
        [RegularExpression(
            @"^(?=(?:.*[A-Z]))(?=(?:.*[a-z]))(?=(?:.*\d))(?=(?:.*[@$!%*?&]))(?!.*\s)(?!.*(.)\1\1).{8,50}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, one special character, no spaces, and no repeated characters."
        )]
        //[Required]
        //[MinLength(8)]
        //[MaxLength(50)]
        //[RegularExpression(@"[A-Z]", ErrorMessage = "Password must contain an uppercase letter")]
        //[RegularExpression(@"[a-z]", ErrorMessage = "Password must contain a lowercase letter")]
        //[RegularExpression(@"\d", ErrorMessage = "Password must contain a number")]
        //[RegularExpression(@"[@$!%*?&]", ErrorMessage = "Password must contain a special character")]
        //[RegularExpression(@"^\S*$", ErrorMessage = "Password cannot contain spaces")]
        public string Password { get; set; } = null!;
    }
}
