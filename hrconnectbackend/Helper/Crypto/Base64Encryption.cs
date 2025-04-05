using System;

namespace hrconnectbackend.Helper.Crypto;

public class Base64Encryption
{
    // Encode string to Base64
    public static string EncodeToBase64(string plainText)
    {
        // Convert string to byte array
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        
        // Convert byte array to Base64 string
        return Convert.ToBase64String(bytes);
    }

    // Decode from Base64 to string
    public static string DecodeFromBase64(string base64Encoded)
    {
        // Convert Base64 string back to byte array
        byte[] bytes = Convert.FromBase64String(base64Encoded);
        
        // Convert byte array back to string
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}