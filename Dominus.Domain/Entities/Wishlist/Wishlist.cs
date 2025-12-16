using Dominus.Domain.Entities;
using System.Collections.Generic;

namespace Dominus.Domain.Entities
{
    public class Wishlist : BaseEntity
    {
        public string UserId { get; set; } = null!;

        public ICollection<WishlistItem> Items { get; set; }
            = new List<WishlistItem>();
    }
}
