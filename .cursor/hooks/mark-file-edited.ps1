# Marks that the agent edited a file during the current turn.
# Consumed by aspire-start-on-stop.ps1 on the stop event.

$ErrorActionPreference = 'Stop'

$stateDir = Join-Path $PSScriptRoot 'state'
$markerPath = Join-Path $stateDir 'files-edited.flag'

try {
    $inputJson = [Console]::In.ReadToEnd()
    if (-not [string]::IsNullOrWhiteSpace($inputJson)) {
        $data = $inputJson | ConvertFrom-Json
        $file = [string]$data.file_path
        # Ignore our own state files
        if ($file -and ($file -replace '\\', '/') -match '/\.cursor/hooks/state/') {
            exit 0
        }
    }
}
catch {
    # Still mark on parse failures — any agent edit should count
}

New-Item -ItemType Directory -Force -Path $stateDir | Out-Null
Set-Content -Path $markerPath -Value (Get-Date -Format 'o') -NoNewline
exit 0
