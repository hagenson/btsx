---
title: "Console Application"
weight: 2
---

# Console Application

The BTSX console application provides a command-line interface for performing IMAP email migrations.

## Usage

```bash
dotnet run --project ImapMove/ImapMove.csproj -- <srcUser> <srcServer> <srcPassword> <dstUser> <dstServer> <dstPassword>
```

### Arguments

| Position | Argument | Description |
|----------|----------|-------------|
| 1 | srcUser | Source IMAP username/email |
| 2 | srcServer | Source IMAP server hostname |
| 3 | srcPassword | Source IMAP password |
| 4 | dstUser | Destination IMAP username/email |
| 5 | dstServer | Destination IMAP server hostname |
| 6 | dstPassword | Destination IMAP password |

## Example

```bash
dotnet run --project ImapMove/ImapMove.csproj -- \
  user@source.com \
  imap.source.com \
  "sourcePassword123" \
  user@destination.com \
  imap.destination.com \
  "destPassword456"
```

## Features

- Connects to source and destination IMAP servers
- Retrieves all folders and messages from source
- Recreates folder hierarchy on destination
- Copies messages with flags preserved
- Marks messages as deleted on source and expunges
- Progress notifications with Unicode symbols
- Error handling and recovery

## Migration Process

1. **Connection**: Establishes connections to both IMAP servers
2. **Folder Discovery**: Lists all folders from source server
3. **Folder Creation**: Creates corresponding folders on destination
4. **Message Copy**: Copies each message with its flags
5. **Source Cleanup**: Marks copied messages as deleted
6. **Expunge**: Permanently removes deleted messages from source

## Output

The console application provides detailed progress information:

```
✓ Connected to source server
✓ Connected to destination server
→ Processing folder: INBOX
  ├─ Copying 150 messages...
  └─ ✓ Completed INBOX
→ Processing folder: Sent
  ├─ Copying 45 messages...
  └─ ✓ Completed Sent
✓ Migration completed successfully
```

## Error Handling

If an error occurs during migration:
- The application will display an error message
- Partial migrations may require manual cleanup
- Check server logs for detailed error information
