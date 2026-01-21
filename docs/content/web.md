---
title: "Web Application"
weight: 3
---

# Web Application

The BTSX web application provides a user-friendly interface for performing IMAP email migrations with real-time progress updates.

## Running the Web Application

### Prerequisites

- Frontend must be built (see [Installation]({{< relref "/installation" >}}))
- ENCRYPTION_KEY environment variable must be set

### Start the Server

```bash
dotnet run --project btsxweb/btsxweb.csproj
```

The web application will be available at `http://localhost:5000` (or the port specified in the console output).

## Features

### Migration Form
- Configure source and destination IMAP servers
- Enter credentials directly or use OAuth for Google
- Enable/disable progress updates
- Start migration with a single click

### Real-time Progress
- Live status updates via SignalR
- Progress bar showing completion percentage
- Current operation display
- Migration statistics on completion

### OAuth Integration
- Google OAuth support for secure authentication
- No need to use app-specific passwords
- Simplified authentication flow

### Job Persistence
- Migrations are saved and can be resumed
- Encrypted credential storage
- Automatic cleanup when jobs complete

## User Interface

### Migration Configuration

1. **Source Server**
   - Email/Username
   - IMAP Server hostname
   - Password (or use OAuth)

2. **Destination Server**
   - Email/Username
   - IMAP Server hostname
   - Password (or use OAuth)

3. **Options**
   - Enable progress updates checkbox

### Progress Tracking

Once migration starts:
- Status messages appear in real-time
- Progress bar shows completion
- Current folder being processed
- Statistics:
  - Total messages copied
  - Total folders processed
  - Time elapsed

## OAuth Setup

To use Google OAuth:

1. Create OAuth credentials in Google Cloud Console
2. Configure authorized redirect URIs
3. Set credentials in `appsettings.json`:

```json
{
  "GoogleOAuth": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

4. Click "Sign in with Google" in the web interface

## Security

- Passwords are encrypted using AES-256
- OAuth tokens are encrypted in storage
- Credentials are cleared when jobs complete
- HTTPS recommended for production use

## Architecture

### Backend Components
- **MailMoverService**: Background service for migrations
- **MigrationHub**: SignalR hub for real-time updates
- **JobPersistenceService**: Manages job storage
- **EncryptionService**: Handles credential encryption

### Frontend Components
- **IndexApp.vue**: Main migration form
- **StatusApp.vue**: Progress tracking interface
- **OAuthCallbackApp.vue**: OAuth callback handler

## API Endpoints

- `POST /api/migration/start` - Start new migration
- `GET /api/migration/status/{jobId}` - Get job status
- `POST /api/oauth/google` - Initiate Google OAuth
- `GET /api/oauth/callback` - OAuth callback handler
