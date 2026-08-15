# On agent turn end: if any file was edited this turn,
# 1) format the repo with CSharpier (autofix)
# 2) start Aspire in the background (aspire start handles an already-running AppHost)

$ErrorActionPreference = 'Continue'

$stateDir = Join-Path $PSScriptRoot 'state'
$markerPath = Join-Path $stateDir 'files-edited.flag'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$appHost = Join-Path $repoRoot 'Juggling.AppHost\Juggling.AppHost.csproj'

function Write-HookJson([hashtable]$obj) {
    $json = ($obj | ConvertTo-Json -Compress)
    [Console]::Out.Write($json)
    [Console]::Out.Flush()
    # Mitigate Windows race where Cursor misses fast hook stdout
    Start-Sleep -Milliseconds 50
}

try {
    $null = [Console]::In.ReadToEnd()
}
catch {
    # stdin optional for this side-effect hook
}

if (-not (Test-Path -LiteralPath $markerPath)) {
    Write-HookJson @{}
    exit 0
}

Remove-Item -LiteralPath $markerPath -Force -ErrorAction SilentlyContinue

Push-Location $repoRoot
try {
    [Console]::Error.WriteLine('[aspire-start-on-stop] csharpier format (files edited this turn)')
    dotnet csharpier format . 2>&1 | ForEach-Object {
        [Console]::Error.WriteLine($_)
    }
    if ($LASTEXITCODE -ne 0) {
        [Console]::Error.WriteLine("[aspire-start-on-stop] csharpier format exited with code $LASTEXITCODE")
    }

    if (-not (Get-Command aspire -ErrorAction SilentlyContinue)) {
        [Console]::Error.WriteLine('[aspire-start-on-stop] aspire CLI not found on PATH; skipping start')
        Write-HookJson @{}
        exit 0
    }

    if (-not (Test-Path -LiteralPath $appHost)) {
        [Console]::Error.WriteLine("[aspire-start-on-stop] AppHost not found: $appHost")
        Write-HookJson @{}
        exit 0
    }

    [Console]::Error.WriteLine('[aspire-start-on-stop] aspire start (files edited this turn)')
    aspire start --apphost $appHost --nologo --non-interactive 2>&1 | ForEach-Object {
        [Console]::Error.WriteLine($_)
    }
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        [Console]::Error.WriteLine("[aspire-start-on-stop] aspire start exited with code $exitCode")
    }
}
finally {
    Pop-Location
}

Write-HookJson @{}
exit 0
