namespace Dominus.Application.DTOs.ShippingAddress
{
    public class ShippingAddressDto
    {
        public int Id { get; set; }
        public string AddressLine { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public int Pincode { get; set; }
        public long Phone { get; set; }
        public bool IsActive { get; set; }

    }
}
