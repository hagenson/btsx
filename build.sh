#!/bin/bash -e
ver=$1
if [ "$ver" == "" ]; then
  echo "Defaulting version number using today's date and time.";
  ver=$(date +%Y.%m.%d.%H%M);
fi
echo "Loging into docker hub..."
docker login registry.hub.docker.com

echo "Building version $ver"
docker build -t registry.hub.docker.com/hagenson/btsxweb:latest -t registry.hub.docker.com/hagenson/btsxweb:$ver .
docker push registry.hub.docker.com/hagenson/btsxweb:$ver
docker push registry.hub.docker.com/hagenson/btsxweb:latest
