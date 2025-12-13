using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        //public Category Category { get; set; }

        public string Brand { get; set; }
        public bool InStock { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public string? SpecialOffer { get; set; }

        public int CurrentStock { get; set; }
        public ICollection<ProductSize> AvailableSizes { get; set; } = new List<ProductSize>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
