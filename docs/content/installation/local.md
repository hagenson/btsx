---
title: "Local Installation"
weight: 1
---

# Local Installation

Install and run BTSX on your local machine or server. This guide covers installation on Windows, Linux, and macOS.

## Prerequisites

### Required Software

- **.NET 8.0 SDK**: Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **Node.js 18+**: Download from [nodejs.org](https://nodejs.org/)
- **Git**: For cloning the repository

### Verify Installation

Check that prerequisites are installed:

```bash
# Check .NET version
dotnet --version
# Should show 8.0.x or higher

# Check Node.js version
node --version
# Should show v18.x or higher

# Check npm version
npm --version
```

## Installation Steps

### 1. Clone the Repository

```bash
git clone https://github.com/fhagenson/btsx.git
cd btsx
```

![Clone Repository](screenshots/git-clone.png)

### 2. Restore .NET Dependencies

```bash
dotnet restore BTSX.sln
```

This downloads all required NuGet packages:
- MailKit (IMAP library)
- ASP.NET Core
- SignalR
- Other dependencies

![Restore Packages](screenshots/dotnet-restore.png)

### 3. Install Frontend Dependencies

```bash
cd btsxweb
npm install
cd ..
```

This installs:
- Vue.js and related packages
- TypeScript
- Webpack and build tools
- Bootstrap

![NPM Install](screenshots/npm-install.png)

### 4. Set Environment Variables

#### Linux/macOS

```bash
# Generate and set encryption key
export ENCRYPTION_KEY="$(openssl rand -base64 32)"

# Optional: Configure OAuth
export GOOGLE_CLIENT_ID="your-client-id"
export GOOGLE_CLIENT_SECRET="your-client-secret"
```

Add to `~/.bashrc` or `~/.zshrc` for persistence:

```bash
echo 'export ENCRYPTION_KEY="'$(openssl rand -base64 32)'"' >> ~/.bashrc
source ~/.bashrc
```

#### Windows (PowerShell)

```powershell
# Generate and set encryption key
$env:ENCRYPTION_KEY = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# Optional: Configure OAuth
$env:GOOGLE_CLIENT_ID = "your-client-id"
$env:GOOGLE_CLIENT_SECRET = "your-client-secret"
```

For persistence, set system environment variables:

```powershell
[Environment]::SetEnvironmentVariable("ENCRYPTION_KEY", $env:ENCRYPTION_KEY, "User")
```

![Set Environment Variables](screenshots/env-variables.png)

### 5. Build the Frontend

#### Windows

```powershell
.\btsxweb\build-frontend.ps1
```

#### Linux/macOS

```bash
./btsxweb/build-frontend.sh
```

Or manually:

```bash
cd btsxweb
npm run build
cd ..
```

This creates optimized production assets in `btsxweb/wwwroot/dist/`.

![Build Frontend](screenshots/frontend-build.png)

### 6. Build the Backend

```bash
dotnet build BTSX.sln
```

This compiles:
- btsx library
- ImapMove CLI
- btsxweb application

![Build Backend](screenshots/dotnet-build.png)

## Running BTSX

### Web Interface

Start the web application:

```bash
dotnet run --project btsxweb/btsxweb.csproj
```

Access at: **http://localhost:5000**

![Web UI Running](screenshots/web-running.png)

#### Custom Port

Run on a different port:

```bash
dotnet run --project btsxweb/btsxweb.csproj --urls "http://localhost:8080"
```

### Command Line Interface

Run a migration from the command line:

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  sourceuser@old.com imap.old.com sourcepass \
  destuser@new.com imap.new.com destpass
```

See [CLI Parameters](../cli/parameters/) for detailed usage.

![CLI Running](screenshots/cli-running.png)

## Directory Structure

After installation, your directory structure looks like:

```
btsx/
├── btsx/                    # Core library
│   ├── MailMover.cs
│   └── btsx.csproj
├── ImapMove/                # CLI application
│   ├── Program.cs
│   └── ImapMove.csproj
├── btsxweb/                 # Web application
│   ├── wwwroot/
│   │   └── dist/           # Built frontend assets
│   ├── Controllers/
│   ├── Services/
│   ├── src/                # Frontend source
│   │   ├── IndexApp.vue
│   │   └── StatusApp.vue
│   └── btsxweb.csproj
└── BTSX.sln
```

## Configuration Files

### appsettings.json

Located in `btsxweb/appsettings.json`:

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

### OAuth Configuration

Add OAuth settings to `appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

Or use environment variables (recommended):
- `GOOGLE_CLIENT_ID`
- `GOOGLE_CLIENT_SECRET`

![Configuration](screenshots/config-file.png)

## Verifying Installation

### Test Web Interface

1. Start the web application
2. Open http://localhost:5000
3. Verify the form loads
4. Test with sample credentials (optional)

![Web UI Test](screenshots/web-test.png)

### Test CLI

Run with test parameters to verify:

```bash
dotnet run --project ImapMove/ImapMove.csproj -- help
```

Should show usage information.

## Development Mode

For frontend development with hot reload:

```bash
cd btsxweb
npm run dev
```

This starts Webpack dev server with watch mode. Changes to Vue components automatically rebuild.

In another terminal, run the backend:

```bash
dotnet run --project btsxweb/btsxweb.csproj
```

![Development Mode](screenshots/dev-mode.png)

## Updating

To update to the latest version:

```bash
# Pull latest changes
git pull origin main

# Restore and rebuild
dotnet restore BTSX.sln
dotnet build BTSX.sln

# Rebuild frontend
cd btsxweb
npm install
npm run build
cd ..
```

## Troubleshooting

### .NET SDK Not Found

**Error**: `The command could not be loaded`

**Solution**: Install .NET 8.0 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

### Node.js Errors

**Error**: `npm: command not found`

**Solution**: Install Node.js from [nodejs.org](https://nodejs.org/)

### Build Errors

**Error**: `error CS0246: The type or namespace name 'MailKit' could not be found`

**Solution**: Run `dotnet restore BTSX.sln`

### Frontend Build Fails

**Error**: `Module not found: Error: Can't resolve 'vue'`

**Solution**: 
```bash
cd btsxweb
npm install
```

### Port Already in Use

**Error**: `Failed to bind to address http://localhost:5000`

**Solution**: Use a different port:
```bash
dotnet run --project btsxweb/btsxweb.csproj --urls "http://localhost:5001"
```

### Encryption Key Missing

**Error**: `Encryption key not configured`

**Solution**: Set `ENCRYPTION_KEY` environment variable as shown in step 4.

### Permission Denied (Linux/macOS)

**Error**: `Permission denied` when running scripts

**Solution**: Make scripts executable:
```bash
chmod +x btsxweb/build-frontend.sh
```

## Uninstalling

To remove BTSX:

```bash
# Remove the directory
cd ..
rm -rf btsx/

# Clean environment variables
unset ENCRYPTION_KEY
unset GOOGLE_CLIENT_ID
unset GOOGLE_CLIENT_SECRET
```

On Windows (PowerShell):
```powershell
# Remove environment variables
[Environment]::SetEnvironmentVariable("ENCRYPTION_KEY", $null, "User")
```

## Next Steps

- **[Getting Started](../../web/start/)** - Configure your first migration
- **[CLI Usage](../../cli/parameters/)** - Learn CLI parameters
- **[Kubernetes](../kubernetes/)** - Deploy to Kubernetes

## Performance Tips

### Local Machine

- **Memory**: Ensure at least 2GB free RAM for large migrations
- **Network**: Use wired connection for stability
- **Background Apps**: Close unnecessary applications during migration

### Server Deployment

- **Firewall**: Open required ports (default 5000)
- **HTTPS**: Configure reverse proxy (nginx/Apache) for HTTPS
- **Systemd**: Create systemd service for auto-start
- **Monitoring**: Set up logging and monitoring

### Example Systemd Service (Linux)

Create `/etc/systemd/system/btsx.service`:

```ini
[Unit]
Description=BTSX Email Migration Tool
After=network.target

[Service]
Type=simple
User=btsx
WorkingDirectory=/opt/btsx
Environment="ENCRYPTION_KEY=your-key-here"
ExecStart=/usr/bin/dotnet run --project btsxweb/btsxweb.csproj --urls "http://0.0.0.0:5000"
Restart=always

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable btsx
sudo systemctl start btsx
```
