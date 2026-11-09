using Microsoft.AspNetCore.Http;
using RaceDay.Api.Services;

namespace RaceDay.Api.Tests
{
    public class FakeBlobStorageService : IBlobStorageService
    {
        public Task<string> UploadImageAsync(IFormFile file, string folder) =>
            Task.FromResult(
                $"https://fake-storage.test/{folder}/{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");
    }
}