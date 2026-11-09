namespace RaceDay.Api.Services
{
    public static class ImageRules
    {
        private static readonly string[] AllowedTypes =
            { "image/jpeg", "image/png", "image/gif", "image/webp" };

        public const long MaxBytes = 5 * 1024 * 1024;

        /// <summary>Returns an error message, or null when the file is an acceptable image.</summary>
        public static string? Check(IFormFile? file)
        {
            if (file == null || file.Length == 0) return "Please choose an image file to upload.";
            if (file.Length > MaxBytes) return "The image must be 5 MB or smaller.";

            var type = (file.ContentType ?? string.Empty).ToLowerInvariant();
            if (!AllowedTypes.Contains(type)) return "Only JPEG, PNG, GIF or WebP images are allowed.";

            return null;
        }
    }
}