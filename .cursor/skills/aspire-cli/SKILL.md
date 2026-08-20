---
name: aspire-cli
description: >-
  Start, stop, and inspect this repo's local dev services exclusively through
  the .NET Aspire AppHost via the `aspire` CLI, instead of running
  `dotnet run` directly in individual projects (Webassembly,
  Siteswaps.Mcp.Server, etc.). Use whenever you need to run/verify the app
  locally, check if a dev server is already running, find service URLs, or
  are about to run `dotnet run` in Webassembly or another orchestrated
  project.
disable-model-invocation: false
---

# Aspire CLI for this repo

## Golden rule

Never start `Webassembly`, `Siteswaps.Mcp.Server`, or any other orchestrated
project with a raw `dotnet run` / `dotnet watch`. Always go through the
Aspire AppHost (`Juggling.AppHost`, referenced in `Juggling.slnx`) via the
`aspire` CLI. Running projects directly bypasses the AppHost, causes port
collisions, and leaves orphaned `dotnet` processes that corrupt subsequent
builds.

The AppHost (`Juggling.AppHost/AppHost.cs`) orchestrates exactly two
resources: **`Webassembly`** (the Blazor WASM client, routes include
`/cardstack` and `/wizard`) and **`McpServer`** (`Siteswaps.Mcp.Server`).
Verified working: `aspire run` starts both as `Healthy` and serves
`Webassembly` on `http://localhost:7021` (routes `/cardstack`, `/wizard`
both return 200) and `McpServer` on `http://localhost:5000` /
`https://localhost:5001`. Exact ports are dynamically assigned per run —
always read them from the dashboard or `aspire describe`, don't hardcode.

## Before starting: check for an already-running instance

```powershell
aspire ps
```

Lists every running AppHost with its PID and dashboard URL. If one is
already running for this repo, reuse it (open the dashboard URL) instead of
starting a second instance.

If `aspire ps` shows nothing but you suspect stray processes from the old
`dotnet run` workflow, check and clean those up instead of trying to share
a port with them:

```powershell
Get-Process dotnet -ErrorAction SilentlyContinue | Select-Object Id, StartTime, Path
Stop-Process -Id <id> -Force   # only for confirmed stray/old processes
```

## Starting the app

Run from the repo root (Aspire searches the current directory and
subdirectories for an AppHost project):

```powershell
aspire run
```

Useful variants:

```powershell
# Target the AppHost explicitly (unambiguous, works from any cwd)
aspire run --apphost './Juggling.AppHost/Juggling.AppHost.csproj'

# Run in the background and get the terminal back (list/stop via aspire ps/aspire stop)
aspire run --detach

# Randomized ports + isolated user secrets, so it can run alongside another instance
aspire run --isolated

# Skip the build step when nothing changed
aspire run --no-build
```

`aspire run` does **not** watch files by default — after editing AppHost
code, `Ctrl+C` and re-run. Individual resources (e.g. the Webassembly
project) still hot-reload/rebuild through the AppHost/dashboard.

## Finding service URLs

`aspire run` prints the **Dashboard** URL directly in the console output,
e.g.:

```
Dashboard:  https://localhost:17244/login?t=...
```

Open the dashboard to see every orchestrated resource — listed there as
`Webassembly` and `McpServer` (dashboard names get a random suffix, e.g.
`Webassembly-gpacbwnm`) — with its live endpoint URLs, logs, traces, and
state. Individual resource endpoints are also listed in the console right
after the dashboard link. For the Webassembly client, append the route
under test to its printed endpoint, e.g. `/cardstack` or `/wizard`.

To get endpoint/dashboard info without watching the console:

```powershell
aspire ps --format Json
```

For per-resource URLs, health, and PIDs (e.g. to script "is Webassembly
up yet"):

```powershell
aspire describe --apphost './Juggling.AppHost/Juggling.AppHost.csproj' --format Json
```

## Stopping the app

```powershell
aspire stop            # stops the in-scope AppHost (prompts if several match)
aspire stop --all      # stops every running AppHost
aspire stop --apphost './Juggling.AppHost/Juggling.AppHost.csproj'
```

Always stop cleanly with `aspire stop` (or `Ctrl+C` in the foreground
terminal running `aspire run`) before starting a new instance. This lets
the AppHost gracefully shut down all child resources, containers, and the
dashboard — avoiding the orphaned-process problem this skill exists to
prevent.

## Installing / updating the CLI

```powershell
# Install (global .NET tool)
dotnet tool install -g Aspire.Cli

# Update to latest
dotnet tool update -g Aspire.Cli

# Verify
aspire --version
```

Requires .NET SDK 10.0.100+. Match the CLI's major version to the
`Aspire.AppHost.Sdk` / `Aspire.Hosting*` package versions referenced by
`Juggling.AppHost.csproj` (check `Directory.Packages.props` for the pinned
`Aspire.*` versions) — a large version mismatch between CLI and packages
can cause `aspire run` to fail to build or start the AppHost.

## Troubleshooting: "multiple parallel dotnet processes corrupt the build"

Root cause: a dev server was started with `dotnet run`/`dotnet watch`
directly inside `Webassembly` (or another project), instead of via the
AppHost. That process keeps holding a port/lock, and a later `aspire run`
(or another direct `dotnet run`) then fails to bind the port or produces
inconsistent build output.

Fix / prevention:

1. Before starting anything, run `aspire ps` to check for an existing
   AppHost-managed run.
2. If none is listed but a stale process might exist from the old
   workflow, run `Get-Process dotnet` and stop any process you don't
   recognize as belonging to a current `aspire run` session
   (`Stop-Process -Id <id> -Force`).
3. Always start via `aspire run` (optionally `--isolated` if you
   deliberately need a second parallel instance) — never via `dotnet run`
   in `Webassembly` or `Siteswaps.Mcp.Server` directly.
4. Always stop via `aspire stop` / `Ctrl+C` on the `aspire run` terminal,
   not by killing the process ad hoc, so child resources shut down
   gracefully.

## No AppHost found?

If `aspire run` ever reports "no AppHost project found" (e.g. after
checking out an older commit/branch without `Juggling.AppHost`), do not
fall back to `dotnet run`. Flag it to the user instead — re-adding or
rewiring the AppHost is a deliberate decision, not something to improvise.
