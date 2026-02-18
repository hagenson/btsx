namespace Btsx
{
    /// <summary>
    /// Encapsulates options for moving contacts from one account to another.
    /// </summary>
    public class ContactMoverOptions
    {
        /// <summary>
        /// Gets or sets how duplicate contacts should be handled during the operation.
        /// </summary>
        public DuplicateHandling DuplicateHandling { get; set; } = DuplicateHandling.Skip;

        /// <summary>
        /// Gets or sets a value indicating whether source contacts should be deleted after they are migrated.
        /// </summary>
        public bool DeleteSource { get; set; }

        /// <summary>
        /// For imported contacts, this will be used to generate the folder name in the destination account.
        /// </summary>
        /// <remarks>
        /// If the source contact is not in a group, the contact will be placed in a group with this name.
        /// If the source contact is in a group, the contact will be placed in a group with this name as
        /// a prefix followed by the source group name with the format "&lt;ImportFolderName&gt; - &lt;Source Folder Name&gt;".
        /// </remarks>
        public string ImportFolderName { get; set; } = "";

        /// <summary>
        /// When true, collected addresses from the source account will be imported to the destination account.
        /// </summary>
        /// <remarks>
        /// Some mail providers "collect" contacts from email address that users send message to and receive from.
        /// If this option is true these contacts will be imported into a folder named "Collected Contacts".
        /// If the ImportFolderName property is set, the folder will be named "&lt;ImportFolderName&gt; - Collected Contacts".
        /// </remarks>
        public bool ImportCollectedContacts { get; set; }
    }
}