using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.ProductDTOs
{
    public class ProductFilterDto
    {
        public int? ProductId { get; set; }

        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public int? ColorId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? IsActive { get; set; }


        [DefaultValue("price / name / createdon")]
        public string? SortBy { get; set; } = "createdOn";
        public bool Descending { get; set; } = true;
    }

}
