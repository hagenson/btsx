---
title: "Installation"
weight: 4
---

# Installation

BTSX can be deployed in multiple ways to suit your infrastructure and requirements. Whether you're running on a local machine, in a container, or on Kubernetes, we've got you covered.

## Installation Options

Choose the deployment method that best fits your needs:

### [Local Installation](local/)

Install and run BTSX directly on your local machine or server.

**Best for**:
- Personal use
- Development and testing
- Single-server deployments
- Quick setup and evaluation

**Requirements**:
- .NET 8.0 SDK or Runtime
- Node.js (for web interface frontend)

### [Kubernetes Deployment](kubernetes/)

Deploy BTSX as a containerized application in your Kubernetes cluster.

**Best for**:
- Production environments
- High availability requirements
- Scalable deployments
- Enterprise infrastructure

**Requirements**:
- Kubernetes cluster (1.19+)
- kubectl configured
- Container registry access

## Quick Comparison

| Feature | Local | Kubernetes |
|---------|-------|------------|
| Setup Time | Minutes | 15-30 minutes |
| Scalability | Single instance | Multiple replicas |
| High Availability | No | Yes |
| Resource Management | Manual | Automatic |
| Updates | Manual | Rolling updates |
| Monitoring | Basic logs | Full observability |

## Prerequisites

### All Installations

- Network access to source and destination IMAP servers
- Valid IMAP credentials for both servers
- Sufficient storage for temporary data

### Local Installation

- **Operating System**: Windows, Linux, or macOS
- **.NET 8.0**: SDK for building, Runtime for running
- **Node.js 18+**: For building the web frontend
- **Git**: For cloning the repository

### Kubernetes Deployment

- **Kubernetes Cluster**: Version 1.19 or later
- **kubectl**: Configured to access your cluster
- **Helm** (optional): For simplified deployment
- **Container Registry**: For storing Docker images

## Architecture Overview

BTSX consists of multiple components:

```
┌─────────────────────┐
│   Web Frontend      │  Vue.js + TypeScript
│   (Static Files)    │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   ASP.NET Core      │  Web API + SignalR
│   Web Application   │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   btsx Library      │  Core migration logic
│   (MailKit)         │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   IMAP Servers      │  Source & Destination
└─────────────────────┘
```

## Deployment Considerations

### Security

- **Encryption**: BTSX encrypts passwords and OAuth tokens at rest
- **HTTPS**: Use HTTPS/TLS for production deployments
- **Secrets Management**: Use environment variables or secret managers
- **Network Isolation**: Deploy in private networks when possible

### Performance

- **Memory**: Minimum 512MB RAM, 2GB+ recommended for large migrations
- **CPU**: 1+ cores, 2+ cores for better performance
- **Network**: Stable connection to IMAP servers required
- **Storage**: Minimal disk space needed (mostly in-memory processing)

### Scalability

- **Local**: Single instance, suitable for personal use
- **Kubernetes**: Horizontal scaling for multiple concurrent migrations

## Configuration

### Environment Variables

Required for all deployments:

```bash
# Encryption key for securing passwords and tokens
ENCRYPTION_KEY="your-base64-encoded-key"

# Optional: Override default ports
ASPNETCORE_URLS="http://0.0.0.0:5000"
```

### OAuth Configuration (Optional)

For Google OAuth support:

```bash
# Google OAuth credentials
GOOGLE_CLIENT_ID="your-client-id"
GOOGLE_CLIENT_SECRET="your-client-secret"
```

## Next Steps

Choose your installation method:

1. **[Local Installation](local/)** - Get started quickly on your machine
2. **[Kubernetes Deployment](kubernetes/)** - Deploy to your cluster

## Support

Having installation issues? Check:

- Prerequisites are met
- Firewall allows necessary connections
- .NET and Node.js versions are correct
- Environment variables are properly set

For additional help, visit our [GitHub repository](https://github.com/fhagenson/btsx).
