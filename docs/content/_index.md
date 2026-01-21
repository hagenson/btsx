---
title: "Overview"
weight: 1
---

# BTSX - Email Migration Tool

BTSX is a powerful email migration tool that helps you move emails between IMAP servers with ease. Whether you're migrating personal mailboxes or managing enterprise-level migrations, BTSX provides both command-line and web-based interfaces to suit your needs.

## Key Features

- **Complete Email Migration**: Migrate all emails, folders, and flags between IMAP servers
- **Folder Hierarchy Preservation**: Maintains the complete folder structure from source to destination
- **Flag Support**: Preserves email flags (read/unread, important, etc.)
- **Two Interfaces**: Choose between CLI for automation or Web UI for ease of use
- **Real-time Progress**: Monitor migration progress with live status updates
- **OAuth Support**: Secure authentication with Google and other OAuth providers
- **Persistent Jobs**: Save migration jobs for later execution

## Getting Started

Choose your preferred interface:

- **[Web Interface](web/)** - User-friendly web UI with real-time progress tracking
- **[Command Line Interface](cli/)** - Scriptable CLI for automation and batch operations

## Installation

Get BTSX up and running:

- **[Local Installation](installation/local/)** - Run BTSX on your local machine
- **[Kubernetes Deployment](installation/kubernetes/)** - Deploy BTSX in your Kubernetes cluster

## Use Cases

- Personal email migrations between providers
- Enterprise mailbox migrations
- Backup and restore operations
- Email consolidation projects
- Server migration support

## Architecture

BTSX consists of:

- **btsx Library**: Core migration logic built with .NET and MailKit
- **ImapMove CLI**: Console application for command-line usage
- **btsxweb**: ASP.NET Core web application with Vue.js frontend

## Support

For issues, questions, or contributions, visit our [GitHub repository](https://github.com/fhagenson/btsx).
