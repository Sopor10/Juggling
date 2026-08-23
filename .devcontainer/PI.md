# Pi im Juggling-Devcontainer

Der Devcontainer installiert [Pi](https://pi.dev) mit dem OpenAI-Codex-Provider. Als Standard sind `PI_PROVIDER=openai-codex` und `PI_MODEL=gpt-5.6-luna` gesetzt; Pi verwendet damit die ChatGPT-Plus/Pro-Codex-Subscription und das Luna-Modell, nicht einen OpenAI-API-Key.

## Anmeldung ohne interaktiven Login im Container

Voraussetzung ist eine bereits bestehende Hermes-Anmeldung auf dem Host:

```text
$HOME/.hermes/auth.json
```

Die Devcontainer-Konfiguration bindet ausschließlich diese Hermes-Auth-Datei read-only nach `/host-hermes-auth.json` ein. Beim Erstellen des Containers konvertiert `.devcontainer/pi-codex-auth.mjs` den Hermes-`openai-codex`-OAuth-Eintrag in Pi's Format und legt ihn in einem separaten Docker-Volume (`juggling-pi-agent`) unter `/home/vscode/.pi/agent/auth.json` ab. Pi kann diesen lokalen Token bei Bedarf selbst refreshen; die Host-Datei wird nicht beschrieben.

Damit ist kein `/login` innerhalb des Containers nötig. Nach einer erneuten Anmeldung auf dem Host kann die lokale Kopie gezielt aktualisiert werden:

```bash
pi-codex-sync
```

Der Befehl überschreibt nur den `openai-codex`-Eintrag und erhält andere Pi-Provider-Einstellungen. `pi` synchronisiert nicht bei jedem Aufruf, damit ein von Pi erneuerter Token nicht durch den read-only Host-Snapshot ersetzt wird.

## Nutzung

Im Repository-Devcontainer:

```bash
pi
pi --version
pi-agent --version
```

Der Standard ist `openai-codex/gpt-5.6-luna`; er kann innerhalb von Pi weiterhin mit `/model` oder über die Pi-Optionen geändert werden.

`pi-agent` ist ein Alias auf die unverpackte Pi-Binary; `pi` ist der Auth-Wrapper. Die Pi-Installation ist auf eine konkrete npm-Version gepinnt und wird ausschließlich im Container installiert.

## Sicherheit und Isolation

- Die Hermes-Auth-Datei ist im Container nur read-only sichtbar.
- Die Pi-Credentials liegen im separaten Docker-Volume und nicht im Git-Workspace.
- Der neue Worktree und der Volume-Name sind vom bestehenden Devcontainer unabhängig; bestehende Container werden nicht ersetzt oder neu gestartet.
- Die Hermes-OpenAI-Codex-Anmeldung muss vor `devcontainer up` vorhanden sein. Fehlt sie, bricht die Container-Erstellung mit einer erklärenden Fehlermeldung statt mit einem interaktiven Login ab.
