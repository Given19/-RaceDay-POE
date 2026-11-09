using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _container;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private bool _containerReady;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["BlobStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
            var containerName = configuration["BlobStorage:ContainerName"] ?? "raceday-images";

            _container = new BlobServiceClient(connectionString).GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            await EnsureContainerAsync();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blob = _container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension}");

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });

            return blob.Uri.ToString();
        }

        private async Task EnsureContainerAsync()
        {
            if (_containerReady) return;

            await _lock.WaitAsync();
            try
            {
                if (!_containerReady)
                {
                    // Public read access so the browser can show the images directly.
                    await _container.CreateIfNotExistsAsync(PublicAccessType.Blob);
                    _containerReady = true;
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}