using System.ComponentModel.DataAnnotations;

namespace Dominus.Domain.DTOs.AuthDTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [RegularExpression(
            @"^(?! )(?!.*  )[A-Za-z]+( [A-Za-z]+)*$",
            ErrorMessage = "Name can contain only letters and spaces, and cannot start or end with space"
        )]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        [RegularExpression(
    @"^(?![.\s])(?!.*\.\.)([a-zA-Z0-9]+(\.[a-zA-Z0-9]+)*)@gmail\.com$",
    ErrorMessage = "Email must be a valid Gmail address ending with @gmail.com"
        )]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(50, ErrorMessage = "Password cannot exceed 50 characters")]
        [RegularExpression(
            @"^(?=(?:.*[A-Z]))(?=(?:.*[a-z]))(?=(?:.*\d))(?=(?:.*[@$!%*?&]))(?!.*\s)(?!.*(.)\1\1).{8,50}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, special character and no spaces"
        )]
        public string Password { get; set; } = null!;
    }
}
