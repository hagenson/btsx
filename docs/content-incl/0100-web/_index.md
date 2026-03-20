---
title: "Web Interface"
weight: 0100
---

# Web Interface

The BTSX Web Interface provides a user-friendly way to configure and run data migrations.
Built with ASP.NET Core and Vue.js, it offers real-time progress tracking and persistent job management.

## Features

- **Unified Migration Interface**: Single interface with migration type selector for all migration types.
- **Multiple Migration Types**: Support for Mail migrations and Contacts migrations.
- **Intuitive Form-Based Configuration**: Simple forms for entering server credentials.
- **OAuth Authentication**: Secure login with Google and other OAuth providers.
- **Real-time Progress Updates**: Status updates are streamed back to the browser.
- **Job Persistence**: Long running migrations will restart if the server is restarted before completion.
- **Encrypted Credentials**: All passwords and tokens are encrypted at rest.
- **Responsive Design**: Works on desktop and mobile devices

## Running a Migration

### Mail Migrations

- **[Getting Started](start/)** - Configure and start your first mail migration.
- **[Status Monitoring](status/)** - Understanding the status page and progress indicators.

### Contacts Migrations

- **[Contacts Migration Guide](contacts/)** - Configure and run contacts migrations.
- **[Contacts Status Monitoring](contacts-status/)** - Monitor your contacts migration progress.

## Technical Details

The web interface uses:

- **Backend**: ASP.NET Core with SignalR for real-time updates
- **Frontend**: Vue 3 with TypeScript
- **Styling**: Bootstrap 5.3
- **Security**: AES-256 encryption for sensitive data
