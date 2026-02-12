using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btsx
{
    /// <summary>
    /// Represents contact data mapped from vCard version 4.0 (RFC 6350).
    /// </summary>
    public interface IContactData
    {
        // Identification Properties
        /// <summary>
        /// Formatted name (FN) - The complete formatted name of the contact.
        /// </summary>
        string? FormattedName { get; set; }

        /// <summary>
        /// Family/surname (N - surname component).
        /// </summary>
        string? FamilyName { get; set; }

        /// <summary>
        /// Given/first name (N - given name component).
        /// </summary>
        string? GivenName { get; set; }

        /// <summary>
        /// Additional/middle names (N - additional names component).
        /// </summary>
        string? AdditionalNames { get; set; }

        /// <summary>
        /// Honorific prefixes, e.g., "Dr.", "Mr." (N - prefixes component).
        /// </summary>
        string? HonorificPrefixes { get; set; }

        /// <summary>
        /// Honorific suffixes, e.g., "Jr.", "PhD" (N - suffixes component).
        /// </summary>
        string? HonorificSuffixes { get; set; }

        /// <summary>
        /// Nickname(s) (NICKNAME).
        /// </summary>
        string? Nickname { get; set; }

        /// <summary>
        /// Unique identifier (UID).
        /// </summary>
        string? UniqueIdentifier { get; set; }

        // Delivery Addressing Properties
        /// <summary>
        /// Email addresses (EMAIL).
        /// </summary>
        List<string>? EmailAddresses { get; set; }

        /// <summary>
        /// Telephone numbers (TEL).
        /// </summary>
        List<string>? PhoneNumbers { get; set; }

        /// <summary>
        /// Postal addresses (ADR).
        /// </summary>
        List<string>? Addresses { get; set; }

        // Organizational Properties
        /// <summary>
        /// Organization name (ORG).
        /// </summary>
        string? Organization { get; set; }

        /// <summary>
        /// Job title or position (TITLE).
        /// </summary>
        string? Title { get; set; }

        /// <summary>
        /// Role or occupation (ROLE).
        /// </summary>
        string? Role { get; set; }

        // Personal Properties
        /// <summary>
        /// Birthday (BDAY).
        /// </summary>
        DateTime? Birthday { get; set; }

        /// <summary>
        /// Anniversary date (ANNIVERSARY).
        /// </summary>
        DateTime? Anniversary { get; set; }

        /// <summary>
        /// Gender (GENDER).
        /// </summary>
        string? Gender { get; set; }

        // Communications Properties
        /// <summary>
        /// Instant messaging addresses (IMPP).
        /// </summary>
        List<string>? InstantMessagingAddresses { get; set; }

        /// <summary>
        /// Preferred language(s) (LANG).
        /// </summary>
        string? Language { get; set; }

        // Geographical Properties
        /// <summary>
        /// Time zone (TZ).
        /// </summary>
        string? TimeZone { get; set; }

        /// <summary>
        /// Geographic position - latitude and longitude (GEO).
        /// </summary>
        string? GeographicPosition { get; set; }

        // Explanatory Properties
        /// <summary>
        /// Categories or tags (CATEGORIES).
        /// </summary>
        List<string>? Categories { get; set; }

        /// <summary>
        /// Notes or additional information (NOTE).
        /// </summary>
        string? Notes { get; set; }

        /// <summary>
        /// Product identifier (PRODID).
        /// </summary>
        string? ProductId { get; set; }

        /// <summary>
        /// Revision/last modified timestamp (REV).
        /// </summary>
        DateTime? Revision { get; set; }

        // Security and URL Properties
        /// <summary>
        /// URLs associated with the contact (URL).
        /// </summary>
        List<string>? Urls { get; set; }

        /// <summary>
        /// Public key or authentication certificate (KEY).
        /// </summary>
        string? PublicKey { get; set; }

        // Media Properties
        /// <summary>
        /// Photo or avatar (PHOTO) - could be URL or base64 encoded data.
        /// </summary>
        string? Photo { get; set; }

        /// <summary>
        /// Logo (LOGO) - organization logo.
        /// </summary>
        string? Logo { get; set; }

        /// <summary>
        /// Sound or pronunciation (SOUND).
        /// </summary>
        string? Sound { get; set; }

        // Calendar Properties
        /// <summary>
        /// Calendar address URI (CALADRURI).
        /// </summary>
        string? CalendarAddressUri { get; set; }

        /// <summary>
        /// Calendar URI (CALURI).
        /// </summary>
        string? CalendarUri { get; set; }

        /// <summary>
        /// Free/busy URL (FBURL).
        /// </summary>
        string? FreeBusyUrl { get; set; }

        // Relationship Properties
        /// <summary>
        /// Related contacts or relationships (RELATED).
        /// </summary>
        List<string>? RelatedContacts { get; set; }

    }
}
