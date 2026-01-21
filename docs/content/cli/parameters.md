---
title: "Parameters"
weight: 1
---

# CLI Parameters

Complete reference for all command-line parameters and usage patterns for the BTSX CLI (ImapMove).

## Basic Syntax

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  <srcUser> <srcServer> <srcPassword> \
  <dstUser> <dstServer> <dstPassword>
```

All six parameters are required and must be provided in order.

## Parameter Reference

### 1. Source User (`srcUser`)

**Description**: Username or email address for the source IMAP account

**Format**: 
- Email address: `user@example.com`
- Username: `username` (provider-dependent)

**Example**:
```bash
user@oldmail.com
```

### 2. Source Server (`srcServer`)

**Description**: Hostname or IP address of the source IMAP server

**Format**: 
- Hostname: `imap.example.com`
- IP address: `192.168.1.100`
- With port (optional): `imap.example.com:993`

**Example**:
```bash
imap.oldmail.com
```

**Common Servers**:
- Gmail: `imap.gmail.com`
- Outlook: `outlook.office365.com`
- Yahoo: `imap.mail.yahoo.com`
- iCloud: `imap.mail.me.com`

### 3. Source Password (`srcPassword`)

**Description**: Password or app-specific password for the source account

**Format**: Plain text string (use quotes if contains spaces or special characters)

**Example**:
```bash
"mySecurePassword123!"
```

**Security Notes**:
- Use app-specific passwords for accounts with 2FA
- Avoid storing in shell history
- Consider using environment variables
- Be aware passwords appear in process lists

### 4. Destination User (`dstUser`)

**Description**: Username or email address for the destination IMAP account

**Format**: 
- Email address: `user@example.com`
- Username: `username` (provider-dependent)

**Example**:
```bash
user@newmail.com
```

### 5. Destination Server (`dstServer`)

**Description**: Hostname or IP address of the destination IMAP server

**Format**: 
- Hostname: `imap.example.com`
- IP address: `192.168.1.100`
- With port (optional): `imap.example.com:993`

**Example**:
```bash
imap.newmail.com
```

### 6. Destination Password (`dstPassword`)

**Description**: Password or app-specific password for the destination account

**Format**: Plain text string (use quotes if contains spaces or special characters)

**Example**:
```bash
"myNewPassword456!"
```

## Usage Examples

### Basic Migration

Migrate from Gmail to Outlook:

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@gmail.com imap.gmail.com gmailAppPassword123 \
  user@outlook.com outlook.office365.com outlookPassword456
```

### With Special Characters

Using passwords with special characters:

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  john@oldmail.com imap.oldmail.com "p@ssw0rd!#$" \
  john@newmail.com imap.newmail.com "n3wP@ss&123"
```

### Using Environment Variables

More secure approach:

```bash
# Set environment variables
export SRC_USER="user@old.com"
export SRC_SERVER="imap.old.com"
export SRC_PASS="oldPassword123"
export DST_USER="user@new.com"
export DST_SERVER="imap.new.com"
export DST_PASS="newPassword456"

# Run migration
dotnet run --project ImapMove/ImapMove.csproj -- \
  "$SRC_USER" "$SRC_SERVER" "$SRC_PASS" \
  "$DST_USER" "$DST_SERVER" "$DST_PASS"
```

### Reading from Files

Store passwords in files for better security:

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@old.com imap.old.com "$(cat ~/secrets/old-pass.txt)" \
  user@new.com imap.new.com "$(cat ~/secrets/new-pass.txt)"
```

### PowerShell Example

Using PowerShell on Windows:

```powershell
$srcUser = "user@old.com"
$srcServer = "imap.old.com"
$srcPass = Get-Content "C:\secrets\old-pass.txt" -Raw

$dstUser = "user@new.com"
$dstServer = "imap.new.com"
$dstPass = Get-Content "C:\secrets\new-pass.txt" -Raw

dotnet run --project ImapMove\ImapMove.csproj -- `
  $srcUser $srcServer $srcPass `
  $dstUser $dstServer $dstPass
```

## Port Configuration

### Default Ports

BTSX uses standard IMAP ports by default:
- **993**: IMAP with SSL/TLS (default)
- **143**: IMAP without encryption (not recommended)

### Custom Ports

If your server uses a non-standard port, include it in the server parameter:

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@old.com imap.old.com:9993 password1 \
  user@new.com imap.new.com:9993 password2
```

## Provider-Specific Configuration

### Gmail

Requirements:
- Enable IMAP in Gmail settings
- Use app-specific password if 2FA is enabled
- Less secure app access may need to be enabled

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@gmail.com imap.gmail.com app-specific-password \
  user@newmail.com imap.newmail.com password
```

### Microsoft 365 / Outlook

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@outlook.com outlook.office365.com password1 \
  user@newmail.com imap.newmail.com password2
```

### Yahoo Mail

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@yahoo.com imap.mail.yahoo.com app-password \
  user@newmail.com imap.newmail.com password
```

### iCloud

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@icloud.com imap.mail.me.com app-specific-password \
  user@newmail.com imap.newmail.com password
```

### Self-Hosted / Custom Servers

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  john mail.mycompany.com:993 secretpass123 \
  john mail.newcompany.com:993 newpass456
```

## Validation

### Parameter Validation

BTSX validates:
- All six parameters are provided
- Server hostnames are reachable
- Credentials are valid
- Connections can be established

### Error Messages

Common validation errors:

**Missing Parameters**:
```
Error: Incorrect number of arguments
Usage: ImapMove <srcUser> <srcServer> <srcPassword> <dstUser> <dstServer> <dstPassword>
```

**Connection Failed**:
```
Error: Could not connect to source server: imap.old.com
Check server hostname, port, and firewall settings
```

**Authentication Failed**:
```
Error: Authentication failed for user@old.com
Verify username and password are correct
```

## Troubleshooting

### Common Issues

#### Invalid Credentials

**Problem**: "Authentication failed"

**Solution**:
- Verify username and password
- Check for app-specific password requirements
- Ensure account is not locked
- Verify IMAP is enabled in account settings

#### Connection Timeout

**Problem**: "Connection timeout"

**Solution**:
- Check server hostname is correct
- Verify port number (default 993)
- Check firewall allows IMAP connections
- Test connectivity with telnet or similar tool

#### SSL/TLS Errors

**Problem**: "SSL/TLS connection failed"

**Solution**:
- Ensure server supports SSL/TLS
- Verify correct port (993 for SSL)
- Check server certificate is valid
- Update .NET runtime if needed

#### Special Characters in Passwords

**Problem**: "Authentication failed" with special characters

**Solution**:
- Use quotes around passwords: `"p@ssw0rd!"`
- Escape special characters if needed: `p\@ssw0rd\!`
- Use environment variables to avoid shell interpretation

### Testing Connection

Test connection before full migration:

```bash
# Test with a simple command
echo "Testing connection..."
dotnet run --project ImapMove/ImapMove.csproj -- \
  testuser@old.com imap.old.com testpass \
  testuser@new.com imap.new.com testpass 2>&1 | head -20
```

## Best Practices

### Security

1. **Never hardcode passwords** in scripts or version control
2. **Use environment variables** for credentials
3. **Store passwords in secure files** with restricted permissions
4. **Clear shell history** after running commands with passwords
5. **Use app-specific passwords** when available

### Scripting

1. **Check exit codes** for error handling
2. **Log output** to files for auditing
3. **Use variables** for configuration
4. **Test with dry-run** on small mailboxes first
5. **Implement retries** for transient errors

### Performance

1. **Run during off-peak hours** for large migrations
2. **Monitor network bandwidth** usage
3. **Check server rate limits** before bulk operations
4. **Consider breaking large migrations** into batches

## See Also

- [CLI Overview](../) - Introduction to the CLI
- [Installation](../../installation/local/) - Installing BTSX
- [Web Interface](../../web/) - Alternative web-based interface
