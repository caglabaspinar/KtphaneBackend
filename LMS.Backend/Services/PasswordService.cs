using System.Security.Cryptography;

namespace LMS.Backend.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }

    public class PasswordService : IPasswordService
    {
        
        private const int SaltSize = 16;      
        private const int KeySize = 32;      
        private const int Iterations = 100_000;

        public string HashPassword(string password)
        {
            
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

           
            var key = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: KeySize
            );

           
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
           
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out var iterations)) return false;

            byte[] salt;
            byte[] key;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                key = Convert.FromBase64String(parts[2]);
            }
            catch
            {
                return false;
            }

            var providedKey = Rfc2898DeriveBytes.Pbkdf2(
                password: providedPassword,
                salt: salt,
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: key.Length
            );

            return CryptographicOperations.FixedTimeEquals(providedKey, key);
        }
    }
}
