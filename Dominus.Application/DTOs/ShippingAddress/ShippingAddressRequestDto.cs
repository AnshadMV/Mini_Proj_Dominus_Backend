using System.ComponentModel.DataAnnotations;

namespace Dominus.Application.DTOs.ShippingAddress
{
    public class ShippingAddressRequestDto
    {
        [Required]
        public string AddressLine { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        [Required]
        public string State { get; set; } = null!;

        [Required]
        [Range(100000, 999999, ErrorMessage = "Pincode must be 6 digits")]
        public int Pincode { get; set; }


        [Required]
        [Range(6000000000, 9999999999, ErrorMessage = "Invalid mobile number")]
        public long Phone { get; set; }
    }
}
