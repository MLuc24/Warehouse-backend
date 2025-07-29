using System.Security.Cryptography;
using System.Text;

namespace WarehouseManage.Helpers;

public static class PasswordHelper
{
    private const int SALT_SIZE = 16;
    private const int HASH_SIZE = 20;
    private const int ITERATIONS = 10000;

    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty");

        // Generate salt
        byte[] salt = new byte[SALT_SIZE];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash password with salt
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HASH_SIZE);

        // Combine salt and hash
        byte[] hashBytes = new byte[SALT_SIZE + HASH_SIZE];
        Array.Copy(salt, 0, hashBytes, 0, SALT_SIZE);
        Array.Copy(hash, 0, hashBytes, SALT_SIZE, HASH_SIZE);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        try
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            
            if (hashBytes.Length != SALT_SIZE + HASH_SIZE)
                return false;

            // Extract salt
            byte[] salt = new byte[SALT_SIZE];
            Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);

            // Hash the input password with the same salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HASH_SIZE);

            // Compare hashes
            for (int i = 0; i < HASH_SIZE; i++)
            {
                if (hashBytes[i + SALT_SIZE] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
