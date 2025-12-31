
namespace Dominus.Domain.Entities
{
    public class ShippingAddress : BaseEntity
    {
        public int UserId { get; set; }

        public string AddressLine { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public int Pincode { get; set; } 
        public long Phone { get; set; }

        public User User { get; set; } = null!;
    }
}
