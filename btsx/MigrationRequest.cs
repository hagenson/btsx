using System.Text.Json.Serialization;

namespace Btsx
{
    /// <summary>
    /// Encapsulates the parameters required to create a data migration job.
    /// </summary>
    [JsonDerivedType(typeof(MailMigrationRequest), typeDiscriminator: "Mail")]
    [JsonDerivedType(typeof(ContactMigrationRequest), typeDiscriminator: "Contact")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    public class MigrationRequest
    {
        /// <summary>
        /// Credentials for connecting to the destination service.
        /// </summary>
        public Creds? DestinationCredentials { get; set; }

        /// <summary>
        /// True to include progress in status updates.
        /// </summary>
        public bool ProgressUpdates { get; set; }

        /// <summary>
        /// Credentials for connecting to the source service.
        /// </summary>
        public Creds? SourceCredentials { get; set; }

        /// <summary>
        /// Called after an incomplete job is loaded from persistent storage after a
        /// service restart.
        /// </summary>
        /// <remarks>
        /// Subclasses should override this to implement any state changes that are required
        /// when a job is restarted.
        /// </remarks>
        public virtual void Restarting()
        {
        }
    }

    public class MigrationRequest<TOptions> : MigrationRequest where TOptions : class
    {
        public TOptions? Options { get; set; }
    }
}