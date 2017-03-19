########################
# THE BUILD!
########################

Push-Location $PSScriptRoot
Import-Module $PSScriptRoot\Build\Autofac.Build.psd1 -Force

$artifactsPath = "$PSScriptRoot\artifacts"
$packagesPath = "$artifactsPath\packages"

if (Test-Path $artifactsPath) {
	Write-Host "[BUILD] Cleaning $artifactsPath folder" -ForegroundColor Cyan
	Remove-Item $artifactsPath -Force -Recurse
}

# Write out dotnet information
& dotnet --info

# Set version suffix
$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$env:APPVEYOR_REPO_BRANCH -ne $NULL];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$versionSuffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "master" -and $revision -ne "local"]

Write-Host "[BUILD] Package version suffix is $versionSuffix" -ForegroundColor Cyan

# Package restore
Write-Host "[BUILD] Restoring packages" -ForegroundColor Cyan
Get-DotNetProjectDirectory -RootPath $PSScriptRoot | Restore-DependencyPackages

# Build/package
Write-Host "[BUILD] Building projects and packages" -ForegroundColor Cyan
Get-DotNetProjectDirectory -RootPath $PSScriptRoot\src | Invoke-DotNetPack -PackagesPath $packagesPath -VersionSuffix $versionSuffix

# Test
Write-Host "[BUILD] Executing unit tests" -ForegroundColor Cyan
Get-DotNetProjectDirectory -RootPath $PSScriptRoot\test | Invoke-Test

# Finished
Write-Host "[BUILD] Build finished" -ForegroundColor Cyan
Pop-Location
