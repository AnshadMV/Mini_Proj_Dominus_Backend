using Dominus.Domain.Entities.Dominus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? Warranty { get; set; }
        public bool TopSelling { get; set; } = true;
        public bool Status { get; set; } = true;

        public bool InStock { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public int CurrentStock { get; set; }

        public ICollection<ProductColors> AvailableColors { get; set; }
            = new List<ProductColors>();

        public ICollection<ProductImage> Images { get; set; }
      = new List<ProductImage>();
    }

}