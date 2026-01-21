---
title: "Command Line Interface"
weight: 3
---

# Command Line Interface

The BTSX CLI (ImapMove) provides a scriptable command-line interface for email migrations. Perfect for automation, batch operations, and server environments without a GUI.

## Features

- **Scriptable**: Easily integrate into automation workflows
- **No UI Required**: Runs in headless environments
- **Direct Execution**: Simple command-line arguments
- **Console Output**: Clear progress indicators with Unicode symbols
- **Error Handling**: Detailed error messages and exit codes
- **Async Operations**: Non-blocking I/O for efficient performance

## Quick Start

Basic usage:

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  srcUser srcServer srcPassword \
  dstUser dstServer dstPassword
```

## Use Cases

- **Automated Migrations**: Schedule migrations with cron or Task Scheduler
- **Batch Operations**: Migrate multiple mailboxes in sequence
- **Server Environments**: Run on headless servers
- **CI/CD Pipelines**: Integrate into deployment workflows
- **Scripted Backups**: Automate regular email backups

## Output Example

```
→ Connecting to source server: imap.oldserver.com
✓ Connected to source server
→ Connecting to destination server: imap.newserver.com
✓ Connected to destination server
→ Scanning folders...
✓ Found 15 folders
→ Processing folder: INBOX
  → Copying 1000 messages
  ✓ Copied 1000 messages
→ Processing folder: INBOX/Archive
  → Copying 5000 messages
  ✓ Copied 5000 messages
✓ Migration complete
```

## Pages

- **[Parameters](parameters/)** - Detailed parameter reference and usage examples

## Architecture

The CLI application (`ImapMove`):

- Uses the core `btsx` library for migration logic
- Accepts credentials as command-line arguments
- Provides console output for progress tracking
- Implements error handling with appropriate exit codes
- Runs migrations synchronously in the console

## Screenshots

![CLI in Action](screenshots/cli-execution.png)

![CLI Completion](screenshots/cli-complete.png)

## Security Considerations

### Password Safety

Be careful with passwords in command-line arguments:

- Visible in process lists
- Stored in shell history
- Logged by monitoring tools

**Recommendations**:
- Use environment variables for passwords
- Clear shell history after use
- Run in isolated environments
- Consider the web interface for sensitive credentials

### Environment Variables

Safer approach using environment variables:

```bash
export SRC_USER="user@oldserver.com"
export SRC_PASS="password123"
export DST_USER="user@newserver.com"
export DST_PASS="password456"

dotnet run --project ImapMove/ImapMove.csproj -- \
  "$SRC_USER" imap.oldserver.com "$SRC_PASS" \
  "$DST_USER" imap.newserver.com "$DST_PASS"
```

## Exit Codes

- **0**: Success
- **1**: General error
- **2**: Connection error
- **3**: Authentication error
- **4**: Migration error

## Integration Examples

### Bash Script

```bash
#!/bin/bash
set -e

# Source configuration
SOURCE_USER="user@old.com"
SOURCE_SERVER="imap.old.com"
SOURCE_PASS="$(cat /secure/old-pass.txt)"

# Destination configuration
DEST_USER="user@new.com"
DEST_SERVER="imap.new.com"
DEST_PASS="$(cat /secure/new-pass.txt)"

# Run migration
dotnet run --project ImapMove/ImapMove.csproj -- \
  "$SOURCE_USER" "$SOURCE_SERVER" "$SOURCE_PASS" \
  "$DEST_USER" "$DEST_SERVER" "$DEST_PASS"

echo "Migration completed successfully"
```

### PowerShell Script

```powershell
$ErrorActionPreference = "Stop"

# Source configuration
$sourceUser = "user@old.com"
$sourceServer = "imap.old.com"
$sourcePass = Get-Content "C:\secure\old-pass.txt"

# Destination configuration
$destUser = "user@new.com"
$destServer = "imap.new.com"
$destPass = Get-Content "C:\secure\new-pass.txt"

# Run migration
dotnet run --project ImapMove\ImapMove.csproj -- `
  $sourceUser $sourceServer $sourcePass `
  $destUser $destServer $destPass

Write-Host "Migration completed successfully"
```

### Cron Job

```cron
# Daily backup at 2 AM
0 2 * * * cd /opt/btsx && dotnet run --project ImapMove/ImapMove.csproj -- user@mail.com imap.mail.com pass123 user@backup.com imap.backup.com pass456 >> /var/log/btsx-backup.log 2>&1
```

## Next Steps

See [Parameters](parameters/) for detailed information on all available command-line options.
