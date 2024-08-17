# Start the containers defined in docker-compose.yml in detached mode (background).
docker-compose -f docker-compose.yml -f docker-compose.release.yml up --build -d

# Run the integration tests.
dotnet test ./Integrationtest/IntegrationTest.csproj

# Stop and remove the containers.
docker-compose down
