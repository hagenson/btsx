# PowerShell script to build the frontend
# This installs npm dependencies and builds the TypeScript/Vue code

Write-Host "Building btsxweb frontend..." -ForegroundColor Cyan

# Check if Node.js is installed
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "Error: Node.js is not installed. Please install Node.js from https://nodejs.org/" -ForegroundColor Red
    exit 1
}

# Check if npm is installed
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Host "Error: npm is not installed. Please install npm." -ForegroundColor Red
    exit 1
}

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Install dependencies if node_modules doesn't exist
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing npm dependencies..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: npm install failed" -ForegroundColor Red
        exit 1
    }
}

# Build the frontend
Write-Host "Building TypeScript and Vue components..." -ForegroundColor Yellow
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "Frontend build completed successfully!" -ForegroundColor Green
Write-Host "Output files are in wwwroot/dist/" -ForegroundColor Green
