param(
	[Parameter(Mandatory = $true)]
	[string]$ApiKey,
	
	[Parameter(Mandatory = $true)]
	[string]$Version
)

$Source = "https://api.nuget.org/v3/index.json"

$Projects = @(
    "Inferno.Caching.Abstractions"
    "Inferno.Data.Abstractions"
    "Inferno.Tasks.Abstractions"
    "Inferno"
    "Inferno.Security"
    "Inferno.Tasks"
    "Inferno.Caching"
    "Inferno.Tenants.Abstractions"
    "Inferno.Data"
    "Inferno.Tenants"
    "Inferno.Web"
    "Inferno.Identity"
    "Inferno.Web.ContentManagement"
    "Plugins\Inferno.Plugins.GoogleAnalytics"
    "Plugins\Inferno.Plugins.Messaging.Forumz"
)

foreach ($ProjectDir in $Projects) {
    $Name = Split-Path $ProjectDir -Leaf
    $Project = Join-Path $ProjectDir "$Name.csproj"
    $PackageFile = Join-Path $ProjectDir "bin\Release\$Name.$Version.nupkg"

    Write-Host "`nPacking $Name..."
    dotnet pack $Project -c Release -p:PackageVersion=$Version

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pack $Name"
    }

    if (-not (Test-Path $PackageFile)) {
        throw "Package not found: $PackageFile"
    }

    Write-Host "Pushing $PackageFile..."

    dotnet nuget push $PackageFile --api-key $ApiKey --source $Source

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to push $Name"
    }
}

Write-Host "`nAll packages packed and pushed successfully!"
