#!/usr/bin/env pwsh
# Build Hugo documentation and copy to wwwroot/help

$ErrorActionPreference = "Stop"

Write-Host "Building Hugo documentation..." -ForegroundColor Cyan

Push-Location docs
try {
    hugo --cleanDestinationDir
    if ($LASTEXITCODE -ne 0) {
        throw "Hugo build failed"
    }
    Write-Host "Hugo build completed successfully" -ForegroundColor Green
}
finally {
    Pop-Location
}

$destination = "btsxweb/wwwroot/help"
Write-Host "Copying Hugo output to $destination..." -ForegroundColor Cyan

if (Test-Path $destination) {
    Remove-Item -Recurse -Force $destination
}

Copy-Item -Recurse -Force "docs/public" $destination

Write-Host "Documentation build complete!" -ForegroundColor Green
