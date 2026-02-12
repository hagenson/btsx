using Google.Apis.PeopleService.v1.Data;

namespace Btsx.Google
{
    /// <summary>
    /// Represents a contact with its Person data and vCard export.
    /// </summary>
    internal class GoogleContactData: IContactData
    {
        public GoogleContactData(Person person)
        {
            this.person = person;
        }

        public GoogleContactData()
        {
            person = new Person();
        }
       
        /// <summary>
        /// The Person object from Google People API.
        /// </summary>
        internal Person person;
             

        // IContactData implementation

        public string? FormattedName
        {
            get => person.Names?.FirstOrDefault()?.DisplayName;
            set
            {
                if (person != null)
                {
                    person.Names ??= new List<Name>();
                    var name = person.Names.FirstOrDefault();
                    if (name == null)
                    {
                        name = new Name();
                        person.Names.Add(name);
                    }
                    name.DisplayName = value;
                }
            }
        }

        public string? FamilyName
        {
            get => person.Names?.FirstOrDefault()?.FamilyName;
            set
            {
                if (person != null)
                {
                    person.Names ??= new List<Name>();
                    var name = person.Names.FirstOrDefault();
                    if (name == null)
                    {
                        name = new Name();
                        person.Names.Add(name);
                    }
                    name.FamilyName = value;
                }
            }
        }

        public string? GivenName
        {
            get => person.Names?.FirstOrDefault()?.GivenName;
            set
            {
                if (person != null)
                {
                    person.Names ??= new List<Name>();
                    var name = person.Names.FirstOrDefault();
                    if (name == null)
                    {
                        name = new Name();
                        person.Names.Add(name);
                    }
                    name.GivenName = value;
                }
            }
        }

        public string? AdditionalNames
        {
            get => person.Names?.FirstOrDefault()?.MiddleName;
            set
            {
                if (person != null)
                {
                    person.Names ??= new List<Name>();
                    var name = person.Names.FirstOrDefault();
                    if (name == null)
                    {
                        name = new Name();
                        person.Names.Add(name);
                    }
                    name.MiddleName = value;
                }
            }
        }

        public string? HonorificPrefixes
        {
            get => person.Names?.FirstOrDefault()?.HonorificPrefix;
            set
            {
                if (person != null)
                {
                    person.Names ??= new List<Name>();
                    var name = person.Names.FirstOrDefault();
                    if (name == null)
                    {
                        name = new Name();
                        person.Names.Add(name);
                    }
                    name.HonorificPrefix = value;
                }
            }
        }

        public string? HonorificSuffixes
        {
            get => person.Names?.FirstOrDefault()?.HonorificSuffix;
            set
            {
                if (person != null)
                {
                    person.Names ??= new List<Name>();
                    var name = person.Names.FirstOrDefault();
                    if (name == null)
                    {
                        name = new Name();
                        person.Names.Add(name);
                    }
                    name.HonorificSuffix = value;
                }
            }
        }

        public string? Nickname
        {
            get => person.Nicknames?.FirstOrDefault()?.Value;
            set
            {
                if (person != null)
                {
                    person.Nicknames ??= new List<Nickname>();
                    var nickname = person.Nicknames.FirstOrDefault();
                    if (nickname == null)
                    {
                        nickname = new Nickname();
                        person.Nicknames.Add(nickname);
                    }
                    nickname.Value = value;
                }
            }
        }

        public string? UniqueIdentifier
        {
            get => person.ResourceName;
            set
            {
                if (person != null)
                {
                    person.ResourceName = value;
                }
            }
        }

        public List<string>? EmailAddresses
        {
            get => person.EmailAddresses?.Select(e => e.Value).Distinct().ToList();
            set
            {
                if (person != null)
                {
                    person.EmailAddresses = value?.Select(v => new EmailAddress { Value = v }).ToList();
                }
            }
        }

        public List<string>? PhoneNumbers
        {
            get => person.PhoneNumbers?.Select(p => p.Value).ToList();
            set
            {
                if (person != null)
                {
                    person.PhoneNumbers = value?.Select(v => new PhoneNumber { Value = v }).ToList();
                }
            }
        }

        public List<string>? Addresses
        {
            get => person.Addresses?.Select(a => a.FormattedValue ?? 
                $"{a.StreetAddress}, {a.City}, {a.Region} {a.PostalCode}, {a.Country}".Trim()).ToList();
            set
            {
                if (person != null)
                {
                    person.Addresses = value?.Select(v => new Address { FormattedValue = v }).ToList();
                }
            }
        }

        public string? Organization
        {
            get => person.Organizations?.FirstOrDefault()?.Name;
            set
            {
                if (person != null)
                {
                    person.Organizations ??= new List<Organization>();
                    var org = person.Organizations.FirstOrDefault();
                    if (org == null)
                    {
                        org = new Organization();
                        person.Organizations.Add(org);
                    }
                    org.Name = value;
                }
            }
        }

        public string? Title
        {
            get => person.Organizations?.FirstOrDefault()?.Title;
            set
            {
                if (person != null)
                {
                    person.Organizations ??= new List<Organization>();
                    var org = person.Organizations.FirstOrDefault();
                    if (org == null)
                    {
                        org = new Organization();
                        person.Organizations.Add(org);
                    }
                    org.Title = value;
                }
            }
        }

        public string? Role
        {
            get => person.Organizations?.FirstOrDefault()?.JobDescription;
            set
            {
                if (person != null)
                {
                    person.Organizations ??= new List<Organization>();
                    var org = person.Organizations.FirstOrDefault();
                    if (org == null)
                    {
                        org = new Organization();
                        person.Organizations.Add(org);
                    }
                    org.JobDescription = value;
                }
            }
        }

        public DateTime? Birthday
        {
            get
            {
                var birthday = person.Birthdays?.FirstOrDefault()?.Date;
                if (birthday != null && birthday.Year.HasValue && birthday.Month.HasValue && birthday.Day.HasValue)
                {
                    return new DateTime(birthday.Year.Value, birthday.Month.Value, birthday.Day.Value);
                }
                return null;
            }
            set
            {
                if (person != null)
                {
                    person.Birthdays ??= new List<Birthday>();
                    var birthday = person.Birthdays.FirstOrDefault();
                    if (birthday == null)
                    {
                        birthday = new Birthday();
                        person.Birthdays.Add(birthday);
                    }
                    if (value.HasValue)
                    {
                        birthday.Date = new Date
                        {
                            Year = value.Value.Year,
                            Month = value.Value.Month,
                            Day = value.Value.Day
                        };
                    }
                    else
                    {
                        birthday.Date = null;
                    }
                }
            }
        }

        public DateTime? Anniversary
        {
            get
            {
                var event_ = person.Events?.FirstOrDefault(e => e.Type == "anniversary");
                var date = event_?.Date;
                if (date != null && date.Year.HasValue && date.Month.HasValue && date.Day.HasValue)
                {
                    return new DateTime(date.Year.Value, date.Month.Value, date.Day.Value);
                }
                return null;
            }
            set
            {
                if (person != null)
                {
                    person.Events ??= new List<Event>();
                    var event_ = person.Events.FirstOrDefault(e => e.Type == "anniversary");
                    if (event_ == null)
                    {
                        event_ = new Event { Type = "anniversary" };
                        person.Events.Add(event_);
                    }
                    if (value.HasValue)
                    {
                        event_.Date = new Date
                        {
                            Year = value.Value.Year,
                            Month = value.Value.Month,
                            Day = value.Value.Day
                        };
                    }
                    else
                    {
                        event_.Date = null;
                    }
                }
            }
        }

        public string? Gender
        {
            get => person.Genders?.FirstOrDefault()?.Value;
            set
            {
                if (person != null)
                {
                    person.Genders ??= new List<Gender>();
                    var gender = person.Genders.FirstOrDefault();
                    if (gender == null)
                    {
                        gender = new Gender();
                        person.Genders.Add(gender);
                    }
                    gender.Value = value;
                }
            }
        }

        public List<string>? InstantMessagingAddresses
        {
            get => person.ImClients?.Select(im => im.FormattedProtocol != null ? $"{im.FormattedProtocol}:{im.Username}" : im.Username).ToList();
            set
            {
                if (person != null)
                {
                    person.ImClients = value?.Select(v => new ImClient { Username = v }).ToList();
                }
            }
        }

        public string? Language
        {
            get => person.Locales?.FirstOrDefault()?.Value;
            set
            {
                if (person != null)
                {
                    person.Locales ??= new List<Locale>();
                    var locale = person.Locales.FirstOrDefault();
                    if (locale == null)
                    {
                        locale = new Locale();
                        person.Locales.Add(locale);
                    }
                    locale.Value = value;
                }
            }
        }

        public string? TimeZone { get; set; }

        public string? GeographicPosition { get; set; }

        public List<string>? Categories { get; set; }

        public string? Notes
        {
            get => person.Biographies?.FirstOrDefault()?.Value;
            set
            {
                if (person != null)
                {
                    person.Biographies ??= new List<Biography>();
                    var bio = person.Biographies.FirstOrDefault();
                    if (bio == null)
                    {
                        bio = new Biography();
                        person.Biographies.Add(bio);
                    }
                    bio.Value = value;
                }
            }
        }

        public string? ProductId { get; set; }

        public DateTime? Revision
        {
            get
            {
                var metadata = person.Metadata;
                if (metadata?.Sources != null)
                {
                    var updateTime = metadata.Sources.FirstOrDefault()?.UpdateTimeDateTimeOffset;
                    if (updateTime != null)
                    {
                        return updateTime.Value.DateTime;
                    }
                }
                return null;
            }
            set { }
        }

        public List<string>? Urls
        {
            get => person.Urls?.Select(u => u.Value).ToList();
            set
            {
                if (person != null)
                {
                    person.Urls = value?.Select(v => new Url { Value = v }).ToList();
                }
            }
        }

        public string? PublicKey { get; set; }

        public string? Photo
        {
            get => person.Photos?.FirstOrDefault()?.Url;
            set
            {
                if (person != null)
                {
                    person.Photos ??= new List<Photo>();
                    var photo = person.Photos.FirstOrDefault();
                    if (photo == null)
                    {
                        photo = new Photo();
                        person.Photos.Add(photo);
                    }
                    photo.Url = value;
                }
            }
        }

        public string? Logo { get; set; }

        public string? Sound { get; set; }

        public string? CalendarAddressUri
        {
            get => person.CalendarUrls?.FirstOrDefault()?.Url;
            set
            {
                if (person != null)
                {
                    person.CalendarUrls ??= new List<CalendarUrl>();
                    var calUrl = person.CalendarUrls.FirstOrDefault();
                    if (calUrl == null)
                    {
                        calUrl = new CalendarUrl();
                        person.CalendarUrls.Add(calUrl);
                    }
                    calUrl.Url = value;
                }
            }
        }

        public string? CalendarUri
        {
            get => person.CalendarUrls?.Skip(1).FirstOrDefault()?.Url;
            set
            {
                if (person != null)
                {
                    person.CalendarUrls ??= new List<CalendarUrl>();
                    if (person.CalendarUrls.Count < 2)
                    {
                        person.CalendarUrls.Add(new CalendarUrl { Url = value });
                    }
                    else
                    {
                        person.CalendarUrls[1].Url = value;
                    }
                }
            }
        }

        public string? FreeBusyUrl { get; set; }

        public List<string>? RelatedContacts
        {
            get => person.Relations?.Select(r => r.Person).ToList();
            set
            {
                if (person != null)
                {
                    person.Relations = value?.Select(v => new Relation { Person = v }).ToList();
                }
            }
        }
    }

}
