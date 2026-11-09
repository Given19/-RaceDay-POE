namespace RaceDay.Api.Services
{
    public interface IBlobStorageService
    {
        /// <summary>Uploads an image to blob storage and returns its public URL.</summary>
        Task<string> UploadImageAsync(IFormFile file, string folder);
    }
}