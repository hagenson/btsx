#!/bin/bash
# Shell script to build the frontend
# This installs npm dependencies and builds the TypeScript/Vue code

echo "Building btsxweb frontend..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "Error: Node.js is not installed. Please install Node.js from https://nodejs.org/"
    exit 1
fi

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "Error: npm is not installed. Please install npm."
    exit 1
fi

# Change to script directory
cd "$(dirname "$0")"

# Install dependencies if node_modules doesn't exist
if [ ! -d "node_modules" ]; then
    echo "Installing npm dependencies..."
    npm install
    if [ $? -ne 0 ]; then
        echo "Error: npm install failed"
        exit 1
    fi
fi

# Build the frontend
echo "Building TypeScript and Vue components..."
npm run build
if [ $? -ne 0 ]; then
    echo "Error: Build failed"
    exit 1
fi

echo "Frontend build completed successfully!"
echo "Output files are in wwwroot/dist/"
