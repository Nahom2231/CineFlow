using Application.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private readonly string _key;

    public EncryptionService(
        IOptions<EncryptionOptions> options)
    {
        _key = options.Value.Key;
    }

    public string Encrypt(string plainText)
    {
        // Encryption implementation
        return plainText;
    }

    public string Decrypt(string cipherText)
    {
        // Decryption implementation
        return cipherText;
    }
}