namespace hrconnectbackend.Services.Clients;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

public class JwtTokenService
{
    private const string SecretKey = "your-256-bit-secret"; // For signing the token
    private const string EncryptionKey = "your-encryption-key"; // For encrypting the token (should be kept secret)

    // Create and sign the token
    public string CreateSignedToken()
    {
        var claims = new[] {
            new Claim("user_id", "123456"),
            new Claim("role", "admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "your-issuer",
            audience: "your-audience",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var signedToken = tokenHandler.WriteToken(token);
        return signedToken;
    }

    // Encrypt the signed token
    public string EncryptToken(string signedToken)
    {
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aesAlg.IV = new byte[16];  // Initialization vector (could be random, but should be the same for decryption)

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            byte[] encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(signedToken), 0, signedToken.Length);

            return Convert.ToBase64String(encrypted); // Return the encrypted token
        }
    }
}
