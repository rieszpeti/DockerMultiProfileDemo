# Check if the script is running with administrator privileges
If (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "Please run the script as an administrator!"
    Exit 1
}

# Prompt the user for the .env file path
$envFilePath = Read-Host "Please enter the path to the .env file (leave empty for default path)"

# Use the default path if the user does not provide one
if ([string]::IsNullOrWhiteSpace($envFilePath)) {
    $envFilePath = Join-Path -Path (Split-Path -Path $MyInvocation.MyCommand.Path -Parent) -ChildPath ".env"
}

# Function to load environment variables from .env file
function Load-Dotenv {
    param (
        [string]$path
    )
    $envVariables = @{}
    if (Test-Path $path) {
        Get-Content $path | ForEach-Object {
            if ($_ -match '^\s*([^#\s][^\s=]*)\s*=\s*(.*)\s*$') {
                $envVariables[$matches[1]] = $matches[2]
            }
        }
        return $envVariables
    } else {
        Write-Host "The .env file was not found."
        Exit 1
    }
}

# Load environment variables from the .env file
$envVars = Load-Dotenv -path $envFilePath

# Retrieve environment variables
$baseAddress = $envVars["DOMAIN_NAME"]
$csharpAddress = $envVars["DOMAIN_CSHARP_PREFIX"] + "." + $baseAddress
$pythonAddress = $envVars["DOMAIN_PYTHON_PREFIX"] + "." + $baseAddress
$dashboardAddress = $envVars["DOMAIN_DASHBOARD_PREFIX"] + "." + $baseAddress
$ipAddress = "127.0.0.1" # Example IP address

# Prepare hosts entries
$hostsEntries = @(
    "$ipAddress $csharpAddress"
    "$ipAddress $pythonAddress"
    "$ipAddress $dashboardAddress"
)

$hostsFilePath = "C:\Windows\System32\drivers\etc\hosts"

# Update hosts file
foreach ($entry in $hostsEntries) {
    if (-Not (Select-String -Path $hostsFilePath -Pattern $entry)) {
        Add-Content -Path $hostsFilePath -Value $entry
        Write-Host "Entry added: $entry"
    } else {
        Write-Host "Entry already exists: $entry"
    }
}

# Run the Docker command
docker-compose -f docker-compose.yml -f docker-compose.release.yml up --build --force-recreate -d

dotnet test ./Integrationtest/IntegrationTest.csproj

# Run .NET tests
Write-Host "Running .NET tests..."
$testResult = dotnet test ./Integrationtest/IntegrationTest.csproj

# Check the exit status of dotnet test
if ($LASTEXITCODE -eq 0) {
    Write-Host "Tests passed successfully."
} else {
    Write-Host "Tests failed."
    Exit 1
}