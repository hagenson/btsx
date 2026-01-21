using BtsxWeb.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Provide encryption and decryption operations for the application.
    /// </summary>
    public class EncryptionService
    {
        /// <summary>
        /// Initialises the service.
        /// </summary>
        public EncryptionService(IOptions<PersistenceSettings> persistenceSettings, ILogger<EncryptionService> logger)
        {
            this.logger = logger;
            var keyString = persistenceSettings.Value.EncryptionKey;

            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("Persistence:EncryptionKey is not set in appsettings.json. Please set it to a 32-byte base64 encoded string.");
            }

            try
            {
                encryptionKey = Convert.FromBase64String(keyString);
                if (encryptionKey.Length != 32)
                {
                    throw new InvalidOperationException("Persistence:EncryptionKey must be a 32-byte (256-bit) key encoded as base64.");
                }
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Persistence:EncryptionKey must be a valid base64 encoded string.");
            }
        }

        /// <summary>
        /// Decrypts a previously encrypted value.
        /// </summary>
        public string? Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = encryptionKey;

                var iv = new byte[aes.IV.Length];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to decrypt data");
                throw;
            }
        }

        /// <summary>
        /// Encrypts a supplied string.
        /// </summary>
        public string? Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            try
            {
                using var aes = Aes.Create();
                aes.Key = encryptionKey;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new MemoryStream();
                msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to encrypt data");
                throw;
            }
        }

        private readonly byte[] encryptionKey;
        private readonly ILogger<EncryptionService> logger;
    }
}