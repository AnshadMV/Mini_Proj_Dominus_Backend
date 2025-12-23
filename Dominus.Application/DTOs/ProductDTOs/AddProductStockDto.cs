using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.ProductDTOs
{
    public class AddProductStockDto
    {
        [DefaultValue("")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
        public int ProductId { get; set; }
        [DefaultValue("")]

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Stock quantity must be at least 1")]
        public int QuantityToAdd { get; set; }
    }
}
