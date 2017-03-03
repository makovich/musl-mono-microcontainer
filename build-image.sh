#!/bin/sh

echo "Using \`local\` as a prefix for the images"
repository="local"

docker rmi $repository/musl-mono-microcontainer-build || true
docker rmi $repository/musl-mono-microcontainer || true

docker build --rm -t=$repository/musl-mono-microcontainer-build -f docker/Dockerfile.build .
docker run --rm $repository/musl-mono-microcontainer-build cat runtime.tar > docker/runtime.tar

docker build --rm -t=$repository/musl-mono-microcontainer -f docker/Dockerfile.runtime docker
docker rmi $repository/musl-mono-microcontainer-build
