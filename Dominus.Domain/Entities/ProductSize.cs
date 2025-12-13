using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Entities
{
    public class ProductSize : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }   

        public string Size { get; set; }
    }
}
