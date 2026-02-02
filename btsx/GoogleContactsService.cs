using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;

namespace Btsx
{
    /// <summary>
    /// Service for fetching Google contacts using OAuth token authentication.
    /// </summary>
    public class GoogleContactsService
    {
        private readonly string oauthToken;

        /// <summary>
        /// Initializes a new instance of the GoogleContactsService.
        /// </summary>
        /// <param name="oauthToken">OAuth access token for authentication.</param>
        public GoogleContactsService(string oauthToken)
        {
            if (string.IsNullOrWhiteSpace(oauthToken))
                throw new ArgumentException("OAuth token cannot be null or empty", nameof(oauthToken));

            this.oauthToken = oauthToken;
        }

        /// <summary>
        /// Fetches all contacts from Google Contacts API with pagination support.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of contacts with their vCard data.</returns>
        public async Task<List<ContactData>> FetchContactsAsync(CancellationToken cancellationToken = default)
        {
            var contacts = new List<ContactData>();
            
            var credential = GoogleCredential.FromAccessToken(oauthToken);
            
            using (var service = new PeopleServiceService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "BTSX Contact Fetcher"
            }))
            {
                string? pageToken = null;
                int pageNumber = 1;

                do
                {
                    var request = service.People.Connections.List("people/me");
                    request.PersonFields = "names,emailAddresses,phoneNumbers,addresses,organizations,birthdays,metadata";
                    request.PageSize = 1000;
                    request.PageToken = pageToken;

                    var response = await request.ExecuteAsync(cancellationToken);

                    if (response.Connections != null)
                    {
                        foreach (var person in response.Connections)
                        {
                            var contactData = new ContactData
                            {
                                ResourceName = person.ResourceName,
                                Person = person,
                                VCard = GenerateVCard(person)
                            };
                            contacts.Add(contactData);
                        }
                    }

                    pageToken = response.NextPageToken;
                    pageNumber++;

                } while (!string.IsNullOrEmpty(pageToken) && !cancellationToken.IsCancellationRequested);
            }

            return contacts;
        }

        /// <summary>
        /// Fetches contacts with progress reporting.
        /// </summary>
        /// <param name="progressCallback">Callback for progress updates.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of contacts with their vCard data.</returns>
        public async Task<List<ContactData>> FetchContactsAsync(
            Action<int, string>? progressCallback,
            CancellationToken cancellationToken = default)
        {
            var contacts = new List<ContactData>();
            
            var credential = GoogleCredential.FromAccessToken(oauthToken);
            
            using (var service = new PeopleServiceService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "BTSX Contact Fetcher"
            }))
            {
                string? pageToken = null;
                int pageNumber = 1;

                do
                {
                    progressCallback?.Invoke(contacts.Count, $"Fetching contacts page {pageNumber}...");

                    var request = service.People.Connections.List("people/me");
                    request.PersonFields = "names,emailAddresses,phoneNumbers,addresses,organizations,birthdays,metadata";
                    request.PageSize = 1000;
                    request.PageToken = pageToken;

                    var response = await request.ExecuteAsync(cancellationToken);

                    if (response.Connections != null)
                    {
                        foreach (var person in response.Connections)
                        {
                            var contactData = new ContactData
                            {
                                ResourceName = person.ResourceName,
                                Person = person,
                                VCard = GenerateVCard(person)
                            };
                            contacts.Add(contactData);
                        }
                    }

                    pageToken = response.NextPageToken;
                    pageNumber++;

                } while (!string.IsNullOrEmpty(pageToken) && !cancellationToken.IsCancellationRequested);

                progressCallback?.Invoke(contacts.Count, $"Completed fetching {contacts.Count} contacts");
            }

            return contacts;
        }

        /// <summary>
        /// Generates a vCard (version 3.0) from a Google Person object.
        /// </summary>
        /// <param name="person">Person object from Google People API.</param>
        /// <returns>vCard formatted string.</returns>
        private string GenerateVCard(Person person)
        {
            var vcard = new System.Text.StringBuilder();
            vcard.AppendLine("BEGIN:VCARD");
            vcard.AppendLine("VERSION:3.0");

            if (person.Names != null && person.Names.Count > 0)
            {
                var name = person.Names[0];
                var familyName = name.FamilyName ?? string.Empty;
                var givenName = name.GivenName ?? string.Empty;
                var middleName = name.MiddleName ?? string.Empty;
                var honorificPrefix = name.HonorificPrefix ?? string.Empty;
                var honorificSuffix = name.HonorificSuffix ?? string.Empty;

                vcard.AppendLine($"N:{EscapeVCardValue(familyName)};{EscapeVCardValue(givenName)};{EscapeVCardValue(middleName)};{EscapeVCardValue(honorificPrefix)};{EscapeVCardValue(honorificSuffix)}");
                
                if (!string.IsNullOrEmpty(name.DisplayName))
                {
                    vcard.AppendLine($"FN:{EscapeVCardValue(name.DisplayName)}");
                }
            }

            if (person.EmailAddresses != null)
            {
                foreach (var email in person.EmailAddresses)
                {
                    if (!string.IsNullOrEmpty(email.Value))
                    {
                        var type = email.Type ?? "OTHER";
                        vcard.AppendLine($"EMAIL;TYPE={type.ToUpperInvariant()}:{EscapeVCardValue(email.Value)}");
                    }
                }
            }

            if (person.PhoneNumbers != null)
            {
                foreach (var phone in person.PhoneNumbers)
                {
                    if (!string.IsNullOrEmpty(phone.Value))
                    {
                        var type = phone.Type ?? "OTHER";
                        vcard.AppendLine($"TEL;TYPE={type.ToUpperInvariant()}:{EscapeVCardValue(phone.Value)}");
                    }
                }
            }

            if (person.Addresses != null)
            {
                foreach (var address in person.Addresses)
                {
                    var type = address.Type ?? "OTHER";
                    var poBox = address.PoBox ?? string.Empty;
                    var extendedAddress = address.ExtendedAddress ?? string.Empty;
                    var streetAddress = address.StreetAddress ?? string.Empty;
                    var city = address.City ?? string.Empty;
                    var region = address.Region ?? string.Empty;
                    var postalCode = address.PostalCode ?? string.Empty;
                    var country = address.Country ?? string.Empty;

                    vcard.AppendLine($"ADR;TYPE={type.ToUpperInvariant()}:{EscapeVCardValue(poBox)};{EscapeVCardValue(extendedAddress)};{EscapeVCardValue(streetAddress)};{EscapeVCardValue(city)};{EscapeVCardValue(region)};{EscapeVCardValue(postalCode)};{EscapeVCardValue(country)}");
                }
            }

            if (person.Organizations != null)
            {
                foreach (var org in person.Organizations)
                {
                    if (!string.IsNullOrEmpty(org.Name))
                    {
                        vcard.AppendLine($"ORG:{EscapeVCardValue(org.Name)}");
                    }
                    if (!string.IsNullOrEmpty(org.Title))
                    {
                        vcard.AppendLine($"TITLE:{EscapeVCardValue(org.Title)}");
                    }
                }
            }

            if (person.Birthdays != null && person.Birthdays.Count > 0)
            {
                var birthday = person.Birthdays[0];
                if (birthday.Date != null)
                {
                    var date = birthday.Date;
                    var year = date.Year ?? 0;
                    var month = date.Month ?? 0;
                    var day = date.Day ?? 0;
                    
                    if (year > 0 && month > 0 && day > 0)
                    {
                        vcard.AppendLine($"BDAY:{year:D4}-{month:D2}-{day:D2}");
                    }
                    else if (month > 0 && day > 0)
                    {
                        vcard.AppendLine($"BDAY:--{month:D2}-{day:D2}");
                    }
                }
            }

            if (person.Metadata?.Sources != null)
            {
                foreach (var source in person.Metadata.Sources)
                {
                    if (source.Id != null)
                    {
                        vcard.AppendLine($"UID:{EscapeVCardValue(source.Id)}");
                        break;
                    }
                }
            }

            vcard.AppendLine("END:VCARD");
            return vcard.ToString();
        }

        /// <summary>
        /// Escapes special characters in vCard values.
        /// </summary>
        /// <param name="value">Value to escape.</param>
        /// <returns>Escaped value.</returns>
        private string EscapeVCardValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\\", "\\\\")
                .Replace(",", "\\,")
                .Replace(";", "\\;")
                .Replace("\n", "\\n")
                .Replace("\r", string.Empty);
        }
    }

    /// <summary>
    /// Represents a contact with its Person data and vCard export.
    /// </summary>
    public class ContactData
    {
        /// <summary>
        /// The resource name identifier from Google.
        /// </summary>
        public string? ResourceName { get; set; }

        /// <summary>
        /// The Person object from Google People API.
        /// </summary>
        public Person? Person { get; set; }

        /// <summary>
        /// The vCard representation of the contact.
        /// </summary>
        public string? VCard { get; set; }
    }
}
