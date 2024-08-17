# Docker Multi-Profile Demo

## Running Docker Compose

Navigate to the solution folder before running the commands.

### Debug

To build and run the containers in debug mode, use:

```sh
docker-compose -f docker-compose.yml -f docker-compose.debug.yml up --build


### Release

To build and run the containers in release mode, use:

```sh
docker-compose -f docker-compose.yml -f docker-compose.release.yml up --build

### Run test

Navigate to the solution folder before running the commands.

```sh
.\integration-test.ps1