<#
Dev sideload loop in one step: build a dev-signed MSIX and install it over the current dev package.

  .\scripts\deploy-dev.ps1               # Debug x64, version = max(manifest, installed dev) + 1 revision
  .\scripts\deploy-dev.ps1 -NoInstall    # build + sign only
  .\scripts\deploy-dev.ps1 -Thumbprint <sha1>

Needs a code-signing cert with subject "CN=GlazeWMDock Dev" in Cert:\CurrentUser\My, trusted once in
Cert:\LocalMachine\TrustedPeople (README, "CLI alternative"). The committed manifest carries the Store
identity: the dev publisher + version are swapped in for the build only, then the file is restored.
#>
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [ValidateSet('x64', 'ARM64')][string]$Platform = 'x64',
    [string]$Thumbprint,
    [switch]$NoInstall
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$project = Join-Path $root 'GlazeWMDock\GlazeWMDock.csproj'
$manifest = Join-Path $root 'GlazeWMDock\Package.appxmanifest'
$devPublisher = 'CN=GlazeWMDock Dev'

if (-not $Thumbprint) {
    $cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $devPublisher -and $_.HasPrivateKey } |
        Sort-Object NotAfter -Descending | Select-Object -First 1
    if (-not $cert) { throw "No '$devPublisher' cert with a private key in Cert:\CurrentUser\My (README, CLI alternative)" }
    $Thumbprint = $cert.Thumbprint
}

# Appx cmdlets aren't native to pwsh 7
if ($PSVersionTable.PSEdition -eq 'Core') { Import-Module Appx -UseWindowsPowerShell -WarningAction SilentlyContinue }

$original = [IO.File]::ReadAllBytes($manifest)
$hasBom = $original.Length -ge 3 -and $original[0] -eq 0xEF -and $original[1] -eq 0xBB -and $original[2] -eq 0xBF
$text = [IO.File]::ReadAllText($manifest)
$identity = [regex]::Match($text, '<Identity\b[^>]*>')
$name = [regex]::Match($identity.Value, 'Name="([^"]+)"').Groups[1].Value
$base = [version][regex]::Match($identity.Value, 'Version="([^"]+)"').Groups[1].Value

$installed = Get-AppxPackage -Name $name | Where-Object Publisher -eq $devPublisher
$top = @($base) + @($installed | ForEach-Object { [version]$_.Version }) | Sort-Object -Descending | Select-Object -First 1
$version = [version]::new($top.Major, $top.Minor, $top.Build, [Math]::Max($top.Revision, 0) + 1)

$devIdentity = $identity.Value -replace 'Publisher="[^"]*"', "Publisher=`"$devPublisher`"" -replace 'Version="[^"]*"', "Version=`"$version`""
"$name $base → $version (installed dev: $(if ($installed) { $installed.Version } else { 'none' }))"

try {
    [IO.File]::WriteAllText($manifest, $text.Remove($identity.Index, $identity.Length).Insert($identity.Index, $devIdentity), [Text.UTF8Encoding]::new($hasBom))
    dotnet build $project -c $Configuration -p:Platform=$Platform -p:GenerateAppxPackageOnBuild=true `
        -p:AppxPackageSigningEnabled=true -p:PackageCertificateThumbprint=$Thumbprint -p:AppxBundle=Never
    if ($LASTEXITCODE) { throw "dotnet build failed ($LASTEXITCODE)" }
}
finally {
    # Restore byte-for-byte so unrelated manifest edits survive
    [IO.File]::WriteAllBytes($manifest, $original)
}

$msix = Get-ChildItem (Join-Path $root 'GlazeWMDock\AppPackages') -Recurse -Filter "*_$($version)_$($Platform)*.msix" |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $msix) { throw "No .msix for $version under GlazeWMDock\AppPackages" }
"$($msix.FullName) (signature: $((Get-AuthenticodeSignature $msix.FullName).Status))"
if ($NoInstall) { return }

# CmdPal keeps the old COM server alive → 0x80073D02 without this
Get-Process GlazeWMDock -ErrorAction SilentlyContinue | Stop-Process -Force
Add-AppxPackage -Path $msix.FullName -ForceApplicationShutdown
"Installed $((Get-AppxPackage -Name $name | Where-Object Publisher -eq $devPublisher).Version). Run Reload in Command Palette."
