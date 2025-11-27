# MCP Server für Siteswap-Generierung - Projektplan

## Übersicht

Dieser Plan beschreibt die Implementierung eines MCP (Model Context Protocol) Servers zur Generierung von Siteswaps zusätzlich zur bestehenden WebAssembly-Anwendung.

**Geschätzter Gesamtaufwand:** 3-5 Stunden

**Status:** 🟡 In Planung

---

## Phase 1: Refaktorierung - Generator in Core Library verschieben

### 1.1 Neue Klassenbibliothek erstellen
- [ ] Neues Projekt `Siteswaps.Generator.Core` erstellen
- [ ] Projekt-Typ: `.NET Standard 2.1` oder `.NET 8` Klassenbibliothek
- [ ] Projekt zur Solution hinzufügen
- 💡 **Hinweis:** Kann mit Rider MCP `create_new_file` oder manuell erstellt werden

### 1.2 Abhängigkeiten bereinigen
- [ ] `Radzen` Abhängigkeit entfernen (nur in `AndFilter.cs` für `WhereNotNull()`)
- [ ] Eigene Extension-Methode `WhereNotNull()` erstellen oder durch LINQ ersetzen
- [ ] `morelinq` Package-Referenz hinzufügen (für `ZipLongest` in `EnumerableIntExtension.cs`)
- [ ] `System.Linq.Async` Package-Referenz hinzufügen (falls benötigt)

### 1.3 Projekt-Referenzen anpassen
- [ ] `Siteswaps.Generator` Projekt referenziert jetzt `Siteswaps.Generator.Core`
- [ ] `Siteswap.Details` Projekt referenziert ggf. `Siteswaps.Generator.Core` (falls nötig)
- [ ] Alle Tests aktualisieren und Referenzen anpassen

### 1.4 Generator-Dateien verschieben
- [ ] Kompletten `Generator/` Ordner inkl. aller Unterordner (`Filter/`, etc.) nach `Siteswaps.Generator.Core/Generator/` verschieben
- 💡 **Hinweis:** Dateien können manuell verschoben werden oder mit Rider's "Move File" Refactoring (hält Referenzen aktuell)

### 1.5 Namespaces anpassen
- [ ] Namespace von `Siteswaps.Generator.Generator` zu `Siteswaps.Generator.Core.Generator` ändern
- [ ] Alle using-Statements in abhängigen Projekten aktualisieren
- 💡 **Hinweis:** Kann mit Rider MCP `rename_refactoring` für Namespace-Refactoring verwendet werden, oder manuell mit "Rename" Refactoring in Rider (aktualisiert automatisch alle Referenzen)

### 1.6 Tests durchführen
- [ ] Alle bestehenden Tests ausführen
- [ ] Sicherstellen, dass keine Regressionen eingeführt wurden
- [ ] Build erfolgreich

---

## Phase 2: MCP Server Implementierung

### 2.1 MCP Server Projekt erstellen
- [ ] Neues Console App Projekt `MCP.SiteswapGenerator` erstellen
- [ ] Projekt-Typ: `.NET 8` Console Application
- [ ] Projekt zur Solution hinzufügen

### 2.2 NuGet Packages hinzufügen
- [ ] `ModelContextProtocol` Package hinzufügen (mit `--prerelease` Flag)
- [ ] `Microsoft.Extensions.Hosting` Package hinzufügen (für Hosting)
- [ ] Projekt-Referenz zu `Siteswaps.Generator.Core` hinzufügen
- [ ] Projekt-Referenz zu `Siteswap.Details` hinzufügen (falls benötigt)

### 2.3 Basis MCP Server Setup
- [ ] `Program.cs` mit MCP Server Basis-Setup erstellen
- [ ] Stdio Transport konfigurieren
- [ ] Logging zu stderr konfigurieren
- [ ] Server startet erfolgreich

### 2.4 Tool: GenerateSiteswaps implementieren
- [ ] `Tools/GenerateSiteswapsTool.cs` erstellen
- [ ] `[McpServerTool]` Attribute hinzufügen
- [ ] Parameter definieren:
  - `period` (int)
  - `numberOfObjects` (int)
  - `minHeight` (int)
  - `maxHeight` (int)
  - `maxResults` (int, optional, default: 100)
  - `timeoutSeconds` (int, optional, default: 30)
- [ ] `SiteswapGenerator` Integration implementieren
- [ ] `IAsyncEnumerable<string>` für Streaming-Ergebnisse
- [ ] Beschreibung und Dokumentation hinzufügen

### 2.5 Tool: ValidateSiteswap implementieren (optional)
- [ ] `Tools/ValidateSiteswapTool.cs` erstellen
- [ ] `[McpServerTool]` Attribute hinzufügen
- [ ] Parameter: `siteswap` (string)
- [ ] Validierung mit `Siteswap.Details.Siteswap.TryCreate()`
- [ ] Ergebnis zurückgeben

### 2.6 Tool: AnalyzeSiteswap implementieren (optional)
- [ ] `Tools/AnalyzeSiteswapTool.cs` erstellen
- [ ] `[McpServerTool]` Attribute hinzufügen
- [ ] Parameter: `siteswap` (string)
- [ ] Analyse-Funktionen nutzen (Orbits, States, etc.)
- [ ] Strukturierte Analyse-Daten zurückgeben

### 2.7 Tools registrieren
- [ ] `WithToolsFromAssembly()` in `Program.cs` verwenden
- [ ] Oder manuelle Tool-Registrierung implementieren
- [ ] Tools werden korrekt erkannt

### 2.8 Error Handling
- [ ] Fehlerbehandlung für ungültige Parameter
- [ ] Fehlerbehandlung für Timeout
- [ ] Fehlerbehandlung für Cancellation
- [ ] Sinnvolle Fehlermeldungen zurückgeben

---

## Phase 3: Testing & Dokumentation

### 3.1 Manuelles Testing
- [ ] MCP Server lokal starten
- [ ] Mit MCP Client verbinden (z.B. Claude Desktop)
- [ ] `GenerateSiteswaps` Tool testen
- [ ] Verschiedene Parameter-Kombinationen testen
- [ ] Edge Cases testen (sehr große Period, sehr viele Objekte, etc.)

### 3.2 Performance Testing
- [ ] Performance bei großen Ergebnismengen testen
- [ ] Memory-Verbrauch überwachen
- [ ] Timeout-Verhalten testen

### 3.3 Dokumentation
- [ ] README für MCP Server erstellen
- [ ] Installation-Anleitung dokumentieren
- [ ] Tool-Beschreibungen dokumentieren
- [ ] Beispiel-Konfiguration für Claude Desktop dokumentieren

### 3.4 Deployment-Vorbereitung
- [ ] Build-Konfiguration prüfen
- [ ] Release-Build testen
- [ ] Deployment-Strategie dokumentieren

---

## Phase 4: Erweiterte Features (Optional)

### 4.1 Filter-Parameter
- [ ] Filter-Parameter zu `GenerateSiteswaps` hinzufügen
- [ ] Pattern-Filter unterstützen
- [ ] Number-Filter unterstützen
- [ ] State-Filter unterstützen

### 4.2 Streaming-Optimierung
- [ ] Streaming-Response für große Ergebnisse optimieren
- [ ] Chunking implementieren

### 4.3 Caching (optional)
- [ ] Caching für häufige Anfragen
- [ ] Cache-Invalidierung

---

## Technische Details

### Projektstruktur (nach Refaktorierung)
```
Siteswaps.Generator.Core/          (Neue Klassenbibliothek)
  - Generator/
    - SiteswapGenerator.cs
    - SiteswapGeneratorInput.cs
    - Siteswap.cs
    - PartialSiteswap.cs
    - Filter/
      - ...

Siteswaps.Generator/               (Bestehend - Blazor Components)
  - Components/
  - DependencyInjectionExtensions.cs

MCP.SiteswapGenerator/            (Neues Console App Projekt)
  - Tools/
    - GenerateSiteswapsTool.cs
    - ValidateSiteswapTool.cs (optional)
    - AnalyzeSiteswapTool.cs (optional)
  - Program.cs
```

### Abhängigkeiten

**Siteswaps.Generator.Core:**
- `morelinq` (für `ZipLongest`)
- `System.Linq.Async` (falls benötigt)

**MCP.SiteswapGenerator:**
- `ModelContextProtocol` (--prerelease)
- `Microsoft.Extensions.Hosting`
- Referenz zu `Siteswaps.Generator.Core`
- Referenz zu `Siteswap.Details` (optional)

### MCP Server Konfiguration

**Stdio Transport:**
- Kommunikation über stdin/stdout
- Logging über stderr

**Tool-Definition Beispiel:**
```csharp
[McpServerTool, Description("Generates siteswaps based on parameters")]
public static async IAsyncEnumerable<string> GenerateSiteswaps(
    [Description("Period of the siteswap")] int period,
    [Description("Number of objects (balls)")] int numberOfObjects,
    [Description("Minimum throw height")] int minHeight,
    [Description("Maximum throw height")] int maxHeight,
    [Description("Maximum number of results")] int maxResults = 100,
    [Description("Timeout in seconds")] int timeoutSeconds = 30,
    CancellationToken cancellationToken = default)
```

---

## Notizen

- Das offizielle C# SDK für MCP ist verfügbar: https://github.com/modelcontextprotocol/csharp-sdk
- NuGet Package: `ModelContextProtocol` (mit --prerelease Flag)
- Die Generator-Logik ist bereits gut getrennt, nur kleine Refaktorierung nötig
- Radzen-Abhängigkeit muss entfernt werden (nur `WhereNotNull()`)
- **Rider MCP Tools:** Für Refactorings können JetBrains Rider MCP Tools verwendet werden:
  - `rename_refactoring` für Namespace-Änderungen (aktualisiert automatisch alle Referenzen)
  - Rider's "Move File" Refactoring für Dateiverschiebungen (hält Referenzen aktuell)
  - `replace_text_in_file` für gezielte Text-Ersetzungen

---

## Status-Tracking

**Letzte Aktualisierung:** [Datum hier eintragen]

**Aktueller Status:** 🟡 In Planung

**Fortschritt:** 0% (0/X Aufgaben abgeschlossen)

