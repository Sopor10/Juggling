# MCP Siteswap Generator Server

MCP Server zur Generierung von Siteswaps für Juggling-Patterns.

## Konfiguration

```json
{
  "mcpServers": {
    "siteswap-generator": {
      "command": "dotnet",
      "args": ["run", "--project", "MCP.SiteswapGenerator/MCP.SiteswapGenerator.csproj"]
    }
  }
}
```

## Tools

### GenerateSiteswaps

Generiert Siteswaps basierend auf Parametern.

**Parameter:**
- `period` (int): Period des Siteswaps
- `numberOfObjects` (int): Anzahl der Objekte (Bälle)
- `minHeight` (int): Minimale Wurfhöhe
- `maxHeight` (int): Maximale Wurfhöhe
- `maxResults` (int, optional, Standard: 100): Maximale Anzahl Ergebnisse
- `timeoutSeconds` (int, optional, Standard: 30): Timeout in Sekunden

**Rückgabe:** Liste von Siteswap-Strings (z.B. "531", "441", "a7242")

**Beispiel:**
```
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, maxResults: 20)
```
