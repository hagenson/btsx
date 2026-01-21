namespace BtsxWeb.Models
{
    /// <summary>
    /// Encapsulates configuration settings for the job persistence service.
    /// </summary>
    public class PersistenceSettings
    {
        /// <summary>
        /// Base64 encoded 256 bit encryption key.
        /// </summary>
        public string EncryptionKey { get; set; } = "";


        /// <summary>
        /// Directory where jobs will be persisted.
        /// </summary>
        public string StorageDirectory { get; set; } = "";
    }
}