$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $scriptDir
$csprojPath = Join-Path $root 'GKFRLobbyGlitchFix.csproj'
$manifestPath = Join-Path $scriptDir 'manifest.json'
$iconPath = Join-Path $scriptDir 'icon.png'
$readmePath = Join-Path $root 'README.md'
$dllPath = Join-Path $root "bin\release\net46\GKFRLobbyGlitchFix.dll"

# Extract version from csproj:
$versionNode = Select-Xml -Path $csprojPath -XPath '//Version' | Select-Object -First 1
$version = $versionNode.Node.InnerText.Trim()
$output = Join-Path $scriptDir "GKFRLobbyGlitchFix-$version.zip"

# Replace version in manifest.json:
$tempManifestPath = Join-Path $env:TEMP "manifest.json"
(Get-Content $manifestPath -Raw) -replace '@VERSION@', $version | Set-Content -Path $tempManifestPath -Encoding UTF8

# Build zip
if (Test-Path $output) {
    Remove-Item $output -Force
}

Compress-Archive -Path $tempManifestPath, $iconPath, $readmePath, $dllPath -DestinationPath $output -Force

Write-Host "Created package: $output"
