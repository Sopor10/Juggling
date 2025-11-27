# MCP Server für Siteswap-Generierung - Projektplan

## Übersicht

Dieser Plan beschreibt die Implementierung eines MCP (Model Context Protocol) Servers zur Generierung von Siteswaps zusätzlich zur bestehenden WebAssembly-Anwendung.

**Geschätzter Gesamtaufwand:** 3-5 Stunden

**Status:** 🟡 In Planung

---

## Phase 1: Refaktorierung - Generator in Core Library verschieben

### 1.1 Neue Klassenbibliothek erstellen
- [x] Neues Projekt `Siteswaps.Generator.Core` erstellen
- [x] Projekt-Typ: `.NET Standard 2.1` oder `.NET 8` Klassenbibliothek
- [x] Projekt zur Solution hinzufügen
- 💡 **Hinweis:** Kann mit Rider MCP `create_new_file` oder manuell erstellt werden

### 1.2 Abhängigkeiten bereinigen
- [x] `Radzen` Abhängigkeit entfernen (nur in `AndFilter.cs` für `WhereNotNull()`)
- [x] Eigene Extension-Methode `WhereNotNull()` erstellen oder durch LINQ ersetzen (bereits vorhanden in `EnumerableExtension.cs`)
- [x] `morelinq` Package-Referenz hinzufügen (für `ZipLongest` in `EnumerableIntExtension.cs`)
- [x] `System.Linq.Async` Package-Referenz hinzufügen (falls benötigt)

### 1.3 Projekt-Referenzen anpassen
- [x] `Siteswaps.Generator` Projekt referenziert jetzt `Siteswaps.Generator.Core`
- [x] `Siteswap.Details` Projekt referenziert ggf. `Siteswaps.Generator.Core` (falls nötig) - nicht nötig
- [x] Alle Tests aktualisieren und Referenzen anpassen

### 1.4 Generator-Dateien verschieben
- [x] Kompletten `Generator/` Ordner inkl. aller Unterordner (`Filter/`, etc.) nach `Siteswaps.Generator.Core/Generator/` verschieben
- 💡 **Hinweis:** Dateien können manuell verschoben werden oder mit Rider's "Move File" Refactoring (hält Referenzen aktuell)

### 1.5 Namespaces anpassen
- [x] Namespace von `Siteswaps.Generator.Generator` zu `Siteswaps.Generator.Core.Generator` ändern
- [x] Alle using-Statements in abhängigen Projekten aktualisieren
- [x] Sichtbarkeit von Klassen anpassen (`internal` → `public` für benötigte Klassen)
- [x] Architektur-Test angepasst
- 💡 **Hinweis:** Kann mit Rider MCP `rename_refactoring` für Namespace-Refactoring verwendet werden, oder manuell mit "Rename" Refactoring in Rider (aktualisiert automatisch alle Referenzen)

### 1.6 Tests durchführen
- [x] Alle bestehenden Tests ausführen
- [x] Sicherstellen, dass keine Regressionen eingeführt wurden
- [x] Build erfolgreich

---

## Phase 2: MCP Server Implementierung

### 2.1 MCP Server Projekt erstellen
- [x] Neues Console App Projekt `MCP.SiteswapGenerator` erstellen
- [x] Projekt-Typ: `.NET 9` Console Application (zentral in Directory.Build.props)
- [x] Projekt zur Solution hinzufügen

### 2.2 NuGet Packages hinzufügen
- [x] `ModelContextProtocol` Package hinzufügen (mit `--prerelease` Flag)
- [x] `Microsoft.Extensions.Hosting` Package hinzufügen (für Hosting)
- [x] Projekt-Referenz zu `Siteswaps.Generator.Core` hinzufügen
- [x] Projekt-Referenz zu `Siteswap.Details` hinzufügen

### 2.3 Basis MCP Server Setup
- [x] `Program.cs` mit MCP Server Basis-Setup erstellen
- [x] Stdio Transport konfigurieren (`StdioServerTransport`)
- [x] Logging zu stderr konfigurieren (Console Logger)
- [x] Server startet erfolgreich

### 2.4 Tool: GenerateSiteswaps implementieren
- [x] `Tools/GenerateSiteswapsTool.cs` erstellen
- [x] `[McpServerTool]` Attribute hinzufügen
- [x] Parameter definieren:
  - `period` (int)
  - `numberOfObjects` (int)
  - `minHeight` (int)
  - `maxHeight` (int)
  - `maxResults` (int, optional, default: 100)
  - `timeoutSeconds` (int, optional, default: 30)
- [x] `SiteswapGenerator` Integration implementieren
- [x] `IAsyncEnumerable<string>` für Streaming-Ergebnisse
- [x] Beschreibung und Dokumentation hinzufügen

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
- [x] Tools werden automatisch durch `[McpServerTool]` Attribute erkannt
- [x] Keine manuelle Registrierung erforderlich
- [x] Tools werden korrekt erkannt

### 2.8 Error Handling
- [x] Fehlerbehandlung für ungültige Parameter (ArgumentException mit sinnvollen Meldungen)
- [x] Fehlerbehandlung für Timeout (durch SiteswapGenerator.StopCriteria)
- [x] Fehlerbehandlung für Cancellation (CancellationToken wird durchgereicht)
- [x] Sinnvolle Fehlermeldungen zurückgeben

---

## Phase 3: Testing & Dokumentation

### 3.1 Manuelles Testing
- [ ] MCP Server lokal starten
- [ ] Mit MCP Client verbinden (z.B. Claude Desktop)
- [ ] `GenerateSiteswaps` Tool testen
- [ ] Verschiedene Parameter-Kombinationen testen

### 3.2 Performance Testing
- [x] Unit-Tests für GenerateSiteswaps Tool erstellt
- [x] 11 Tests implementiert und alle bestanden
- [x] Validierungstests für alle Parameter
- [x] Funktionalitätstests (Generierung, Limits, Cancellation)
- [x] Timeout-Verhalten getestet

### 3.3 Dokumentation
- [x] README für MCP Server erstellen
- [x] Installation-Anleitung dokumentieren
- [x] Tool-Beschreibungen dokumentieren
- [x] Beispiel-Konfiguration für Claude Desktop dokumentieren

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

**Letzte Aktualisierung:** 2025-01-27

**Aktueller Status:** 🟢 In Bearbeitung

**Fortschritt:** 
- Phase 1 komplett ✅ abgeschlossen (1.1-1.6)
- Phase 2.1-2.4, 2.7-2.8 ✅ abgeschlossen
- Phase 2.5-2.6 (optionale Tools) noch offen
- Phase 3.2 ✅ abgeschlossen (Unit-Tests implementiert und ausgeführt)
- Phase 3.3 ✅ abgeschlossen (README erstellt mit Installation, Tool-Beschreibungen und Claude Desktop Konfiguration)
- Phase 3.4 ✅ abgeschlossen (Release-Build getestet, Deployment-Strategie dokumentiert)
- Phase 3.1 (Manuelles Testing) - Server startet erfolgreich, benötigt MCP Client für vollständiges Testing

