---
title: "Installation"
weight: 1
---

# Installation

## Prerequisites

### Backend (.NET)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Frontend (Web UI)
- [Node.js](https://nodejs.org/) version 16 or later
- npm (included with Node.js)

## Installation Steps

### 1. Clone the Repository

```bash
git clone https://github.com/fhagenson/btsx.git
cd btsx
```

### 2. Restore .NET Dependencies

```bash
dotnet restore BTSX.sln
```

### 3. Install Frontend Dependencies (Web UI only)

```bash
cd btsxweb
npm install
cd ..
```

### 4. Configure Environment (Web UI only)

Set the required encryption key environment variable:

**Windows (PowerShell):**
```powershell
$env:ENCRYPTION_KEY = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

**Linux/macOS:**
```bash
export ENCRYPTION_KEY="$(openssl rand -base64 32)"
```

## Build

### Console Application

```bash
dotnet build BTSX.sln
```

### Web Application

Build backend:
```bash
dotnet build BTSX.sln
```

Build frontend:

**Windows:**
```powershell
.\btsxweb\build-frontend.ps1
```

**Linux/macOS:**
```bash
./btsxweb/build-frontend.sh
```

Or manually:
```bash
cd btsxweb
npm run build
cd ..
```

## Verification

Verify the installation by running:

```bash
dotnet --version
```

This should display .NET 8.0 or later.
