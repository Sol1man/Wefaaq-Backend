using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using Wefaaq.Bll.Interfaces;

namespace Wefaaq.Bll.Services;

/// <summary>
/// Service for encrypting and decrypting passwords using AES encryption
/// </summary>
public class PasswordEncryptionService : IPasswordEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public PasswordEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key not found in configuration");
        var ivBase64 = configuration["Encryption:IV"]
            ?? throw new InvalidOperationException("Encryption:IV not found in configuration");

        _key = Convert.FromBase64String(keyBase64);
        _iv = Convert.FromBase64String(ivBase64);

        // Validate key and IV sizes
        if (_key.Length != 32) // AES-256 requires 32-byte key
            throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits)");
        if (_iv.Length != 16) // AES requires 16-byte IV
            throw new InvalidOperationException("Encryption IV must be 16 bytes (128 bits)");
    }

    /// <summary>
    /// Encrypts a plain text password using AES-256
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using (var aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    /// <summary>
    /// Decrypts an encrypted password using AES-256
    /// </summary>
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return encryptedText;

        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var cipherBytes = Convert.FromBase64String(encryptedText);

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(cipherBytes))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            // If decryption fails, it might be plain text (migration scenario)
            // Log the error and return the original text
            Console.WriteLine($"Decryption failed, returning as-is: {ex.Message}");
            return encryptedText;
        }
    }
}
