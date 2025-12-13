using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.DTOs.ProductDTOs
{
    public class UpdateDto
    {
        [Required]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }


        public string? Brand { get; set; }
        public int? CategoryId { get; set; }
        public string? SpecialOffer { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int? CurrentStock { get; set; }

        public bool? IsActive { get; set; }

        public List<string>? AvailableSizes { get; set; } = new List<string>();
        public List<IFormFile>? NewImages { get; set; }
    }
}
