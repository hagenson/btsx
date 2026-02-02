using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace Btsx
{
    /// <summary>
    /// Service for uploading contacts to NextCloud using CardDAV protocol.
    /// </summary>
    public class NextCloudContactsService
    {
        private readonly HttpClient httpClient;
        private readonly string username;
        private readonly string password;
        private readonly string baseUrl;

        /// <summary>
        /// Initializes a new instance of the NextCloudContactsService.
        /// </summary>
        /// <param name="serverUrl">NextCloud server URL (e.g., https://cloud.example.com).</param>
        /// <param name="username">NextCloud username.</param>
        /// <param name="password">NextCloud password or app password.</param>
        public NextCloudContactsService(string serverUrl, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
                throw new ArgumentException("Server URL cannot be null or empty", nameof(serverUrl));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            this.username = username;
            this.password = password;
            this.baseUrl = $"{serverUrl.TrimEnd('/')}/remote.php/dav/addressbooks/users/{username}/contacts/";

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
        public async Task<bool> UploadContactAsync(string vcard, string filename, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(vcard))
                throw new ArgumentException("vCard cannot be null or empty", nameof(vcard));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

            if (!filename.EndsWith(".vcf", StringComparison.OrdinalIgnoreCase))
                filename += ".vcf";

            var url = $"{baseUrl}{filename}";

            try
            {
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
        /// Uploads multiple contacts to NextCloud with progress reporting.
        /// </summary>
        /// <param name="contacts">List of contacts with vCard data.</param>
        /// <param name="progressCallback">Callback for progress updates (current count, status message).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Upload result with success/failure counts.</returns>
        public async Task<UploadResult> UploadContactsAsync(
            IEnumerable<ContactData> contacts,
            Action<int, int, string>? progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            if (contacts == null)
                throw new ArgumentNullException(nameof(contacts));

            var result = new UploadResult();
            var contactList = contacts.ToList();
            result.Total = contactList.Count;

            progressCallback?.Invoke(0, result.Total, "Starting contact upload...");

            foreach (var contact in contactList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (string.IsNullOrWhiteSpace(contact.VCard))
                {
                    result.Failed++;
                    result.FailedContacts.Add((contact.ResourceName ?? "unknown", "Empty vCard"));
                    continue;
                }

                var filename = GenerateFilename(contact);
                var success = await UploadContactAsync(contact.VCard, filename, cancellationToken);

                if (success)
                {
                    result.Successful++;
                    progressCallback?.Invoke(result.Successful, result.Total, $"Uploaded {filename}");
                }
                else
                {
                    result.Failed++;
                    result.FailedContacts.Add((contact.ResourceName ?? filename, "Upload failed"));
                    progressCallback?.Invoke(result.Successful, result.Total, $"Failed to upload {filename}");
                }
            }

            progressCallback?.Invoke(result.Successful, result.Total, 
                $"Upload complete: {result.Successful} successful, {result.Failed} failed");

            return result;
        }

        /// <summary>
        /// Checks if a contact already exists on the server using PROPFIND.
        /// </summary>
        /// <param name="filename">Filename to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the contact exists.</returns>
        public async Task<bool> ContactExistsAsync(string filename, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

            if (!filename.EndsWith(".vcf", StringComparison.OrdinalIgnoreCase))
                filename += ".vcf";

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
        /// Lists all contacts in the addressbook using PROPFIND.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of contact filenames.</returns>
        public async Task<List<string>> ListContactsAsync(CancellationToken cancellationToken = default)
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

            return contacts;
        }

        /// <summary>
        /// Deletes a contact from the server.
        /// </summary>
        /// <param name="filename">Filename of the contact to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deletion was successful.</returns>
        public async Task<bool> DeleteContactAsync(string filename, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

            if (!filename.EndsWith(".vcf", StringComparison.OrdinalIgnoreCase))
                filename += ".vcf";

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
        /// Generates a unique filename for a contact.
        /// </summary>
        /// <param name="contact">Contact data.</param>
        /// <returns>Filename string.</returns>
        private string GenerateFilename(ContactData contact)
        {
            var id = contact.ResourceName?.Replace("/", "-").Replace("\\", "-") ?? Guid.NewGuid().ToString();
            
            if (contact.Person?.Names != null && contact.Person.Names.Count > 0)
            {
                var name = contact.Person.Names[0];
                var displayName = name.DisplayName ?? 
                    $"{name.GivenName} {name.FamilyName}".Trim();
                
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    var safeName = string.Join("", displayName
                        .Replace(" ", "-")
                        .Where(c => char.IsLetterOrDigit(c) || c == '-'))
                        .ToLowerInvariant();
                    
                    if (!string.IsNullOrEmpty(safeName))
                    {
                        id = $"{safeName}-{Guid.NewGuid():N}";
                    }
                }
            }

            return $"{id}.vcf";
        }

        /// <summary>
        /// Disposes the HttpClient if it was created by this instance.
        /// </summary>
        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Represents the result of a contact upload operation.
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// Total number of contacts processed.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Number of contacts successfully uploaded.
        /// </summary>
        public int Successful { get; set; }

        /// <summary>
        /// Number of contacts that failed to upload.
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// List of failed contacts with error messages.
        /// </summary>
        public List<(string ContactId, string Error)> FailedContacts { get; set; } = new();
    }
}
