//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
//using Dominus.Domain.Interfaces;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Threading.Tasks;

//namespace Dominus.Infrastructure.Cloudinary
//{
//    public class CloudinaryService : IImageStorageService
//    {
//        private readonly CloudinaryDotNet.Cloudinary _client;
//        private readonly ILogger<CloudinaryService> _logger;
//        private readonly CloudinarySettings _settings;

//        public CloudinaryService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryService> logger)
//        {
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            var cfg = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

//            // Normalize and validate configuration early
//            cfg.CloudName = (cfg.CloudName ?? string.Empty).Trim();
//            cfg.ApiKey = (cfg.ApiKey ?? string.Empty).Trim();
//            cfg.ApiSecret = (cfg.ApiSecret ?? string.Empty).Trim();

//            if (string.IsNullOrWhiteSpace(cfg.CloudName) ||
//                string.IsNullOrWhiteSpace(cfg.ApiKey) ||
//                string.IsNullOrWhiteSpace(cfg.ApiSecret))
//            {
//                _logger.LogError("Cloudinary configuration is incomplete. CloudName set: {HasCloudName}, ApiKey set: {HasApiKey}",
//                    !string.IsNullOrWhiteSpace(cfg.CloudName),
//                    !string.IsNullOrWhiteSpace(cfg.ApiKey));
//                throw new InvalidOperationException("Cloudinary configuration is missing. Ensure CloudName, ApiKey and ApiSecret are configured.");
//            }

//            _settings = cfg;

//            // Masked values for diagnostics only
//            _logger.LogInformation("Initializing Cloudinary client for cloud '{CloudName}' with ApiKey '{ApiKeyMasked}'",
//                cfg.CloudName, Mask(cfg.ApiKey));

//            var account = new Account(cfg.CloudName, cfg.ApiKey, cfg.ApiSecret);
//            _client = new CloudinaryDotNet.Cloudinary(account);
//            _client.Api.Secure = true;

//            // Warn if system time looks suspicious (signature uses timestamp)
//            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//            _logger.LogDebug("Cloudinary Service started (unix time {Timestamp}).", now);
//        }

//        public async Task<string> UploadImageAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                throw new ArgumentException("File is null or empty", nameof(file));

//            try
//            {
//                using var stream = file.OpenReadStream();
//                var uploadParams = new ImageUploadParams
//                {
//                    File = new FileDescription(file.FileName, stream),
//                    Folder = "products",
//                    UseFilename = true,
//                    UniqueFilename = false,
//                    Overwrite = false
//                };

//                // Diagnostic log (do not include secrets)
//                _logger.LogDebug("Uploading file {FileName} to Cloudinary '{CloudName}'. Folder={Folder}, UseFilename={UseFilename}, UniqueFilename={UniqueFilename}, Overwrite={Overwrite}",
//                    file.FileName, _settings.CloudName, uploadParams.Folder, uploadParams.UseFilename, uploadParams.UniqueFilename, uploadParams.Overwrite);

//                var uploadResult = await _client.UploadAsync(uploadParams);

//                if (uploadResult == null)
//                {
//                    _logger.LogError("Cloudinary upload returned null result for file {FileName}", file.FileName);
//                    throw new InvalidOperationException("Cloudinary upload returned null result.");
//                }

//                // Log and surface Cloudinary error details when upload fails
//                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK &&
//                    uploadResult.StatusCode != System.Net.HttpStatusCode.Created)
//                {
//                    var errMsg = uploadResult.Error?.Message ?? "Unknown error";
//                    _logger.LogError(
//                        "Cloudinary upload failed. Status: {Status}, Error: {Error}, PublicId: {PublicId}",
//                        uploadResult.StatusCode,
//                        errMsg,
//                        uploadResult.PublicId);

//                    throw new InvalidOperationException(
//                        $"Cloudinary upload failed: Status={uploadResult.StatusCode}; Error={errMsg}; PublicId={uploadResult.PublicId}");
//                }

//                return uploadResult.SecureUrl?.ToString() ?? string.Empty;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Exception during Cloudinary upload for file {FileName}: {Message}", file.FileName, ex.Message);
//                throw new InvalidOperationException($"Failed to upload image to Cloudinary: {ex.Message}", ex);
//            }
//        }

//        private static string Mask(string value)
//        {
//            if (string.IsNullOrEmpty(value))
//                return string.Empty;
//            if (value.Length <= 8)
//                return new string('*', value.Length);
//            return value.Substring(0, 4) + new string('*', value.Length - 8) + value.Substring(value.Length - 4);
//        }
//    }
//}