#!/bin/bash
# Setup script for Hugo Book theme

echo "Setting up Hugo Book theme..."

# Check if git is available
if ! command -v git &> /dev/null; then
    echo "Error: git is not installed or not in PATH"
    exit 1
fi

# Get the directory of this script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Add Hugo Book theme as submodule
if [ -d "themes/hugo-book/.git" ]; then
    echo "Theme already installed, updating..."
    git submodule update --init --recursive
else
    echo "Installing Hugo Book theme..."
    git submodule add https://github.com/alex-shpak/hugo-book.git themes/hugo-book
    git submodule update --init --recursive
fi

echo ""
echo "Hugo Book theme setup complete!"
echo ""
echo "Next steps:"
echo "  1. Install Hugo Extended: https://gohugo.io/installation/"
echo "  2. Run 'hugo server -D' to preview the site"
echo "  3. Run 'hugo' to build the site"
