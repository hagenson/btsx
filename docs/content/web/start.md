---
title: "Getting Started"
weight: 1
---

# Getting Started with the Web Interface

This guide walks you through starting your first email migration using the BTSX web interface.

## Prerequisites

- BTSX web application running (see [Installation](../installation/local/))
- Source IMAP server credentials
- Destination IMAP server credentials
- Network access to both IMAP servers

## Accessing the Web Interface

1. Start the BTSX web application:
   ```bash
   dotnet run --project btsxweb/btsxweb.csproj
   ```

2. Open your browser and navigate to:
   ```
   http://localhost:5000
   ```

![Login Screen](screenshots/web-login.png)

## Configuring a Migration

### Source Server Configuration

Fill in the source server details:

1. **Username**: Your email address or IMAP username
2. **Server**: IMAP server hostname (e.g., `imap.gmail.com`)
3. **Port**: IMAP port (typically 993 for SSL/TLS)
4. **Password**: Your account password or app-specific password

![Source Server Configuration](screenshots/source-config.png)

### Destination Server Configuration

Fill in the destination server details:

1. **Username**: Destination email address or IMAP username
2. **Server**: Destination IMAP server hostname
3. **Port**: IMAP port (typically 993 for SSL/TLS)
4. **Password**: Destination account password

![Destination Server Configuration](screenshots/destination-config.png)

## OAuth Authentication (Optional)

For supported providers like Gmail:

1. Click the **"Sign in with Google"** button
2. Authorize BTSX to access your account
3. Credentials will be automatically populated

![OAuth Authentication](screenshots/oauth-flow.png)

## Migration Options

Configure additional options:

- **Delete from Source**: Check to remove emails from source after successful migration
- **Progress Updates**: Enable detailed progress reporting
- **Save Job**: Save the migration configuration for later use

![Migration Options](screenshots/migration-options.png)

## Starting the Migration

1. Review all configuration details
2. Click **"Start Migration"**
3. You'll be redirected to the status page

![Start Migration Button](screenshots/start-button.png)

## What Happens Next

Once started, BTSX will:

1. Connect to both IMAP servers
2. Retrieve the folder hierarchy from the source
3. Create folders on the destination
4. Copy emails with their flags
5. Optionally mark source emails as deleted
6. Provide real-time progress updates

See [Status Monitoring](status/) for details on tracking your migration.

## Troubleshooting

### Connection Errors

- Verify server hostnames and ports
- Check firewall and network connectivity
- Ensure credentials are correct
- For Gmail, use an app-specific password

### Authentication Errors

- Verify username and password
- Check if two-factor authentication requires app passwords
- For OAuth, ensure proper authorization

### Migration Stops

- Check the status page for error messages
- Verify network connectivity remains stable
- Check server quotas and storage limits

## Tips for Successful Migration

1. **Test First**: Try migrating a single folder first
2. **Check Quotas**: Ensure destination has sufficient storage
3. **Use OAuth**: More secure than password authentication for supported providers
4. **Monitor Progress**: Keep the status page open during migration
5. **Save Jobs**: Use job persistence for large migrations you can't complete in one session
