using Dominus.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Dominus.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public string UserId { get; set; } = null!;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
