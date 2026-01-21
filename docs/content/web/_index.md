---
title: "Web Interface"
weight: 2
---

# Web Interface

The BTSX Web Interface provides a user-friendly way to configure and monitor email migrations. Built with ASP.NET Core and Vue.js, it offers real-time progress tracking and persistent job management.

## Features

- **Intuitive Form-Based Configuration**: Simple forms for entering server credentials
- **OAuth Authentication**: Secure login with Google and other OAuth providers
- **Real-time Progress Updates**: Watch your migration progress live with SignalR
- **Job Persistence**: Save jobs to resume later
- **Encrypted Credentials**: All passwords and tokens are encrypted at rest
- **Progress Visualization**: Visual progress bars and detailed statistics
- **Responsive Design**: Works on desktop and mobile devices

## Quick Start

1. Start the web application
2. Fill in source and destination server details
3. Click "Start Migration"
4. Monitor progress in real-time

## Pages

- **[Getting Started](start/)** - Configure and start your first migration
- **[Status Monitoring](status/)** - Understanding the status page and progress indicators

## Screenshots

![Web Interface Overview](screenshots/web-overview.png)

## Technical Details

The web interface uses:

- **Backend**: ASP.NET Core with SignalR for real-time updates
- **Frontend**: Vue 3 with TypeScript
- **Styling**: Bootstrap 5.3
- **Security**: AES-256 encryption for sensitive data
