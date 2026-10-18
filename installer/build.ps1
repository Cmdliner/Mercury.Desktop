# Builds the release: a clean build of the app as one self-contained exe, a check that it really
# opens, then the installer. Run from the repository root:
#
#   powershell -ExecutionPolicy Bypass -File installer\build.ps1
#   powershell -ExecutionPolicy Bypass -File installer\build.ps1 -TestInstaller
#
# -TestInstaller also installs the finished installer into a temporary folder and uninstalls it again.
# It refuses to run on a machine that already has Mercury installed, because both use the same install
# record and the test would remove the real one.
param(
    [switch]$TestInstaller
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

# 1. Clean first. A stale incremental build once shipped without the app icon inside it, and the
#    installed app crashed on start. A release is always built from scratch.
Write-Host 'Cleaning...'
dotnet clean Mercury.Desktop.csproj -c Release | Out-Null
if (Test-Path 'publish') { Remove-Item 'publish' -Recurse -Force }

# 2. Publish.
Write-Host 'Publishing...'
dotnet publish Mercury.Desktop.csproj -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
if ($LASTEXITCODE -ne 0) { throw 'publish failed' }

# 3. Does it open? Never ship an installer for an exe that has not been seen to open a window.
Write-Host 'Checking that the app opens...'
& "$PSScriptRoot\smoke-test.ps1" -Exe "$root\publish\Mercury.Desktop.exe"

# 4. The installer.
$iscc = @(
  "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
  "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
  "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 is not installed. Get it from https://jrsoftware.org/isdl.php' }

Write-Host 'Building the installer...'
& $iscc 'installer\Mercury.iss'
if ($LASTEXITCODE -ne 0) { throw 'installer build failed' }
$setup = Join-Path $root 'dist\Mercury-Setup.exe'

# 5. Optionally, try the installer itself.
if ($TestInstaller) {
    $appId = '6F0B8A52-3B8D-4C1B-9E55-7D1C4A3E2F10'
    $installed = @('HKCU', 'HKLM') | Where-Object { Test-Path "${_}:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{$appId}_is1" }
    if ($installed) {
        Write-Warning 'Mercury is already installed here, so the installer test was skipped (it would replace the real install record).'
    }
    else {
        Write-Host 'Testing the installer...'
        $target = Join-Path ([IO.Path]::GetTempPath()) ('mercury-install-test-' + [guid]::NewGuid().ToString('N'))
        Start-Process $setup -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/NOICONS', "/DIR=$target" -Wait
        try {
            $installedExe = Join-Path $target 'Mercury.Desktop.exe'
            if (-not (Test-Path $installedExe)) { throw 'Installer test FAILED: the app was not installed.' }
            if ((Get-FileHash $installedExe).Hash -ne (Get-FileHash "$root\publish\Mercury.Desktop.exe").Hash) {
                throw 'Installer test FAILED: the installed app is not the one that was built.'
            }
            & "$PSScriptRoot\smoke-test.ps1" -Exe $installedExe
        }
        finally {
            $uninstaller = Join-Path $target 'unins000.exe'
            if (Test-Path $uninstaller) { Start-Process $uninstaller -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART' -Wait }
        }
    }
}

# 6. What the website's download page needs for this build.
$file = Get-Item $setup
Write-Host ''
Write-Host "Done: $($file.FullName)" -ForegroundColor Green
Write-Host ("Size:   {0:N0} MB" -f ($file.Length / 1MB))
Write-Host ("SHA-256: {0}" -f (Get-FileHash $setup).Hash.ToLower())
