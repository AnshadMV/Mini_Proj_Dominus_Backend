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
    @"^(?![.\s])(?!.*\.\.)([a-zA-Z0-9]+(\.[a-zA-Z0-9]+)*)@gmail\.com$",
    ErrorMessage = "Email must be a valid Gmail address ending with @gmail.com"
)]
        public string Email { get; set; } = null!;

        [DefaultValue("yourPass")]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(50, ErrorMessage = "Password cannot exceed 50 characters")]
        [RegularExpression(
            @"^(?=(?:.*[A-Z]))(?=(?:.*[a-z]))(?=(?:.*\d))(?=(?:.*[@$!%*?&]))(?!.*\s)(?!.*(.)\1\1).{8,50}$",
            ErrorMessage = "Password cannot contain spaces"
        )]
        public string Password { get; set; } = null!;
    }
}
