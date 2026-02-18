
using System.Text.Json.Serialization;

namespace Btsx
{
    /// <summary>
    /// Encapsulates the parameters required to create mail migration job.
    /// </summary>
    [JsonDerivedType(typeof(MailMigrationRequest), typeDiscriminator: "Mail")]
    [JsonDerivedType(typeof(ContactMigrationRequest), typeDiscriminator: "Contact")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    public class MigrationRequest
    {
        /// <summary>
        /// True to include progress in status updates.
        /// </summary>
        public bool ProgressUpdates { get; set; }

        public Creds? SourceCredentials { get; set; }

        public Creds? DestinationCredentials { get; set; }

        public virtual void Restarting()
        {            
        }
    }

    public class MigrationRequest<TOptions>: MigrationRequest where TOptions : class
    {

        public TOptions? Options { get; set; }

    }
}