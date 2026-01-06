
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Dominus.Application.DTOs.AuthDTOs
{
    public class RegisterRequestDto
    {
        private string _name = null!;

        [DefaultValue("yourName")]
        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [RegularExpression(
            @"^(?! )(?!.*  )[A-Za-z]+( [A-Za-z]+)*$",
            ErrorMessage = "Name can contain only letters and spaces"
        )]
        public string Name
        {
            get => _name;
            set => _name = value?.Trim() ?? string.Empty;
        }




        [DefaultValue("yourEmail")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters")]

        //official RFC 5322 email pattern 
        /*  [RegularExpression(
        @"^(?![.\s])(?!.*\.\.)([a-zA-Z0-9]+([._%+-][a-zA-Z0-9]+)*)@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$",
        ErrorMessage = "Email must be a valid email address"
    )]*/


        [RegularExpression(
    @"^(?![.\s])(?!.*\.\.)([a-zA-Z0-9]+([._%+-][a-zA-Z0-9]+)*)@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$",
    ErrorMessage = "Email must be a valid email address"
)]
        public string Email { get; set; } = null!;






        [DefaultValue("yourPass")]
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
