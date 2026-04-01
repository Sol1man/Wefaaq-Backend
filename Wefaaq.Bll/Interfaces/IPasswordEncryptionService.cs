namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Interface for password encryption and decryption service
/// </summary>
public interface IPasswordEncryptionService
{
    /// <summary>
    /// Encrypts a plain text password
    /// </summary>
    /// <param name="plainText">The plain text password to encrypt</param>
    /// <returns>The encrypted password as a Base64 string</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted password
    /// </summary>
    /// <param name="encryptedText">The encrypted password as a Base64 string</param>
    /// <returns>The decrypted plain text password</returns>
    string Decrypt(string encryptedText);
}
