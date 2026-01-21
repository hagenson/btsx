#!/bin/bash
# Build Hugo documentation and copy to wwwroot/help

set -e

echo "Building Hugo documentation..."

cd docs
hugo --cleanDestinationDir
echo "Hugo build completed successfully"
cd ..

destination="btsxweb/wwwroot/help"
echo "Copying Hugo output to $destination..."

if [ -d "$destination" ]; then
    rm -rf "$destination"
fi

cp -r "docs/public" "$destination"

echo "Documentation build complete!"
