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
- `minOccurrence` (string, optional): Mindestanzahl einer Zahl (Format: "zahl:anzahl", z.B. "3:2")
- `maxOccurrence` (string, optional): Maximalanzahl einer Zahl (Format: "zahl:anzahl", z.B. "5:1")
- `numberOfPasses` (int, optional): Exakte Anzahl Pässe (benötigt numberOfJugglers)
- `numberOfJugglers` (int, optional): Anzahl Jongleure (für numberOfPasses/pattern)
- `pattern` (string, optional): Pattern (komma-separiert, z.B. "3,3,1", benötigt numberOfJugglers)

**Rückgabe:** Liste von Siteswap-Strings (z.B. "531", "441", "a7242")

**Beispiele:**
```
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, maxResults: 20)
GenerateSiteswaps(period: 3, numberOfObjects: 3, minHeight: 2, maxHeight: 5, minOccurrence: "3:2")
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, pattern: "3,3,1", numberOfJugglers: 2)
```
