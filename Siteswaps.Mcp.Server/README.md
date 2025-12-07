# MCP Siteswap Generator Server

MCP Server zur Generierung von Siteswaps für Juggling-Patterns.

Der Server wird als ASP.NET Core Web-Server gehostet und kann über HTTP/HTTPS erreicht werden.

## Wichtiger Hinweis

**Der Server muss ZUERST manuell gestartet werden**, bevor sich der MCP-Client verbinden kann. Der Server läuft als separater Prozess und der Client verbindet sich über HTTP/HTTPS.

## Konfiguration

### Schritt 1: Server starten

Der Server muss als normale Web-Anwendung gestartet werden:

```bash
dotnet run --project MCP.SiteswapGenerator/MCP.SiteswapGenerator.csproj
```

Standardmäßig läuft der Server auf `http://localhost:5000` oder `https://localhost:5001`.

**Wichtig:** Der Server muss laufen, bevor der MCP-Client sich verbindet. Lassen Sie diesen Prozess im Hintergrund laufen.

### Schritt 2: MCP Client Konfiguration

Für die Verwendung mit einem MCP-Client (z.B. Claude Desktop) muss der Server über HTTP/HTTPS konfiguriert werden. **Verwenden Sie `url` statt `command`/`args`:**

```json
{
  "mcpServers": {
    "siteswap-generator": {
      "url": "http://localhost:5000"
    }
  }
}
```

Oder mit HTTPS:

```json
{
  "mcpServers": {
    "siteswap-generator": {
      "url": "https://localhost:5001"
    }
  }
}
```

**Wichtig:** 
- Verwenden Sie **NICHT** `command` und `args` - der Server wird nicht automatisch vom Client gestartet
- Die URL muss dem Port entsprechen, auf dem der Server läuft
- Der Standard-Port kann in `launchSettings.json` oder über Umgebungsvariablen konfiguriert werden
- Stellen Sie sicher, dass der Server läuft, bevor Sie den Client starten
