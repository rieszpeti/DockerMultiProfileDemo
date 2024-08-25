#!/bin/bash

# ChatGPT generated, not tested

# Check if the script is running with root privileges
if [[ "$EUID" -ne 0 ]]; then
    echo "Please run the script as root (administrator)!"
    exit 1
fi

# Prompt the user for the .env file path
read -p "Please enter the path to the .env file (leave empty for default path): " envFilePath

# Use the default path if the user does not provide one
if [[ -z "$envFilePath" ]]; then
    envFilePath="$(dirname "$0")/.env"
fi

# Function to load environment variables from .env file
load_dotenv() {
    local path=$1
    local envFileContent
    declare -A envVariables

    if [[ -f "$path" ]]; then
        while IFS='=' read -r key value; do
            if [[ ! "$key" =~ ^\s*# && ! -z "$key" && ! -z "$value" ]]; then
                # Filter and store only variables starting with "DOMAIN_"
                if [[ "$key" =~ ^DOMAIN_ ]]; then
                    envVariables["$key"]="$value"
                fi
            fi
        done < "$path"
    else
        echo "The .env file was not found."
        exit 1
    fi

    # Print environment variables in a way that they can be used later
    for key in "${!envVariables[@]}"; do
        echo "$key=${envVariables[$key]}"
    done
    echo "${envVariables[@]}" # Return the array values
}

# Load environment variables from the .env file
envVars=$(load_dotenv "$envFilePath")

# Prepare hosts entries based on loaded environment variables
hostsEntries=()
for key in $(echo "$envVars" | awk -F= '{print $1}'); do
    if [[ "$key" != "DOMAIN_NAME" ]]; then
        prefix=$(echo "$envVars" | grep "^$key=" | cut -d'=' -f2)
        baseAddress=$(echo "$envVars" | grep "^DOMAIN_NAME=" | cut -d'=' -f2)
        hostsEntries+=("127.0.0.1 $prefix.$baseAddress")
    fi
done

# Update hosts file
hostsFilePath="/etc/hosts"
for entry in "${hostsEntries[@]}"; do
    # Escape any regex special characters in the entry
    pattern=$(printf "%s" "$entry" | sed 's/[][\.*^$]/\\&/g')
    
    # Check if entry exists in the hosts file
    if ! grep -q "$pattern" "$hostsFilePath"; then
        echo "$entry" >> "$hostsFilePath"
        echo "Entry added: $entry"
    else
        echo "Entry already exists: $entry"
    fi
done

# Run the Docker command
docker-compose -f docker-compose.yml -f docker-compose.release.yml up --build --force-recreate -d

# Run .NET tests
echo "Running .NET tests..."
testResult=$(dotnet test ./Integrationtest/IntegrationTest.csproj)

# Check the exit status of dotnet test
if [[ $? -eq 0 ]]; then
    echo "Tests passed successfully."
else
    echo "Tests failed."
    exit 1
fi
