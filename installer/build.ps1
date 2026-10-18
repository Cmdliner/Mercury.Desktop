# Builds the release: publishes the app as one self-contained exe, then compiles the installer
# with Inno Setup. Run from the repository root:  powershell -File installer\build.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

Write-Host 'Publishing...'
dotnet publish Mercury.Desktop.csproj -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
if ($LASTEXITCODE -ne 0) { throw 'publish failed' }

$iscc = @(
  "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
  "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
  "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 is not installed. Get it from https://jrsoftware.org/isdl.php' }

Write-Host 'Building the installer...'
& $iscc 'installer\Mercury.iss'
if ($LASTEXITCODE -ne 0) { throw 'installer build failed' }
Write-Host "Done: $root\dist"
