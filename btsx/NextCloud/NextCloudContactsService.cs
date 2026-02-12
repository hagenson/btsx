using Btsx.Google;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Btsx.NextCloud
{
    /// <summary>
    /// Service for uploading contacts to NextCloud using CardDAV protocol.
    /// </summary>
    public class NextCloudContactsService : IContactService
    {
        /// <summary>
        /// Initializes a new instance of the NextCloudContactsService.
        /// </summary>
        public NextCloudContactsService(Creds creds)
        {
            if (string.IsNullOrWhiteSpace(creds.Server))
                throw new ArgumentException("Server URL cannot be null or empty", nameof(creds.Server));
            if (string.IsNullOrWhiteSpace(creds.User))
                throw new ArgumentException("Username cannot be null or empty", nameof(creds.User));
            if (string.IsNullOrWhiteSpace(creds.Password))
                throw new ArgumentException("Password cannot be null or empty", nameof(creds.Password));

            this.username = creds.User;
            this.password = creds.Password;
            this.baseUrl = $"{creds.Server.TrimEnd('/')}/remote.php/dav/addressbooks/users/{username}/contacts/";

            httpClient = new HttpClient();
            var authBytes = Encoding.UTF8.GetBytes($"{username}:{password}");
            var authHeader = Convert.ToBase64String(authBytes);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        }

        /// <summary>
        /// Initializes a new instance with a custom HttpClient.
        /// </summary>
        /// <param name="httpClient">Custom HttpClient instance.</param>
        /// <param name="serverUrl">NextCloud server URL.</param>
        /// <param name="username">NextCloud username.</param>
        /// <param name="password">NextCloud password or app password.</param>
        public NextCloudContactsService(HttpClient httpClient, string serverUrl, string username, string password)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            if (string.IsNullOrWhiteSpace(serverUrl))
                throw new ArgumentException("Server URL cannot be null or empty", nameof(serverUrl));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            this.username = username;
            this.password = password;
            this.baseUrl = $"{serverUrl.TrimEnd('/')}/remote.php/dav/addressbooks/users/{username}/contacts/";
            this.httpClient = httpClient;

            var authBytes = Encoding.UTF8.GetBytes($"{username}:{password}");
            var authHeader = Convert.ToBase64String(authBytes);
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        }

        /// <summary>
        /// Checks if a contact already exists on the server using PROPFIND.
        /// </summary>
        /// <param name="contact">Contact to check for existence.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the contact exists.</returns>
        public async Task<bool> ContactExistsAsync(IContactData contact, CancellationToken cancellationToken = default)
        {            
            var filename = GenerateFilename(contact);
            var url = $"{baseUrl}{filename}";

            try
            {
                var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), url);
                request.Headers.Add("Depth", "0");
                request.Content = new StringContent(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<d:propfind xmlns:d=\"DAV:\">" +
                    "<d:prop><d:getetag /></d:prop>" +
                    "</d:propfind>",
                    Encoding.UTF8,
                    "application/xml");

                var response = await httpClient.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a contact from the server.
        /// </summary>
        /// <param name="filename">Filename of the contact to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deletion was successful.</returns>
        public async Task<bool> DeleteContactAsync(IContactData contact, CancellationToken cancellationToken = default)
        {
            var filename = GenerateFilename(contact);

            var url = $"{baseUrl}{filename}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                var response = await httpClient.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes the HttpClient if it was created by this instance.
        /// </summary>
        public void Dispose()
        {
            httpClient?.Dispose();
        }

        /// <summary>
        /// Lists all contacts in the addressbook using PROPFIND.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of contact filenames.</returns>
        public async Task<List<IContactData>> ListContactsAsync(CancellationToken cancellationToken = default)
        {
            var contacts = new List<string>();

            try
            {
                var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), baseUrl);
                request.Headers.Add("Depth", "1");
                request.Content = new StringContent(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<d:propfind xmlns:d=\"DAV:\">" +
                    "<d:prop><d:resourcetype /></d:prop>" +
                    "</d:propfind>",
                    Encoding.UTF8,
                    "application/xml");

                var response = await httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var xml = XDocument.Parse(content);
                    XNamespace d = "DAV:";

                    foreach (var responseElement in xml.Descendants(d + "response"))
                    {
                        var href = responseElement.Element(d + "href")?.Value;
                        if (!string.IsNullOrEmpty(href) && href.EndsWith(".vcf", StringComparison.OrdinalIgnoreCase))
                        {
                            var filename = Path.GetFileName(href);
                            contacts.Add(filename);
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Tests the connection to the NextCloud CardDAV server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the connection and authentication are successful.</returns>
        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), baseUrl);
                request.Headers.Add("Depth", "0");
                request.Content = new StringContent(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<d:propfind xmlns:d=\"DAV:\">" +
                    "<d:prop><d:resourcetype /></d:prop>" +
                    "</d:propfind>",
                    Encoding.UTF8,
                    "application/xml");

                var response = await httpClient.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Uploads a single contact to NextCloud in vCard 3.0 format.
        /// </summary>
        /// <param name="vcard">vCard 3.0 formatted string.</param>
        /// <param name="filename">Filename for the contact (e.g., "contact-123.vcf").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if upload was successful.</returns>
        public async Task<bool> UploadContactAsync(IContactData contact, CancellationToken cancellationToken = default)
        {
            string filename = GenerateFilename(contact);


            var url = $"{baseUrl}{filename}";

            try
            {
                string vcard = ConvertToVCard(contact);
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Content = new StringContent(vcard, Encoding.UTF8, "text/vcard");

                var response = await httpClient.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts an IContactData to vCard 4.0 format string.
        /// </summary>
        /// <param name="contact">Contact data to convert.</param>
        /// <returns>vCard 4.0 formatted string.</returns>
        private string ConvertToVCard(IContactData contact)
        {
            var vcard = new StringBuilder();

            // vCard 4.0 header
            vcard.AppendLine("BEGIN:VCARD");
            vcard.AppendLine("VERSION:4.0");

            // UID - Unique identifier (required for vCard 4.0)
            if (!string.IsNullOrWhiteSpace(contact.UniqueIdentifier))
            {
                vcard.AppendLine($"UID:{EscapeVCardValue(contact.UniqueIdentifier)}");
            }
            else
            {
                vcard.AppendLine($"UID:{Guid.NewGuid()}");
            }

            // FN - Formatted name (required)
            if (!string.IsNullOrWhiteSpace(contact.FormattedName))
            {
                vcard.AppendLine($"FN:{EscapeVCardValue(contact.FormattedName)}");
            }
            else
            {
                // Generate formatted name from components
                var name = $"{contact.GivenName} {contact.FamilyName}".Trim();
                vcard.AppendLine($"FN:{EscapeVCardValue(string.IsNullOrWhiteSpace(name) ? "Unknown" : name)}");
            }

            // N - Name components (FamilyName;GivenName;AdditionalNames;HonorificPrefixes;HonorificSuffixes)
            if (!string.IsNullOrWhiteSpace(contact.FamilyName) || !string.IsNullOrWhiteSpace(contact.GivenName))
            {
                vcard.AppendLine($"N:{EscapeVCardValue(contact.FamilyName ?? "")};{EscapeVCardValue(contact.GivenName ?? "")};{EscapeVCardValue(contact.AdditionalNames ?? "")};{EscapeVCardValue(contact.HonorificPrefixes ?? "")};{EscapeVCardValue(contact.HonorificSuffixes ?? "")}");
            }

            // NICKNAME
            if (!string.IsNullOrWhiteSpace(contact.Nickname))
            {
                vcard.AppendLine($"NICKNAME:{EscapeVCardValue(contact.Nickname)}");
            }

            // EMAIL addresses
            if (contact.EmailAddresses != null && contact.EmailAddresses.Any())
            {
                foreach (var email in contact.EmailAddresses)
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        vcard.AppendLine($"EMAIL:{EscapeVCardValue(email)}");
                    }
                }
            }

            // TEL - Phone numbers
            if (contact.PhoneNumbers != null && contact.PhoneNumbers.Any())
            {
                foreach (var phone in contact.PhoneNumbers)
                {
                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        vcard.AppendLine($"TEL:{EscapeVCardValue(phone)}");
                    }
                }
            }

            // ADR - Addresses
            if (contact.Addresses != null && contact.Addresses.Any())
            {
                foreach (var address in contact.Addresses)
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        // In vCard 4.0, ADR format: ;;street;city;region;postalCode;country
                        vcard.AppendLine($"ADR:;;{EscapeVCardValue(address)};;;;");
                    }
                }
            }

            // ORG - Organization
            if (!string.IsNullOrWhiteSpace(contact.Organization))
            {
                vcard.AppendLine($"ORG:{EscapeVCardValue(contact.Organization)}");
            }

            // TITLE - Job title
            if (!string.IsNullOrWhiteSpace(contact.Title))
            {
                vcard.AppendLine($"TITLE:{EscapeVCardValue(contact.Title)}");
            }

            // ROLE - Role/occupation
            if (!string.IsNullOrWhiteSpace(contact.Role))
            {
                vcard.AppendLine($"ROLE:{EscapeVCardValue(contact.Role)}");
            }

            // BDAY - Birthday
            if (contact.Birthday.HasValue)
            {
                vcard.AppendLine($"BDAY:{contact.Birthday.Value:yyyyMMdd}");
            }

            // ANNIVERSARY
            if (contact.Anniversary.HasValue)
            {
                vcard.AppendLine($"ANNIVERSARY:{contact.Anniversary.Value:yyyyMMdd}");
            }

            // GENDER
            if (!string.IsNullOrWhiteSpace(contact.Gender))
            {
                vcard.AppendLine($"GENDER:{EscapeVCardValue(contact.Gender)}");
            }

            // IMPP - Instant messaging addresses
            if (contact.InstantMessagingAddresses != null && contact.InstantMessagingAddresses.Any())
            {
                foreach (var im in contact.InstantMessagingAddresses)
                {
                    if (!string.IsNullOrWhiteSpace(im))
                    {
                        vcard.AppendLine($"IMPP:{EscapeVCardValue(im)}");
                    }
                }
            }

            // LANG - Language
            if (!string.IsNullOrWhiteSpace(contact.Language))
            {
                vcard.AppendLine($"LANG:{EscapeVCardValue(contact.Language)}");
            }

            // TZ - Time zone
            if (!string.IsNullOrWhiteSpace(contact.TimeZone))
            {
                vcard.AppendLine($"TZ:{EscapeVCardValue(contact.TimeZone)}");
            }

            // GEO - Geographic position
            if (!string.IsNullOrWhiteSpace(contact.GeographicPosition))
            {
                vcard.AppendLine($"GEO:{EscapeVCardValue(contact.GeographicPosition)}");
            }

            // CATEGORIES
            if (contact.Categories != null && contact.Categories.Any())
            {
                var categories = string.Join(",", contact.Categories.Select(c => EscapeVCardValue(c)));
                vcard.AppendLine($"CATEGORIES:{categories}");
            }

            // NOTE
            if (!string.IsNullOrWhiteSpace(contact.Notes))
            {
                vcard.AppendLine($"NOTE:{EscapeVCardValue(contact.Notes)}");
            }

            // PRODID
            if (!string.IsNullOrWhiteSpace(contact.ProductId))
            {
                vcard.AppendLine($"PRODID:{EscapeVCardValue(contact.ProductId)}");
            }
            else
            {
                vcard.AppendLine("PRODID:-//BTSX//Contact Mover//EN");
            }

            // REV - Revision timestamp
            if (contact.Revision.HasValue)
            {
                vcard.AppendLine($"REV:{contact.Revision.Value:yyyyMMddTHHmmssZ}");
            }
            else
            {
                vcard.AppendLine($"REV:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            }

            // URL
            if (contact.Urls != null && contact.Urls.Any())
            {
                foreach (var url in contact.Urls)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        vcard.AppendLine($"URL:{EscapeVCardValue(url)}");
                    }
                }
            }

            // KEY - Public key
            if (!string.IsNullOrWhiteSpace(contact.PublicKey))
            {
                vcard.AppendLine($"KEY:{EscapeVCardValue(contact.PublicKey)}");
            }

            // PHOTO
            if (!string.IsNullOrWhiteSpace(contact.Photo))
            {
                // Assume URL format
                vcard.AppendLine($"PHOTO;MEDIATYPE=image/jpeg:{EscapeVCardValue(contact.Photo)}");
            }

            // LOGO
            if (!string.IsNullOrWhiteSpace(contact.Logo))
            {
                vcard.AppendLine($"LOGO:{EscapeVCardValue(contact.Logo)}");
            }

            // SOUND
            if (!string.IsNullOrWhiteSpace(contact.Sound))
            {
                vcard.AppendLine($"SOUND:{EscapeVCardValue(contact.Sound)}");
            }

            // CALADRURI - Calendar address URI
            if (!string.IsNullOrWhiteSpace(contact.CalendarAddressUri))
            {
                vcard.AppendLine($"CALADRURI:{EscapeVCardValue(contact.CalendarAddressUri)}");
            }

            // CALURI - Calendar URI
            if (!string.IsNullOrWhiteSpace(contact.CalendarUri))
            {
                vcard.AppendLine($"CALURI:{EscapeVCardValue(contact.CalendarUri)}");
            }

            // FBURL - Free/busy URL
            if (!string.IsNullOrWhiteSpace(contact.FreeBusyUrl))
            {
                vcard.AppendLine($"FBURL:{EscapeVCardValue(contact.FreeBusyUrl)}");
            }

            // RELATED - Related contacts
            if (contact.RelatedContacts != null && contact.RelatedContacts.Any())
            {
                foreach (var related in contact.RelatedContacts)
                {
                    if (!string.IsNullOrWhiteSpace(related))
                    {
                        vcard.AppendLine($"RELATED:{EscapeVCardValue(related)}");
                    }
                }
            }

            // vCard footer
            vcard.AppendLine("END:VCARD");

            return vcard.ToString();
        }

        /// <summary>
        /// Escapes special characters in vCard values according to RFC 6350.
        /// </summary>
        /// <param name="value">Value to escape.</param>
        /// <returns>Escaped value.</returns>
        private string EscapeVCardValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape backslashes, newlines, commas, and semicolons
            return value
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "")
                .Replace(",", "\\,")
                .Replace(";", "\\;");
        }

        private readonly string baseUrl;
        private readonly HttpClient httpClient;
        private readonly string password;
        private readonly string username;

        /// <summary>
        /// Generates a unique filename for a contact.
        /// </summary>
        /// <param name="contact">Contact data.</param>
        /// <returns>Filename string.</returns>
        private string GenerateFilename(IContactData contact)
        {
            var id = contact.FormattedName
                ?? contact.EmailAddresses?.FirstOrDefault()
                ?? contact.PhoneNumbers?.FirstOrDefault()
                ?? contact.UniqueIdentifier
                ?? Guid.NewGuid().ToString();

            id = Regex.Replace(id, Regex.Escape(new string(Path.GetInvalidFileNameChars())), "-"); 

            return $"{id}.vcf";
        }

        public Task<List<IContactData>> ListCollectedContactsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents the result of a contact upload operation.
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// Number of contacts that failed to upload.
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// List of failed contacts with error messages.
        /// </summary>
        public List<(string ContactId, string Error)> FailedContacts { get; set; } = new();

        /// <summary>
        /// Number of contacts successfully uploaded.
        /// </summary>
        public int Successful { get; set; }

        /// <summary>
        /// Total number of contacts processed.
        /// </summary>
        public int Total { get; set; }
    }
}