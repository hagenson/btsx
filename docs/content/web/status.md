---
title: "Status Monitoring"
weight: 2
---

# Status Monitoring

The status page provides real-time feedback on your email migration progress. Understanding the different status indicators and metrics helps you monitor and troubleshoot migrations effectively.

## Status Page Overview

After starting a migration, you'll be redirected to the status page where you can monitor progress in real-time.

![Status Page Overview](screenshots/status-overview.png)

## Status Indicators

### Connection Status

Shows the current connection state:

- **Connecting**: Establishing connection to IMAP servers
- **Connected**: Successfully connected to both servers
- **Authenticating**: Verifying credentials
- **Authenticated**: Successfully logged in
- **Failed**: Connection or authentication error

![Connection Status](screenshots/connection-status.png)

### Migration Phase

Indicates the current migration phase:

- **Initializing**: Setting up the migration
- **Scanning Folders**: Retrieving folder structure from source
- **Creating Folders**: Creating folders on destination
- **Copying Messages**: Transferring emails
- **Finalizing**: Completing migration and cleanup
- **Complete**: Migration finished successfully
- **Error**: Migration stopped due to error

![Migration Phase](screenshots/migration-phase.png)

## Progress Indicators

### Progress Bar

Visual representation of migration completion:

- Shows percentage complete
- Updates in real-time as messages are copied
- Color-coded (blue for in-progress, green for complete, red for errors)

![Progress Bar](screenshots/progress-bar.png)

### Message Counter

Displays:
- **Total Messages**: Number of messages to migrate
- **Copied Messages**: Number successfully copied
- **Current Folder**: Which folder is currently being processed

![Message Counter](screenshots/message-counter.png)

## Live Status Messages

Real-time status messages provide detailed information:

```
✓ Connected to source server: imap.oldserver.com
✓ Connected to destination server: imap.newserver.com
→ Scanning folders...
✓ Found 15 folders
→ Creating folder: INBOX/Archive
→ Copying messages from INBOX (1/5000)
→ Copying messages from INBOX (100/5000)
```

![Status Messages](screenshots/status-messages.png)

## Migration Statistics

Upon completion, view detailed statistics:

### Summary Stats

- **Total Folders**: Number of folders processed
- **Total Messages**: Total emails migrated
- **Total Size**: Data transferred (in MB/GB)
- **Duration**: Time taken for migration
- **Average Speed**: Messages per minute

![Summary Statistics](screenshots/summary-stats.png)

### Per-Folder Breakdown

- **Folder Name**: Each folder processed
- **Message Count**: Messages in each folder
- **Size**: Size of each folder
- **Status**: Success or error status

![Folder Breakdown](screenshots/folder-breakdown.png)

## Real-time Updates with SignalR

The status page uses SignalR for real-time updates:

- **Automatic Updates**: No need to refresh the page
- **Low Latency**: Updates appear within seconds
- **Connection Monitoring**: Automatic reconnection if connection drops

### Connection Indicator

Watch for the connection status indicator:

- **Connected** (green): Receiving real-time updates
- **Reconnecting** (yellow): Attempting to reconnect
- **Disconnected** (red): Not receiving updates

![Connection Indicator](screenshots/connection-indicator.png)

## Error Handling

### Error Messages

When errors occur, detailed messages appear:

```
✗ Error copying message: Authentication failed
✗ Error creating folder: Folder already exists
⚠ Warning: Message skipped due to size limit
```

![Error Messages](screenshots/error-messages.png)

### Error Recovery

- Some errors are recoverable (BTSX will retry)
- Fatal errors stop the migration
- Check error messages for troubleshooting guidance

## Monitoring Best Practices

### Keep Page Open

- Keep the status page open during migration
- Browser must remain connected for real-time updates
- Closing the browser doesn't stop the migration (it runs server-side)

### Watch for Errors

- Monitor for error messages
- Red status indicators require attention
- Check network connectivity if updates stop

### Large Migrations

For migrations with thousands of messages:

- Progress may appear slow initially (folder scanning phase)
- Speed varies based on message size and server performance
- Expect 10-100 messages per minute on average

## Resuming Migrations

If a migration is interrupted:

1. Return to the main page
2. If the job was saved, it will appear in the job list
3. Click **"Resume"** to continue from where it stopped

![Resume Job](screenshots/resume-job.png)

## Troubleshooting Status Issues

### Updates Stop Appearing

- Check browser console for JavaScript errors
- Verify SignalR connection (check connection indicator)
- Refresh the page to reconnect
- Migration continues server-side even if page disconnects

### Stuck Progress

- Some folders may take longer (large messages)
- Server performance affects speed
- Network latency impacts throughput
- Wait for status message updates

### Completion Doesn't Show

- Allow time for finalization phase
- Check server logs for backend errors
- Refresh the page if statistics don't appear

## Next Steps

After successful migration:

1. Review the statistics
2. Verify emails in destination account
3. Check folder structure is correct
4. Confirm all important messages migrated
5. If source deletion was enabled, verify source is clean
