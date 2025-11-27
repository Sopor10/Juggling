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
- `minOccurrence` (string, optional): Mindestanzahl. Format: "3:2" (einzeln), "3:2,4:1" (mehrere), "3:1|4:1" (OR), "3,4:2" (mehrere Zahlen)
- `maxOccurrence` (string, optional): Maximalanzahl. Format: "5:1" (einzeln), "5:1,6:2" (mehrere), "3,4:2" (mehrere Zahlen)
- `exactOccurrence` (string, optional): Exakte Anzahl. Format: "5:2" (einzeln), "5:2,6:1" (mehrere), "3,4:2" (mehrere Zahlen)
- `numberOfPasses` (int, optional): Exakte Anzahl Pässe (benötigt numberOfJugglers)
- `numberOfJugglers` (int, optional): Anzahl Jongleure (für numberOfPasses/pattern)
- `pattern` (string, optional): Pattern (komma-separiert, z.B. "3,3,1", benötigt numberOfJugglers)

**Rückgabe:** Liste von Siteswap-Strings (z.B. "531", "441", "a7242")

**Beispiele:**
```
GenerateSiteswaps(period: 5, numberOfObjects: 4, minHeight: 2, maxHeight: 10, minOccurrence: "3:1", maxOccurrence: "5:2")
GenerateSiteswaps(period: 3, numberOfObjects: 3, minHeight: 2, maxHeight: 5, minOccurrence: "3:2,4:1")
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, minOccurrence: "3:1|4:1", numberOfJugglers: 2)
```
