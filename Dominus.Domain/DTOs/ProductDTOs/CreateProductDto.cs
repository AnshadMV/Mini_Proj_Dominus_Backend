using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.DTOs.ProductDTOs
{

    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public string SpecialOffer { get; set; }
        [Required(ErrorMessage = "At least one size is required")]
        [MinLength(1, ErrorMessage = "At least one size is required")]
        public List<string> AvailableSizes { get; set; } = new List<string>();

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int CurrentStock { get; set; }

        [Required(ErrorMessage = "At least one image is required")]
        [MinLength(1, ErrorMessage = "At least one image is required")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public int? MainImageIndex { get; set; } = 0;

    }
}
