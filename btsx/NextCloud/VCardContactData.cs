using System.Globalization;

namespace Btsx.NextCloud
{
    /// <summary>
    /// Implements <see cref="IContactService"/> for V-Card data.
    /// </summary>
    public class VCardContactData : IContactData
    {
        /// <summary>
        /// Initialises the contact from the supplied v-card data.
        /// </summary>
        /// <param name="vcardContent">V-Card data to create contact from.</param>
        public VCardContactData(string vcardContent)
        {
            ParseVCard(vcardContent);
        }

        /// <inheritdoc/>
        public string? AdditionalNames { get; set; }

        /// <inheritdoc/>
        public List<string>? Addresses { get; set; }

        /// <inheritdoc/>
        public DateTime? Anniversary { get; set; }

        /// <inheritdoc/>
        public DateTime? Birthday { get; set; }

        /// <inheritdoc/>
        public string? CalendarAddressUri { get; set; }

        /// <inheritdoc/>
        public string? CalendarUri { get; set; }

        /// <inheritdoc/>
        public List<string>? Categories { get; set; }

        /// <inheritdoc/>
        public List<string>? EmailAddresses { get; set; }

        /// <inheritdoc/>
        public string? FamilyName { get; set; }

        /// <inheritdoc/>
        public string? FormattedName { get; set; }

        /// <inheritdoc/>
        public string? FreeBusyUrl { get; set; }

        /// <inheritdoc/>
        public string? Gender { get; set; }

        /// <inheritdoc/>
        public string? GeographicPosition { get; set; }

        /// <inheritdoc/>
        public string? GivenName { get; set; }

        /// <inheritdoc/>
        public string? HonorificPrefixes { get; set; }

        /// <inheritdoc/>
        public string? HonorificSuffixes { get; set; }

        /// <inheritdoc/>
        public List<string>? InstantMessagingAddresses { get; set; }

        /// <inheritdoc/>
        public string? Language { get; set; }

        /// <inheritdoc/>
        public string? Logo { get; set; }

        /// <inheritdoc/>
        public string? Nickname { get; set; }

        /// <inheritdoc/>
        public string? Notes { get; set; }

        /// <inheritdoc/>
        public string? Organization { get; set; }

        /// <inheritdoc/>
        public List<string>? PhoneNumbers { get; set; }

        /// <inheritdoc/>
        public string? Photo { get; set; }

        /// <inheritdoc/>
        public string? ProductId { get; set; }

        /// <inheritdoc/>
        public string? PublicKey { get; set; }

        /// <inheritdoc/>
        public List<string>? RelatedContacts { get; set; }

        /// <inheritdoc/>
        public DateTime? Revision { get; set; }

        /// <inheritdoc/>
        public string? Role { get; set; }

        /// <inheritdoc/>
        public string? Sound { get; set; }

        /// <inheritdoc/>
        public string? TimeZone { get; set; }

        /// <inheritdoc/>
        public string? Title { get; set; }

        /// <inheritdoc/>
        public string? UniqueIdentifier { get; set; }

        /// <inheritdoc/>
        public List<string>? Urls { get; set; }

        private void ParseVCard(string vcardContent)
        {
            if (string.IsNullOrWhiteSpace(vcardContent))
                return;

            var lines = UnfoldLines(vcardContent);

            foreach (var line in lines)
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex <= 0)
                    continue;

                var propertyPart = line.Substring(0, colonIndex);
                var valuePart = line.Substring(colonIndex + 1);

                var semicolonIndex = propertyPart.IndexOf(';');
                var propertyName = semicolonIndex > 0
                    ? propertyPart.Substring(0, semicolonIndex)
                    : propertyPart;

                var unescapedValue = UnescapeVCardValue(valuePart);

                switch (propertyName.ToUpperInvariant())
                {
                    case "FN":
                        FormattedName = unescapedValue;
                        break;

                    case "N":
                        var nameParts = SplitVCardValue(unescapedValue);
                        if (nameParts.Length > 0) FamilyName = nameParts[0];
                        if (nameParts.Length > 1) GivenName = nameParts[1];
                        if (nameParts.Length > 2) AdditionalNames = nameParts[2];
                        if (nameParts.Length > 3) HonorificPrefixes = nameParts[3];
                        if (nameParts.Length > 4) HonorificSuffixes = nameParts[4];
                        break;

                    case "NICKNAME":
                        Nickname = unescapedValue;
                        break;

                    case "UID":
                        UniqueIdentifier = unescapedValue;
                        break;

                    case "EMAIL":
                        EmailAddresses ??= new List<string>();
                        if (!string.IsNullOrWhiteSpace(unescapedValue))
                            EmailAddresses.Add(unescapedValue);
                        break;

                    case "TEL":
                        PhoneNumbers ??= new List<string>();
                        if (!string.IsNullOrWhiteSpace(unescapedValue))
                            PhoneNumbers.Add(unescapedValue);
                        break;

                    case "ADR":
                        Addresses ??= new List<string>();
                        var adrParts = SplitVCardValue(unescapedValue);
                        var address = string.Join(", ", adrParts.Where(p => !string.IsNullOrWhiteSpace(p)));
                        if (!string.IsNullOrWhiteSpace(address))
                            Addresses.Add(address);
                        break;

                    case "ORG":
                        Organization = unescapedValue;
                        break;

                    case "TITLE":
                        Title = unescapedValue;
                        break;

                    case "ROLE":
                        Role = unescapedValue;
                        break;

                    case "BDAY":
                        Birthday = ParseVCardDate(unescapedValue);
                        break;

                    case "ANNIVERSARY":
                        Anniversary = ParseVCardDate(unescapedValue);
                        break;

                    case "GENDER":
                        Gender = unescapedValue;
                        break;

                    case "IMPP":
                        InstantMessagingAddresses ??= new List<string>();
                        if (!string.IsNullOrWhiteSpace(unescapedValue))
                            InstantMessagingAddresses.Add(unescapedValue);
                        break;

                    case "LANG":
                        Language = unescapedValue;
                        break;

                    case "TZ":
                        TimeZone = unescapedValue;
                        break;

                    case "GEO":
                        GeographicPosition = unescapedValue;
                        break;

                    case "CATEGORIES":
                        Categories = SplitVCardValue(unescapedValue).Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
                        break;

                    case "NOTE":
                        Notes = unescapedValue;
                        break;

                    case "PRODID":
                        ProductId = unescapedValue;
                        break;

                    case "REV":
                        Revision = ParseVCardDateTime(unescapedValue);
                        break;

                    case "URL":
                        Urls ??= new List<string>();
                        if (!string.IsNullOrWhiteSpace(unescapedValue))
                            Urls.Add(unescapedValue);
                        break;

                    case "KEY":
                        PublicKey = unescapedValue;
                        break;

                    case "PHOTO":
                        Photo = unescapedValue;
                        break;

                    case "LOGO":
                        Logo = unescapedValue;
                        break;

                    case "SOUND":
                        Sound = unescapedValue;
                        break;

                    case "CALADRURI":
                        CalendarAddressUri = unescapedValue;
                        break;

                    case "CALURI":
                        CalendarUri = unescapedValue;
                        break;

                    case "FBURL":
                        FreeBusyUrl = unescapedValue;
                        break;

                    case "RELATED":
                        RelatedContacts ??= new List<string>();
                        if (!string.IsNullOrWhiteSpace(unescapedValue))
                            RelatedContacts.Add(unescapedValue);
                        break;
                }
            }
        }

        private DateTime? ParseVCardDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            dateString = dateString.Trim();

            var formats = new[]
            {
                "yyyyMMdd",
                "yyyy-MM-dd",
                "yyyyMMdd'T'HHmmss'Z'",
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                "yyyyMMdd'T'HHmmss",
                "yyyy-MM-dd'T'HH:mm:ss"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
            }

            if (DateTime.TryParse(dateString, out var parsedDate))
                return parsedDate;

            return null;
        }

        private DateTime? ParseVCardDateTime(string dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
                return null;

            dateTimeString = dateTimeString.Trim();

            var formats = new[]
            {
                "yyyyMMdd'T'HHmmss'Z'",
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                "yyyyMMdd'T'HHmmss",
                "yyyy-MM-dd'T'HH:mm:ss",
                "yyyyMMdd",
                "yyyy-MM-dd"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
                    return result.ToUniversalTime();
            }

            if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDateTime))
                return parsedDateTime.ToUniversalTime();

            return null;
        }

        private string[] SplitVCardValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<string>();

            var parts = new List<string>();
            var currentPart = new System.Text.StringBuilder();
            var escaped = false;

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];

                if (escaped)
                {
                    currentPart.Append(c);
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                    currentPart.Append(c);
                }
                else if (c == ';')
                {
                    parts.Add(currentPart.ToString());
                    currentPart.Clear();
                }
                else
                {
                    currentPart.Append(c);
                }
            }

            parts.Add(currentPart.ToString());
            return parts.ToArray();
        }

        private string UnescapeVCardValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\\n", "\n")
                .Replace("\\N", "\n")
                .Replace("\\,", ",")
                .Replace("\\;", ";")
                .Replace("\\\\", "\\");
        }

        private List<string> UnfoldLines(string vcardContent)
        {
            var lines = new List<string>();
            var currentLine = new System.Text.StringBuilder();

            using (var reader = new System.IO.StringReader(vcardContent))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(" ") || line.StartsWith("\t"))
                    {
                        currentLine.Append(line.Substring(1));
                    }
                    else
                    {
                        if (currentLine.Length > 0)
                        {
                            var completeLine = currentLine.ToString();
                            if (!completeLine.Equals("BEGIN:VCARD", StringComparison.OrdinalIgnoreCase) &&
                                !completeLine.Equals("END:VCARD", StringComparison.OrdinalIgnoreCase) &&
                                !completeLine.StartsWith("VERSION:", StringComparison.OrdinalIgnoreCase))
                            {
                                lines.Add(completeLine);
                            }
                        }
                        currentLine.Clear();
                        currentLine.Append(line);
                    }
                }

                if (currentLine.Length > 0)
                {
                    var completeLine = currentLine.ToString();
                    if (!completeLine.Equals("BEGIN:VCARD", StringComparison.OrdinalIgnoreCase) &&
                        !completeLine.Equals("END:VCARD", StringComparison.OrdinalIgnoreCase) &&
                        !completeLine.StartsWith("VERSION:", StringComparison.OrdinalIgnoreCase))
                    {
                        lines.Add(completeLine);
                    }
                }
            }

            return lines;
        }
    }
}