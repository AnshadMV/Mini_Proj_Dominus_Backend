using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Interfaces
{
    public interface IImageStorageService
    {
        Task<(string url, string publicId)> UploadAsync(IFormFile file);
        Task<bool> DeleteAsync(string publicId);
    }

}
