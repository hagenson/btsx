---
title: "Troubleshooting"
weight: 5
---

# Troubleshooting

## Common Issues

### Authentication Failures

#### "Authentication failed" error

**Cause:** Invalid credentials or server settings

**Solutions:**
1. Verify username/email is correct
2. Check password (may need app-specific password)
3. Confirm IMAP server hostname
4. Ensure IMAP is enabled on the account

#### Gmail-specific issues

- **Two-Factor Authentication:** Use App Password instead of regular password
  - Go to Google Account → Security → 2-Step Verification → App passwords
  - Generate a new app password for "Mail"
  - Use the generated password instead of your regular password

- **Less secure apps:** Gmail may block IMAP access
  - Use OAuth authentication instead (Web UI)
  - Or enable "Less secure app access" (not recommended)

### Connection Issues

#### "Could not connect to server" error

**Cause:** Network connectivity or firewall blocking

**Solutions:**
1. Check internet connection
2. Verify firewall allows IMAP ports (typically 993)
3. Test server connectivity: `telnet imap.server.com 993`
4. Check if server requires VPN or special network access

#### SSL/TLS certificate errors

**Cause:** Certificate validation failure

**Solutions:**
1. Ensure system time is correct
2. Update root certificates
3. Check for corporate proxy/firewall interference

### Performance Issues

#### Migration is very slow

**Cause:** Large mailbox size or network speed

**Solutions:**
1. Run migration during off-peak hours
2. Check network bandwidth
3. Consider migration in batches
4. Disable progress updates to reduce overhead

#### Application becomes unresponsive

**Cause:** Large number of messages or folders

**Solutions:**
1. Increase application timeout settings
2. Monitor system resources (CPU, memory, disk)
3. Close other applications
4. Consider using console application for large migrations

### Web UI Issues

#### Frontend not loading

**Cause:** Frontend not built or build output missing

**Solutions:**
1. Build frontend: `cd btsxweb && npm run build`
2. Verify `btsxweb/wwwroot/dist/` exists and contains files
3. Check browser console for errors

#### Real-time updates not working

**Cause:** SignalR connection failure

**Solutions:**
1. Check browser console for WebSocket errors
2. Verify firewall allows WebSocket connections
3. Try disabling browser extensions
4. Check server logs for SignalR errors

#### OAuth callback fails

**Cause:** Misconfigured redirect URI

**Solutions:**
1. Verify redirect URI in Google Cloud Console matches application
2. Ensure HTTPS is used in production
3. Check `appsettings.json` configuration
4. Review OAuth consent screen settings

### Encryption Issues

#### "Encryption key not set" error

**Cause:** ENCRYPTION_KEY environment variable not configured

**Solution:**
```powershell
# Windows PowerShell
$env:ENCRYPTION_KEY = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# Linux/macOS
export ENCRYPTION_KEY="$(openssl rand -base64 32)"
```

#### "Failed to decrypt credentials" error

**Cause:** ENCRYPTION_KEY changed since credentials were stored

**Solutions:**
1. Clear job storage: Delete files in `btsxweb/JobStorage/`
2. Restart application with original ENCRYPTION_KEY
3. Re-enter credentials

### Data Issues

#### Some messages not copied

**Cause:** Message size limits or corruption

**Solutions:**
1. Check server logs for specific errors
2. Verify destination server message size limits
3. Check source messages for corruption
4. Consider manual copying of problematic messages

#### Folder structure not preserved

**Cause:** Folder naming conflicts or special characters

**Solutions:**
1. Check for folders with special characters
2. Verify destination server folder naming rules
3. Review migration logs for folder creation errors

## Logging and Debugging

### Enable Detailed Logging

Modify `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "btsx": "Trace",
      "MailKit": "Debug"
    }
  }
}
```

### Check Application Logs

**Console Application:** Output is displayed in terminal

**Web Application:** Check console output or configure file logging

### Network Diagnostics

Test IMAP connectivity:

```bash
# Test connection
telnet imap.server.com 993

# Or using OpenSSL
openssl s_client -connect imap.server.com:993
```

## Getting Help

If issues persist:

1. Check [GitHub Issues](https://github.com/fhagenson/btsx/issues)
2. Review server-specific documentation
3. Enable debug logging and capture logs
4. Create a new issue with:
   - BTSX version
   - Server types (Gmail, Outlook, etc.)
   - Error messages and logs
   - Steps to reproduce

## Known Limitations

- OAuth currently only supports Google
- Progress updates add overhead for very large mailboxes
- Source messages are deleted after successful copy
- No built-in rollback mechanism
