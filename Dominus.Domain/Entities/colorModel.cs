using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Entities
{
    public class Color : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public ICollection<ProductColors> ProductColors { get; set; }
            = new List<ProductColors>();
    }
}

