---
title: "Configuration"
weight: 4
---

# Configuration

## Environment Variables

### ENCRYPTION_KEY (Required for Web UI)

The web application requires an encryption key for securing stored credentials.

**Windows (PowerShell):**
```powershell
$env:ENCRYPTION_KEY = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

**Linux/macOS:**
```bash
export ENCRYPTION_KEY="$(openssl rand -base64 32)"
```

**Production:** Store this value securely and set it in your deployment environment.

## Application Settings (Web UI)

Configuration is managed via `appsettings.json` in the `btsxweb` directory.

### Basic Settings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Google OAuth Configuration

To enable Google authentication:

```json
{
  "GoogleOAuth": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret",
    "RedirectUri": "https://yourdomain.com/oauth/callback"
  }
}
```

#### Setting up Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Gmail API
4. Create OAuth 2.0 credentials:
   - Application type: Web application
   - Authorized redirect URIs: `https://yourdomain.com/oauth/callback`
5. Copy Client ID and Client Secret to `appsettings.json`

### IMAP Server Settings

Common IMAP server configurations:

#### Gmail
- **Server:** `imap.gmail.com`
- **Port:** 993 (SSL/TLS)
- **Auth:** OAuth or App Password

#### Outlook/Office 365
- **Server:** `outlook.office365.com`
- **Port:** 993 (SSL/TLS)
- **Auth:** Password

#### Yahoo Mail
- **Server:** `imap.mail.yahoo.com`
- **Port:** 993 (SSL/TLS)
- **Auth:** App Password

#### iCloud Mail
- **Server:** `imap.mail.me.com`
- **Port:** 993 (SSL/TLS)
- **Auth:** App Password

## Development Configuration

For local development, create `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "GoogleOAuth": {
    "ClientId": "dev-client-id",
    "ClientSecret": "dev-client-secret",
    "RedirectUri": "http://localhost:5000/oauth/callback"
  }
}
```

**Note:** `appsettings.Development.json` is gitignored to prevent accidental commits of credentials.

## Job Storage

Migration jobs are persisted in the `JobStorage` directory with encrypted credentials:

- **Location:** `btsxweb/JobStorage/`
- **Format:** JSON files with encrypted sensitive data
- **Cleanup:** Automatically cleared when jobs complete

## Logging

Logs are written to the console by default. To configure file logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "File": {
      "Path": "logs/btsx-.txt",
      "FileSizeLimitBytes": 10485760,
      "RetainedFileCountLimit": 7
    }
  }
}
```

## Port Configuration

By default, the web application runs on port 5000 (HTTP) and 5001 (HTTPS).

To change the port, modify `Properties/launchSettings.json` or set environment variables:

```bash
export ASPNETCORE_URLS="http://localhost:8080"
```
