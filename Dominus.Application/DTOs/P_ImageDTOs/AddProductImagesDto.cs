using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.P_ImageDTOs
{
    public class AddProductImagesDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public List<IFormFile> Images { get; set; } = new();
    }

}
