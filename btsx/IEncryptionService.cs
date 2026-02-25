namespace Btsx
{
    /// <summary>
    /// Defines operations to encrypt and decrypt data.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Decrypts a string previously encrypted with a call to <see cref="Encrypt(string?)"/>.
        /// </summary>
        /// <param name="cipherText">Encrypted string to decrypt.</param>
        /// <returns>Original decrypted string.</returns>
        string? Decrypt(string? cipherText);

        /// <summary>
        /// Encrypts a plain text string.
        /// </summary>
        /// <param name="plainText">Text to encrypt.</param>
        /// <returns>Encrypted string.</returns>
        string? Encrypt(string? plainText);
    }
}