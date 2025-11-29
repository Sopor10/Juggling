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
- [x] `Tools/ValidateSiteswapTool.cs` erstellen
- [x] `[McpServerTool]` Attribute hinzufügen
- [x] Parameter: `siteswap` (string)
- [x] Validierung mit `Siteswap.Details.Siteswap.TryCreate()` (mit Namespace-Alias behoben)
- [x] Ergebnis zurückgeben
- [x] Tool in `Program.cs` registriert
- [x] Tests erstellt und erfolgreich

### 2.6 Tool: AnalyzeSiteswap implementieren (optional)
- [x] `Tools/AnalyzeSiteswapTool.cs` erstellen
- [x] `[McpServerTool]` Attribute hinzufügen
- [x] Parameter: `siteswap` (string)
- [x] Analyse-Funktionen nutzen (Orbits, States, Period, NumberOfObjects, MaxHeight, etc.)
- [x] Strukturierte Analyse-Daten zurückgeben (SiteswapAnalysis mit Orbits, States, etc.)
- [x] Tool in `Program.cs` registriert
- [x] Tests erstellt und erfolgreich

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
- [x] Filter-Parameter zu `GenerateSiteswaps` hinzufügen
- [x] Pattern-Filter (pattern Parameter, benötigt numberOfJugglers)
- [x] Number-Filter (minOccurrence, maxOccurrence, exactOccurrence Parameter)
  - [x] Unterstützung für OR-Logik mit `|` in minOccurrence
  - [x] Unterstützung für mehrere Zahlen mit Komma (z.B. "3,4:2")
- [x] State-Filter (state Parameter)
- [x] NumberOfPasses-Filter (numberOfPasses Parameter, benötigt numberOfJugglers)
- [x] Flexible Pattern-Filter (flexiblePattern Parameter, benötigt numberOfJugglers)
- [x] Rotation-Aware Pattern-Filter (rotationAwarePattern Parameter, benötigt numberOfJugglers und jugglerIndex)
- [x] Personalized Number-Filter (personalizedNumberFilter Parameter, benötigt numberOfJugglers)
- [x] Locally Valid Filter (jugglerIndex Parameter, benötigt numberOfJugglers)
- [x] Default Filter Option (useDefaultFilter Parameter, Standard: true)
- [x] No Filter Option (useNoFilter Parameter, Standard: false)
- [x] Anzahl Jongleure Parameter (numberOfJugglers Parameter, für Multi-Juggler Filter)
- [x] Not-Filter (Negation von Filtern)
- [x] Erweiterte OR-Logik für alle Filter-Typen (nicht nur minOccurrence)
- [ ] Filter-Kombinationen über komplexe AND/OR-Bäume (teilweise durch OR-Logik und Not-Filter abgedeckt)

