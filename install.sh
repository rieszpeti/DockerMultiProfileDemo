#!/bin/bash

# ChatGPT generated, not tested

# Function to check if the script is running with root privileges
check_root() {
    if [[ "$EUID" -ne 0 ]]; then
        echo "Please run the script as root!"
        exit 1
    fi
}

# Function to load environment variables from .env file
load_dotenv() {
    local path=$1
    if [[ -f "$path" ]]; then
        while IFS='=' read -r key value; do
            if [[ ! "$key" =~ ^\s*# && ! -z "$key" && ! -z "$value" ]]; then
                export "$key"="$value"
            fi
        done < "$path"
    else
        echo "The .env file was not found."
        exit 1
    fi
}

# Check if the script is running with root privileges
check_root

# Prompt the user for the .env file path
read -p "Please enter the path to the .env file (leave empty for default path): " envFilePath

# Use the default path if the user does not provide one
if [[ -z "$envFilePath" ]]; then
    envFilePath="$(dirname "$0")/.env"
fi

# Load environment variables from the .env file
load_dotenv "$envFilePath"

# Retrieve environment variables
baseAddress=${DOMAIN_NAME}
csharpAddress="${DOMAIN_CSHARP_PREFIX}.${baseAddress}"
pythonAddress="${DOMAIN_PYTHON_PREFIX}.${baseAddress}"
dashboardAddress="${DOMAIN_DASHBOARD_PREFIX}.${baseAddress}"
ipAddress="127.0.0.1" # Example IP address

# Prepare hosts entries
hostsEntries=(
    "$ipAddress $csharpAddress"
    "$ipAddress $pythonAddress"
    "$ipAddress $dashboardAddress"
)

hostsFilePath="/etc/hosts"

# Update hosts file
for entry in "${hostsEntries[@]}"; do
    if ! grep -q "$entry" "$hostsFilePath"; then
        echo "$entry" | sudo tee -a "$hostsFilePath" > /dev/null
        echo "Entry added: $entry"
    else
        echo "Entry already exists: $entry"
    fi
done

# Run the Docker command
docker-compose -f docker-compose.yml -f docker-compose.release.yml up --build --force-recreate
