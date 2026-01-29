---
title: "Overview"
weight: 0001
---

# BTSX - Email Migration Tool

BTSX is a email migration tool that helps you transfer email account between servers with ease.
BTSX provides both command-line and web-based interfaces to suit your needs.

## Key Features

- **Complete Email Migration**: Migrate all emails, folders, and flags between servers
- **Folder Hierarchy Preservation**: Maintains the complete folder structure from source to destination
- **Two Interfaces**: Choose between CLI for automation or Web UI for ease of use
- **Real-time Progress**: Monitor migration progress with live status updates
- **OAuth Support**: Secure authentication with Google and other OAuth providers
- **Persistent Jobs**: Save migration jobs for later execution

## Getting Started

Choose your preferred interface:

- **[Web Interface](web/)** - User-friendly web UI with real-time progress tracking
- **[Command Line Interface](cli/)** - Scriptable CLI for automation and batch operations

## Installation

Because email contains your most sensitive information, the source code to BTSX is made freely available.
You can download, inspect the code and build and run your own instances to have complete control over
your own data.

- **[Local Installation](installation/local/)** - Run BTSX on your local machine
- **[Kubernetes Deployment](installation/kubernetes/)** - Deploy BTSX in your Kubernetes cluster

## Architecture

BTSX consists of:

- **btsx Library**: Core migration logic built with .NET and MailKit
- **btsxcli**: Console application for command-line usage
- **btsxweb**: ASP.NET Core web application with Vue.js frontend

## Support

For issues, questions, or contributions, visit our [GitHub repository](https://github.com/hagenson/btsx).
