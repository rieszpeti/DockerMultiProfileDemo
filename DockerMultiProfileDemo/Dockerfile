# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
RUN echo "Base image setup completed."

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION

WORKDIR /src
COPY ["DockerMultiProfileDemo/DockerMultiProfileDemo.csproj", "DockerMultiProfileDemo/"]
RUN echo "Restoring dependencies for DockerMultiProfileDemo.csproj" && \
    dotnet restore "./DockerMultiProfileDemo/DockerMultiProfileDemo.csproj"
COPY . .
WORKDIR "/src/DockerMultiProfileDemo"
RUN echo "Building DockerMultiProfileDemo.csproj with configuration $BUILD_CONFIGURATION" && \
    dotnet build "./DockerMultiProfileDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Certificate Generator Stage
FROM build AS certificate-generator
ARG BUILD_CONFIGURATION
ARG PASSWORD_ENV_SEEDED
RUN echo "Certificate Generator Stage: Build configuration is $BUILD_CONFIGURATION" && \
    if [ "$BUILD_CONFIGURATION" = "Release" ]; then \
        echo "Generating HTTPS certificate." && \
        mkdir -p /https && \
        dotnet dev-certs https -ep /https/aspnetapp.pfx -p "$PASSWORD_ENV_SEEDED" && \
        ls -l /https; \
    else \
        echo "Not in Release configuration, skipping certificate generation"; \
    fi

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION
RUN echo "Publishing DockerMultiProfileDemo.csproj with configuration $BUILD_CONFIGURATION" && \
    dotnet publish "./DockerMultiProfileDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN echo "Published app copied to final stage."

# Copy HTTPS certificate if available
COPY --from=certificate-generator /https* /https
RUN echo "HTTPS certificate copied from certificate-generator stage."

ARG BUILD_CONFIGURATION
# Temporarily switch to root to change permissions
USER root 

RUN echo "Setting permissions for HTTPS certificate." && \
    if [ "$BUILD_CONFIGURATION" = "Release" ]; then \
        chmod 755 /https && chmod 755 /https/aspnetapp.pfx && \
        echo "Permissions set for /https/aspnetapp.pfx"; \
    else \
        echo "Not in Release configuration, skipping permission changes for certificate"; \
    fi

# Revert to non-root user if necessary
USER app
RUN echo "Switched back to non-root user."

ENTRYPOINT ["dotnet", "DockerMultiProfileDemo.dll"]
RUN echo "Entry point set to dotnet DockerMultiProfileDemo.dll"
