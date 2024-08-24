# Docker Multi-Profile Demo

## Running Docker Compose

Navigate to the solution folder before running the commands.

### Debug

To build and run the containers in debug mode, use:

```sh
docker-compose -f docker-compose.yml -f docker-compose.debug.yml up --build
```

To use it without docker but with docker image then use this command:

```sh
docker run -d --name dockermultiprofiledemo.db.nodocker.dev `
>   -e POSTGRES_DB=dockermultiprofiledemo `
>   -e POSTGRES_USER=postgres `
>   -e POSTGRES_PASSWORD=postgres `
>   -p 5432:5432 `
>   postgres:latest
```

### Release

To build and run the containers in release mode, use:

```sh
docker-compose -f docker-compose.yml -f docker-compose.release.yml up --build --force-recreate
```

#### Configuring Local Hostnames on Windows

To configure a local hostname for your application, follow these steps:

1. **Open Notepad or another text editor as Administrator**:
   - Search for "Notepad" in the Start menu.
   - Right-click on "Notepad" and select "Run as administrator".

2. **Open the `hosts` file**:
   - In Notepad, go to `File` -> `Open`.
   - Navigate to `C:\Windows\System32\drivers\etc\`.
   - Select `All Files (*.*)` in the file type dropdown.
   - Open the file named `hosts`.

3. **Add the following line**:
   ```plaintext
   127.0.0.1 myapp.local
   ```

### Run test

Navigate to the solution folder before running the commands.

```sh
.\integration-test.ps1
```