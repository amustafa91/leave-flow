using System.Security.Cryptography;
using System.Text;
using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Infrastructure.Services;

/// <summary>
/// AES-256-GCM encryption service for sensitive data protection.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private const int NonceSize = 12; // 96 bits for GCM
    private const int TagSize = 16;   // 128 bits for authentication tag

    public EncryptionService(string encryptionKey)
    {
        // Derive a 256-bit key from the provided key using SHA256
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Combine nonce + ciphertext + tag
        var result = new byte[NonceSize + cipherBytes.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSize + cipherBytes.Length, TagSize);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipherBytes = new byte[fullCipher.Length - NonceSize - TagSize];

        Buffer.BlockCopy(fullCipher, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(fullCipher, NonceSize, cipherBytes, 0, cipherBytes.Length);
        Buffer.BlockCopy(fullCipher, NonceSize + cipherBytes.Length, tag, 0, TagSize);

        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
