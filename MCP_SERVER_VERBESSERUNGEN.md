# MCP Server Verbesserungsvorschläge

Basierend auf den Erkenntnissen aus `docs/passing-siteswap-learnings.md`.

---

## Dokumentation verbessern

- [x] **Wildcards in Tool-Beschreibungen dokumentieren** ✅
  - In `GenerateSiteswapsTool.cs` die Description für `pattern` und `flexiblePattern` erweitern
  - Spezialwerte: `-1` (beliebig), `-2` (nur Pässe), `-3` (nur Selfs)
  - Beispiel hinzufügen: `"9,-1,6,-1"` für Patterns mit festen Positionen 0 und 2

- [x] **Passing-Mathematik dokumentieren** ✅
  - Wie berechnet man lokale Siteswaps aus dem globalen?
  - Welche Positionen gehören zu welchem Jongleur?
  - Beispiele mit Ballverteilung zwischen Jongleuren

---

## Neue Tools hinzufügen

- [x] **`GetLocalSiteswapsBatch`** - Batch-Abfrage für lokale Siteswaps ✅
  - Input: Liste von Siteswaps + numberOfJugglers
  - Output: Alle lokalen Siteswaps für alle Jongleure auf einmal
  - Spart viele API-Calls bei der Analyse mehrerer Patterns

---

## Visualisierung verbessern

- [ ] **`GenerateTransitionGraph` erweitern für Passing**
  - Neuer Parameter: `numberOfJugglers`
  - Kantenfarben basierend auf welcher Jongleur sich ändert
  - Legende mit Jongleur-Farben

- [ ] **Neues Tool: `GeneratePassingGraph`**
  - Input: Liste von Siteswaps, numberOfJugglers
  - Output: Graph mit Knoten=Siteswaps, Kanten=mögliche Übergänge
  - Kanten gefärbt nach: Jongleur A ändert sich (rot), B ändert sich (blau), beide (lila)
  - Nützlich für das Finden von Passing-Kreisen

---

## Prioritäten

### Schnelle Wins (niedrige Komplexität, hoher Nutzen) ✅ ERLEDIGT
1. ~~Wildcards dokumentieren in Tool-Beschreibungen~~ ✅
2. ~~`GetLocalSiteswapsBatch` implementieren~~ ✅
3. ~~README erweitern~~ ✅

---

## Implementierungsnotizen

### Wildcards in Beschreibungen (Beispiel)

```csharp
[Description("Pattern to match (comma-separated numbers, e.g., '3,3,1'). " +
             "Special values: -1 (any value), -2 (any pass), -3 (any self). " +
             "Example: '9,-1,6,-1' matches patterns where position 0=9 and position 2=6.")]
string? pattern = null,
```
