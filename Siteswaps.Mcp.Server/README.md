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
- `numberOfJugglers` (int, optional): Anzahl Jongleure (für numberOfPasses/pattern/flexiblePattern)
- `pattern` (string, optional): Pattern (komma-separiert, z.B. "3,3,1", benötigt numberOfJugglers)
- `state` (string, optional): State-Filter (komma-separiert 0/1, z.B. "1,1,0,0" für State mit ersten beiden Slots belegt)
- `flexiblePattern` (string, optional): Flexibles Pattern (Semikolon-getrennte Gruppen, z.B. "3,4;5,6" für zwei Gruppen, benötigt numberOfJugglers)
- `useDefaultFilter` (bool, optional, Standard: true): Standard-Filter verwenden (RightAmountOfBallsFilter)
- `useNoFilter` (bool, optional, Standard: false): Kein Filter verwenden (akzeptiert alle Siteswaps)
- `jugglerIndex` (int, optional): Index des Jongleurs für lokale Filter (0-basiert, benötigt numberOfJugglers)
- `rotationAwarePattern` (string, optional): Rotation-aware flexibles Pattern für spezifischen Jongleur (Semikolon-getrennte Gruppen, benötigt numberOfJugglers und jugglerIndex)
- `personalizedNumberFilter` (string, optional): Personalisierter Nummer-Filter für spezifischen Jongleur. Format: "zahl:anzahl:typ:von" wobei typ "exact", "atleast" oder "atmost" ist (benötigt numberOfJugglers)
- `useNoFilter` (bool, optional, Standard: false): Kein Filter verwenden (akzeptiert alle Siteswaps)
- `jugglerIndex` (int, optional): Index des Jongleurs für lokale Filter (0-basiert, benötigt numberOfJugglers)
- `rotationAwarePattern` (string, optional): Rotation-aware flexibles Pattern für spezifischen Jongleur (Semikolon-getrennte Gruppen, benötigt numberOfJugglers und jugglerIndex)
- `personalizedNumberFilter` (string, optional): Personalisierter Nummer-Filter für spezifischen Jongleur. Format: "zahl:anzahl:typ:von" wobei typ "exact", "atleast" oder "atmost" ist (benötigt numberOfJugglers)

**Rückgabe:** Liste von Siteswap-Strings (z.B. "531", "441", "a7242")

**Beispiele:**
```
GenerateSiteswaps(period: 5, numberOfObjects: 4, minHeight: 2, maxHeight: 10, minOccurrence: "3:1", maxOccurrence: "5:2")
GenerateSiteswaps(period: 3, numberOfObjects: 3, minHeight: 2, maxHeight: 5, minOccurrence: "3:2,4:1")
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, minOccurrence: "3:1|4:1", numberOfJugglers: 2)
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, state: "1,1,0,0,0")
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, flexiblePattern: "3,4;5,6", numberOfJugglers: 2)
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, jugglerIndex: 0, numberOfJugglers: 2)
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, rotationAwarePattern: "3,4;5,6", numberOfJugglers: 2, jugglerIndex: 0)
GenerateSiteswaps(period: 5, numberOfObjects: 5, minHeight: 2, maxHeight: 10, personalizedNumberFilter: "3:2:atleast:0", numberOfJugglers: 2)
```
