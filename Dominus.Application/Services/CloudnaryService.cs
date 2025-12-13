using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dominus.Application.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            try
            {
                var cloudName = configuration["Cloudinary:CloudName"];
                var apiKey = configuration["Cloudinary:ApiKey"];
                var apiSecret = configuration["Cloudinary:ApiSecret"];

                if (string.IsNullOrEmpty(cloudName) ||
                    string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(apiSecret))
                {
                    _logger.LogError("Cloudinary configuration is missing. Please check appsettings.json");
                    throw new InvalidOperationException("Cloudinary configuration is missing. Please check appsettings.json");
                }

                // Configure Cloudinary with proper HTTP client timeout settings
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
                
                // Configure HTTP client timeout to handle large uploads (5 minutes)
                // Note: Cloudinary SDK manages its own HttpClient, so we configure timeout via Api
                _cloudinary.Api.Timeout = (int)TimeSpan.FromMinutes(5).TotalSeconds;
                
                _logger.LogInformation("CloudinaryService initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize CloudinaryService");

                throw;
            }
        }

        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                return null;

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "ecommerce_products"
            };


            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            var result = await _cloudinary.UploadAsync(uploadParams, cts.Token);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return result.SecureUrl?.ToString();
        }

    }
}
