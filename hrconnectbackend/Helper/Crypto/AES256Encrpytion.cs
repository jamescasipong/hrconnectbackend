using System;
using System.Security.Cryptography;
using System.Text;

public class AES256Encrpytion
{
    private readonly string secretKey;

    public AES256Encrpytion(string key)
    {
        secretKey = key;
    }
    /// <summary>
    /// Generates a key from the given password using SHA-256 hashing algorithm.
    /// The key size is 32 bytes (256 bits) for AES-256 encryption.
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    // Method to generate a key of the correct size (32 bytes for AES-256)
    public byte[] GenerateKeyFromPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Hash the password and return the first 32 bytes (256 bits) for AES-256
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
    /// <summary>
    /// Encrypts the given plain text using AES-256 algorithm.
    /// /// The plain text must be a valid UTF-8 string.
    /// The method generates a key from the password and uses a fixed IV (Initialization Vector).
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    // AES Encryption method
    public string Encrypt(string plainText)
    {
        // Generate key from the password (32 bytes for AES-256)
        byte[] keyBytes = GenerateKeyFromPassword(secretKey);

        // Ensure the key size is 32 bytes for AES-256 (AES-128 would use 16 bytes, AES-192 uses 24 bytes)
        if (keyBytes.Length != 32)
        {
            Array.Resize(ref keyBytes, 32);  // Resize it to 32 bytes (may truncate or pad)
        }

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = new byte[16];  // Initialization vector (IV) should be 16 bytes for AES
            aesAlg.Mode = CipherMode.CBC; // CBC mode for AES encryption

            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                byte[] encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainText), 0, plainText.Length);
                return Convert.ToBase64String(encrypted);  // Return base64 encoded encrypted text
            }
        }
    }
    /// <summary>
    /// Decrypts the given cipher text using AES-256 algorithm.
    /// The cipher text must be a valid Base64 string.
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    // AES Decryption method
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            throw new ArgumentException("The cipherText cannot be null or empty.", nameof(cipherText));
        }

        try
        {
            // Generate key from the password (32 bytes for AES-256)
            byte[] keyBytes = GenerateKeyFromPassword(secretKey);

            // Ensure the key size is 32 bytes for AES-256
            if (keyBytes.Length != 32)
            {
                Array.Resize(ref keyBytes, 32);  // Resize it to 32 bytes (may truncate or pad)
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = new byte[16];  // Initialization vector (IV) should be 16 bytes for AES
                aesAlg.Mode = CipherMode.CBC;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] cipherBytes = Convert.FromBase64String(cipherText); // Base64 to byte array
                    byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decrypted);  // Return the decrypted string
                }
            }
        }
        catch (FormatException ex)
        {
            // Log or handle invalid Base64 string error
            throw new InvalidOperationException("Invalid Base64 string.", ex);
        }
    }
}