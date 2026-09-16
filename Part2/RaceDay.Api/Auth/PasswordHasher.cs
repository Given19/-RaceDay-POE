using System.Security.Cryptography;

namespace RaceDay.Api.Auth
{
    /// <summary>
    /// Hashes and verifies passwords using PBKDF2 (Rfc2898DeriveBytes).
    /// Uses only the .NET base class library - no external package required.
    /// Format stored: {iterations}.{base64 salt}.{base64 hash}
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;       // 128-bit salt
        private const int KeySize = 32;        // 256-bit derived key
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.', 3);
            if (parts.Length != 3) return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedKey = Convert.FromBase64String(parts[2]);

            byte[] actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);

            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
    }
}