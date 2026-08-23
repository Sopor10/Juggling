# Pi in the Juggling Dev Container

The Dev Container installs [Pi](https://pi.dev) with the OpenAI Codex provider. The defaults are `PI_PROVIDER=openai-codex` and `PI_MODEL=gpt-5.6-luna`. Pi therefore uses the ChatGPT Plus/Pro Codex subscription and the Luna model, not an OpenAI API key.

## Authentication without an interactive container login

A Hermes login must already exist on the host when Pi should use the subscription directly:

```text
$HOME/.hermes/auth.json
```

`initializeCommand` automatically creates an empty placeholder file with mode `0600` for CI and new hosts. An existing real authentication file is never overwritten. Without a real login, Pi is still installed, but a Pi invocation does not prompt for an interactive login and instead fails with an authentication error.

The Dev Container configuration mounts only this Hermes authentication file read-only at `/host-hermes-auth.json`. During container creation, `.devcontainer/pi-codex-auth.mjs` converts the Hermes `openai-codex` OAuth entry into Pi's format and stores it in a separate Docker volume (`juggling-pi-agent`) at `/home/vscode/.pi/agent/auth.json`. Pi can refresh this local token when necessary; the host file is never modified.

This means that `/login` is not required inside the container. After logging in again on the host, update the local copy explicitly:

```bash
pi-codex-sync
```

The command replaces only the `openai-codex` entry and preserves other Pi provider settings. `pi` does not synchronize on every invocation, so a token refreshed by Pi is not replaced by the read-only host snapshot.

## Usage

Inside the repository's Dev Container:

```bash
pi
pi --version
pi-agent --version
```

The default is `openai-codex/gpt-5.6-luna`. It can still be changed inside Pi with `/model` or through Pi's options.

`pi-agent` is an alias for the unwrapped Pi binary; `pi` is the authentication wrapper. Pi is pinned to a specific npm version and is installed only inside the container.

## Security and isolation

- The Hermes authentication file is visible inside the container only as a read-only mount.
- Pi credentials are stored in a separate Docker volume, not in the Git workspace.
- The new worktree and volume name are independent of the existing Dev Container; existing containers are not replaced or restarted.
- The Hermes OpenAI Codex login must exist before `devcontainer up` when Pi should use the subscription directly. If no local login exists, authentication synchronization is skipped with an explanatory message rather than starting an interactive login. CI can install Pi without authentication; the authentication synchronization is skipped there.
