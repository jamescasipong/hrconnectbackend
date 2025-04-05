using System.Security.Cryptography;
using System.Text;

namespace hrconnectbackend.Helper;

public static class Generator
{
    private static readonly string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private static readonly string Digits = "0123456789";
    private static readonly string SpecialChars = "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?";

    // Function to generate a random password   
    public static string GeneratePassword(int length = 12, bool includeSpecialChars = true)
    {
        // Validate length
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters.");

        // Define the characters pool
        string charPool = UppercaseChars + LowercaseChars + Digits;
        if (includeSpecialChars)
        {
            charPool += SpecialChars;
        }

        // Generate random password
        var randomPassword = new char[length];
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] randomBytes = new byte[length];
            rng.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
            {
                int index = randomBytes[i] % charPool.Length;
                randomPassword[i] = charPool[index];
            }
        }

        return new string(randomPassword);
    }

    public static string GenerateUsername(int length = 8)
    {
        // Define allowed characters in the username (letters and digits)
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        // Create a StringBuilder to construct the username
        StringBuilder username = new StringBuilder();

        Random random = new Random();

        // Generate a random username
        for (int i = 0; i < length; i++)
        {
            int index = random.Next(validChars.Length);
            username.Append(validChars[index]);
        }

        return username.ToString();
    }
}