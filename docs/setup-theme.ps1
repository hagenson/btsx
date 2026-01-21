#!/usr/bin/env pwsh
# Setup script for Hugo Book theme

Write-Host "Setting up Hugo Book theme..." -ForegroundColor Cyan

# Check if git is available
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "Error: git is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

# Check if already in docs directory
$currentDir = (Get-Location).Path
if ($currentDir -notlike "*\docs") {
    Write-Host "Changing to docs directory..." -ForegroundColor Yellow
    Set-Location -Path (Join-Path $PSScriptRoot ".")
}

# Add Hugo Book theme as submodule
if (Test-Path "themes/hugo-book/.git") {
    Write-Host "Theme already installed, updating..." -ForegroundColor Yellow
    git submodule update --init --recursive
} else {
    Write-Host "Installing Hugo Book theme..." -ForegroundColor Yellow
    git submodule add https://github.com/alex-shpak/hugo-book.git themes/hugo-book
    git submodule update --init --recursive
}

Write-Host "Hugo Book theme setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Install Hugo Extended: https://gohugo.io/installation/" -ForegroundColor White
Write-Host "  2. Run 'hugo server -D' to preview the site" -ForegroundColor White
Write-Host "  3. Run 'hugo' to build the site" -ForegroundColor White
