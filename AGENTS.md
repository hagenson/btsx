# AGENTS.md

## Setup
```bash
dotnet restore BTSX.sln

# btsxweb frontend (TypeScript/Vue)
cd btsxweb
npm install

# Set required environment variable for encryption
# Linux/macOS: export ENCRYPTION_KEY="$(openssl rand -base64 32)"
# Windows: $env:ENCRYPTION_KEY = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

## Commands
- **Build Backend**: `dotnet build btsx.sln`
- **Build Frontend**: 
  - Windows: `.\btsxweb\build-frontend.ps1`
  - Linux/macOS: `./btsxweb/build-frontend.sh`
  - Or manually: `cd btsxweb && npm run build`
- **Build Frontend (dev mode with watch)**: `cd btsxweb && npm run dev`
- **Lint**: No linter configured
- **Test**: No unit tests configured
- **Run Console App**: `dotnet run --project ImapMove/ImapMove.csproj -- <srcUser> <srcServer> <srcPassword> <dstUser> <dstServer> <dstPassword>`
- **Run Web UI**: `dotnet run --project btsxweb/btsxweb.csproj` (requires frontend to be built first)

## Tech Stack

### Backend
- .NET 8.0
- MailKit 4.14.1 (IMAP client library)
- ASP.NET Core (Web UI)
- SignalR (Real-time updates)

### Frontend
- Vue 3 (JavaScript framework)
- TypeScript (Type-safe JavaScript)
- Webpack (Module bundler)
- Bootstrap 5.3 (CSS framework)

## Architecture

### btsx Library
Core library containing the IMAP mail migration logic:
- `MailMover`: Main class that performs IMAP migration
- `Creds`: Credentials for IMAP servers
- `MigrationStats`: Statistics tracking for migrations
- `StatusEventArgs`: Event arguments for status updates

### ImapMove Console Application
Console application for IMAP email migration that:
- Connects to source and destination IMAP servers
- Retrieves all folders and messages from source
- Recreates folder hierarchy on destination
- Copies messages with flags preserved
- Marks messages as deleted on source and expunges
- Implements error handling with progress notifications

### btsxweb Web Application
ASP.NET Core web application with Vue.js frontend providing a web UI for mail migration:

**Backend (ASP.NET Core)**:
- Hosted service (`MailMoverService`) runs migrations in background
- SignalR hub (`MigrationHub`) provides real-time communication
- OAuth integration for Google authentication
- RESTful API endpoints
- `EncryptionService`: AES-256 encryption for passwords and OAuth tokens in persistent storage
- `JobPersistenceService`: Persists migration jobs with encrypted credentials; clears credentials when jobs complete

**Frontend (Vue 3 + TypeScript)**:
- `IndexApp.vue`: Main migration form with server configuration
- `StatusApp.vue`: Real-time progress tracking with SignalR
- `OAuthCallbackApp.vue`: OAuth callback handler
- Reactive UI with automatic updates
- Type-safe TypeScript code
- Bootstrap 5.3 responsive design
- Progress bar display when ProgressUpdates is enabled
- Live status message display
- Migration statistics on completion

## Code Style
- C# with nullable reference types enabled
- Async/await patterns for I/O operations
- Descriptive console output with Unicode symbols
- Do not use underscore (_) prefix for member names (use camelCase for private fields)
