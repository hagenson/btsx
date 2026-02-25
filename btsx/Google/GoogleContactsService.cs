using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;

namespace Btsx.Google
{
    /// <summary>
    /// Implements <see cref="IContactService"/> using Google's PeopleService API.
    /// </summary>
    public class GoogleContactsService : IContactService
    {
        /// <summary>
        /// Initializes a new instance of the GoogleContactsService.
        /// </summary>
        public GoogleContactsService(Creds creds)
        {
            if (string.IsNullOrWhiteSpace(creds.OAuthToken))
                throw new ArgumentException("OAuth token cannot be null or empty", nameof(oauthToken));

            this.oauthToken = creds.OAuthToken;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteContactAsync(IContactData contact, CancellationToken cancellationToken)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            var googleContact = contact as GoogleContactData;
            if (googleContact == null)
                throw new ArgumentException("Contact must be a GoogleContactData instance", nameof(contact));

            var resourceName = googleContact.person.ResourceName;
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Contact resource name cannot be null or empty", nameof(contact));

            try
            {
                var credential = GoogleCredential.FromAccessToken(oauthToken);

                using (var service = new PeopleServiceService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "BTSX Contact Fetcher"
                }))
                {
                    var request = service.People.DeleteContact(resourceName);
                    await request.ExecuteAsync(cancellationToken);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<List<IContactData>> ListCollectedContactsAsync(CancellationToken cancellationToken)
        {
            var contacts = new List<IContactData>();

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
                    var request = service.OtherContacts.List();
                    request.ReadMask = string.Join(",", otherContactFields);
                    request.PageSize = 1000;
                    request.PageToken = pageToken;

                    var response = await request.ExecuteAsync(cancellationToken);

                    if (response.OtherContacts != null)
                    {
                        foreach (var person in response.OtherContacts)
                        {
                            if (!contacts.Any(c => ((GoogleContactData)c).person.ResourceName == person.ResourceName))
                            {
                                var contactData = new GoogleContactData(person);
                                contactData.Categories = new List<string> { "Collected Contacts" };
                                contacts.Add(contactData);
                            }
                        }
                    }

                    pageToken = response.NextPageToken;
                    pageNumber++;
                } while (!string.IsNullOrEmpty(pageToken) && !cancellationToken.IsCancellationRequested);
            }

            return contacts;
        }

        /// <inheritdoc/>
        public async Task<List<IContactData>> ListContactsAsync(CancellationToken cancellationToken)
        {
            var contacts = new List<IContactData>();

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
                    request.PersonFields = string.Join(",", personFields);
                    request.PageSize = 1000;
                    request.PageToken = pageToken;

                    var response = await request.ExecuteAsync(cancellationToken);

                    if (response.Connections != null)
                    {
                        foreach (var person in response.Connections)
                        {
                            if (!contacts.Any(c => ((GoogleContactData)c).person.ResourceName == person.ResourceName))
                            {
                                var contactData = new GoogleContactData(person);
                                contacts.Add(contactData);
                            }
                        }
                    }

                    pageToken = response.NextPageToken;
                    pageNumber++;
                } while (!string.IsNullOrEmpty(pageToken) && !cancellationToken.IsCancellationRequested);
            }

            // Now match to the groups
            var groups = await ListContactGroupsAsync(cancellationToken);
            foreach (GoogleContactData contact in contacts)
            {
                var memberGroups = groups.Where(g => g.MemberResourceNames != null
                        && g.MemberResourceNames.Contains(contact.UniqueIdentifier))
                    .Select(g => g.FormattedName).ToList();
                contact.Categories = memberGroups;
            }
            return contacts;
        }

        /// <inheritdoc/>
        public Task<List<IContactData>> MatchContactsAsync(IContactData contact, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> TestConnectionAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateContactAsync(IContactData contact, CancellationToken cancellationToken)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            var googleContact = contact as GoogleContactData;
            if (googleContact == null)
                throw new ArgumentException("Contact must be a GoogleContactData instance", nameof(contact));

            var resourceName = googleContact.person.ResourceName;
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Contact resource name cannot be null or empty", nameof(contact));

            try
            {
                var credential = GoogleCredential.FromAccessToken(oauthToken);

                using (var service = new PeopleServiceService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "BTSX Contact Fetcher"
                }))
                {
                    var request = service.People.UpdateContact(googleContact.person, resourceName);
                    request.UpdatePersonFields = string.Join(",", personFields);
                    await request.ExecuteAsync(cancellationToken);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public Task<bool> CreateContactAsync(IContactData contact, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        private readonly string oauthToken;

        private string[] otherContactFields = new string[]
        {
            "emailAddresses",
            "metadata",
            "names",
            "phoneNumbers",
            "photos"
        };

        private string[] personFields = new string[]
        {
            "addresses",
            "ageRanges",
            "biographies",
            "birthdays",
            "calendarUrls",
            "clientData",
            "coverPhotos",
            "emailAddresses",
            "events",
            "externalIds",
            "genders",
            "imClients",
            "interests",
            "locales",
            "locations",
            "memberships",
            "metadata",
            "miscKeywords",
            "names",
            "nicknames",
            "occupations",
            "organizations",
            "phoneNumbers",
            "photos",
            "relations",
            "sipAddresses",
            "skills",
            "urls",
            "userDefined"
        };

        private async Task<List<ContactGroup>> ListContactGroupsAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ContactGroup>();
            var credential = GoogleCredential.FromAccessToken(oauthToken);

            using (var service = new PeopleServiceService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "BTSX Contact Fetcher"
            }))
            {
                string? pageToken = null;
                do
                {
                    if (cancellationToken.IsCancellationRequested)
                        return result;
                    var request = service.ContactGroups.List();
                    request.PageSize = 1000;
                    request.PageToken = pageToken;

                    var response = await request.ExecuteAsync(cancellationToken);

                    if (response.ContactGroups != null)
                    {
                        foreach (var group in response.ContactGroups.Where(g => !(g.Name == "all" && g.GroupType == "SYSTEM_CONTACT_GROUP")))
                        {
                            // Get the group with the members
                            var greq = service.ContactGroups.Get(group.ResourceName);
                            greq.MaxMembers = 10000;
                            var gresp = await greq.ExecuteAsync(cancellationToken);
                            if (gresp != null)
                                result.Add(gresp);
                        }
                    }

                    pageToken = response.NextPageToken;
                } while (!string.IsNullOrEmpty(pageToken) && !cancellationToken.IsCancellationRequested);
            }
            return result;
        }
    }
}