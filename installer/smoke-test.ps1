# Checks that Mercury really opens. "The process is still running" proves nothing: when an app crashes
# on start-up, Windows' crash dialog keeps the process alive. So this waits for a real window titled
# "Mercury" and fails with the crash log if it never appears.
#
#   powershell -File installer\smoke-test.ps1 -Exe publish\Mercury.Desktop.exe
param(
    [Parameter(Mandatory)][string]$Exe,
    [int]$TimeoutSeconds = 30
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Exe)) { throw "No such file: $Exe" }

# Run a copy from a fresh folder, the way an installed app would be run, never from the build folder.
$dir = Join-Path ([IO.Path]::GetTempPath()) ('mercury-smoke-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $dir | Out-Null
Copy-Item $Exe $dir
$copy = Join-Path $dir (Split-Path $Exe -Leaf)

$crashLog = Join-Path $env:LOCALAPPDATA 'Mercury\crash.log'
$logBefore = if (Test-Path $crashLog) { (Get-Item $crashLog).LastWriteTimeUtc } else { $null }

# A failure is logged without leaving a dialog open on the build machine.
$env:MERCURY_NO_DIALOGS = '1'
$process = Start-Process $copy -PassThru -WorkingDirectory $dir

$opened = $false
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
try {
    while ((Get-Date) -lt $deadline) {
        $process.Refresh()
        if ($process.HasExited) { break }
        if ($process.MainWindowHandle -ne 0 -and $process.MainWindowTitle -eq 'Mercury') { $opened = $true; break }
        Start-Sleep -Milliseconds 300
    }

    # Opening is not enough: it must also still be there a moment later.
    if ($opened) {
        Start-Sleep -Seconds 3
        $process.Refresh()
        if ($process.HasExited) { $opened = $false }
    }

    if (-not $opened) {
        $reason = if ($process.HasExited) { "it exited with code $($process.ExitCode)" } else { "no window appeared within $TimeoutSeconds seconds" }
        $detail = ''
        if ((Test-Path $crashLog) -and ((Get-Item $crashLog).LastWriteTimeUtc -ne $logBefore)) {
            $detail = "`nCrash log ($crashLog):`n" + ((Get-Content $crashLog -Tail 12) -join "`n")
        }
        throw "Smoke test FAILED: Mercury did not open ($reason).$detail"
    }

    Write-Host "Smoke test passed: Mercury opened a window." -ForegroundColor Green
}
finally {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    Remove-Item Env:\MERCURY_NO_DIALOGS -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 300
    Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue
}
