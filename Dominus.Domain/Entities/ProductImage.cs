
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Entities
{
    namespace Dominus.Domain.Entities
    {
        public class ProductImage : BaseEntity
        {
            public string ImageUrl { get; set; } = null!;
            public string PublicId { get; set; } = null!;   

            public int ProductId { get; set; }
            public Product Product { get; set; } = null!;
        }
    }

}
