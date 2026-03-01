# Learnings: Passing-Siteswap-Kreise generieren

## Was ich während des Prozesses gelernt habe

### 1. Globale vs. Lokale Siteswaps

Bei 2-Personen-Passing mit abwechselnden Würfen gilt:
- **Person A** wirft an den **geraden** Positionen (0, 2, 4, ...)
- **Person B** wirft an den **ungeraden** Positionen (1, 3, 5, ...)

Beispiel für `9663`:
- Global: 9, 6, 6, 3
- Person A (Pos 0, 2): 9, 6 → lokal "4.5p 3" (geteilt durch 2)
- Person B (Pos 1, 3): 6, 3 → lokal "3 1.5p"

### 2. Ballverteilung zwischen Jongleuren

Die Ballverteilung muss **nicht** gleichmäßig sein! Bei Passing-Patterns ist es völlig normal, dass ein Jongleur durchschnittlich mehr Bälle hat als der andere.

**Beispiel 9663:**
- A: durchschnittlich 3.75 Bälle
- B: durchschnittlich 2.25 Bälle

Das funktioniert, weil die Bälle ständig zwischen den Jongleuren wechseln. Mal hat A mehr, mal B - im Durchschnitt über den Zyklus behält jeder seine Bälle.

### 3. Übergänge mit nur einer lokalen Änderung

Um Übergänge zu finden, bei denen sich nur der Siteswap **einer** Person ändert:

- **Nur Person A ändert sich:** Die Siteswaps müssen sich nur an **geraden** Positionen unterscheiden
- **Nur Person B ändert sich:** Die Siteswaps müssen sich nur an **ungeraden** Positionen unterscheiden

**Gefundene Paare (nur B ändert sich, gleiche A-Position):**
| A lokal | Siteswap 1 | B lokal 1 | Siteswap 2 | B lokal 2 |
|---------|------------|-----------|------------|-----------|
| 96      | 9663       | 63        | 9564       | 54        |
| 87      | 8673       | 63        | 8574       | 54        |
| 95      | 9753       | 73        | 9555       | 55        |

**Gefundene Paare (nur A ändert sich, gleiche B-Position):**
| B lokal | Siteswap 1 | A lokal 1 | Siteswap 2 | A lokal 2 |
|---------|------------|-----------|------------|-----------|
| 63      | 9663       | 96        | 8673       | 87        |
| 54      | 9564       | 96        | 8574       | 87        |
| 73      | 9753       | 95        | 7773       | 77        |

### 4. Schwierigkeit bei 5er-Kreisen

Ein 5er-Kreis mit der Bedingung "nur eine lokale Änderung pro Transition" ist mathematisch schwierig, weil:
- Die Anzahl der Siteswaps mit gemeinsamen lokalen Teilen begrenzt ist
- Man einen geschlossenen Graphen mit 5 Knoten finden muss
- Der 4er-Kreis (9663 → 9564 → 8574 → 8673 → 9663) ist der natürliche minimale Kreis

---

## Was mir geholfen hätte

### 1. Funktion zum Finden von "ähnlichen" Siteswaps

```
find_similar_siteswaps(
    baseSiteswap: "9663",
    changePositions: "odd"   // Nur ungerade Positionen ändern
    // oder: "even"          // Nur gerade Positionen ändern
)
```

**Warum:** Um Übergänge zu finden wo sich nur ein lokaler Siteswap ändert, musste ich manuell alle Kombinationen durchprobieren.

### 2. Direkte "Passing-Kreis" Generierung

```
generate_passing_cycle(
    numberOfJugglers: 2,
    numberOfObjects: 6,
    cycleLength: 5,
    constraint: "single_local_change"  // Pro Übergang ändert sich nur ein lokaler Siteswap
)
```

**Warum:** Das war mein eigentliches Ziel, und ich musste es manuell aus Einzelteilen zusammenbauen.

### 3. Batch-Abfrage für lokale Siteswaps

```
get_local_siteswaps_batch(
    siteswaps: ["9663", "9564", "8574", ...],
    numberOfJugglers: 2
)
```

**Warum:** Ich musste jeden Siteswap einzeln für beide Jongleure abfragen (2 Calls pro Siteswap).

### 4. Dokumentation über Passing-Mathematik

Hilfreiche Dokumentation wäre:
- Wie berechnet man die lokalen Siteswaps aus dem globalen Siteswap?
- Wie findet man Siteswaps die sich nur in bestimmten Positionen unterscheiden?

### 5. Visualisierung des Übergangsgraphen

Eine Graph-Visualisierung mit:
- Knoten = Siteswaps
- Kanten = mögliche Übergänge
- Kantenfarbe = welcher Jongleur sich ändert (A, B, oder beide)

---

## Fehlerhafte Annahme während des Prozesses

Ich habe fälschlicherweise angenommen, dass die Ballverteilung zwischen den Jongleuren gleichmäßig sein muss. Das war **nicht** Teil der Anforderung und ist auch **nicht notwendig** für gültige Passing-Patterns. Diese Annahme hat den Suchprozess unnötig verkompliziert.

---

## Nachträgliche Erkenntnis: Wildcards im Pattern-Filter existieren bereits!

Bei der Code-Analyse habe ich entdeckt, dass der `flexiblePattern`-Filter bereits **Wildcards unterstützt**, die ich hätte nutzen können:

### Versteckte Wildcard-Werte im FlexiblePatternFilter

Im Code (`FlexiblePatternFilter.cs`) sind spezielle Werte definiert:

| Wert | Konstante | Bedeutung |
|------|-----------|-----------|
| `-1` | `DontCare` | Wildcard - jeder Wert ist erlaubt |
| `-2` | `Pass` | Nur Pässe (ungerade Werte bei 2 Jongleuren) |
| `-3` | `Self` | Nur Selfs (gerade Werte bei 2 Jongleuren) |

### Wie ich es hätte nutzen können

Um alle Siteswaps zu finden, bei denen **Person A** den lokalen Siteswap `96` hat (Positionen 0 und 2 fix, Rest beliebig):

```
generate_siteswaps(
    period: 4,
    numberOfObjects: 6,
    minHeight: 3,
    maxHeight: 9,
    pattern: "9,-1,6,-1",    // Pos 0=9, Pos 2=6, Pos 1 und 3 beliebig
    numberOfJugglers: 2
)
```

Oder mit flexiblePattern für mehr Optionen pro Position:

```
generate_siteswaps(
    period: 4,
    numberOfObjects: 6,
    minHeight: 3,
    maxHeight: 9,
    flexiblePattern: "9;-1;6;-1",    // Semikolon-getrennt
    numberOfJugglers: 2
)
```

### Das Problem: Fehlende Dokumentation

Diese Wildcards werden **nirgends dokumentiert**:

1. **Tool-Beschreibung** (GenerateSiteswapsTool.cs):
   - `pattern`: "Pattern to match (comma-separated numbers, e.g., '3,3,1')"
   - `flexiblePattern`: "Flexible pattern (semicolon-separated groups, e.g., '3,4;5,6' for two groups)"
   
2. **README.md**: Keine Erwähnung von `-1`, `-2`, `-3`

### Empfehlung für die Dokumentation

Die Tool-Beschreibungen sollten erweitert werden:

```csharp
[Description("Pattern to match (comma-separated numbers, e.g., '3,3,1'). " +
             "Use -1 for wildcard (any value), -2 for any pass, -3 for any self. " +
             "Example: '9,-1,6,-1' matches patterns where position 0=9 and position 2=6.")]
string? pattern = null,

[Description("Flexible pattern (semicolon-separated groups, e.g., '3,4;5,6'). " +
             "Each group can contain multiple allowed values. " +
             "Special values: -1 (any), -2 (any pass), -3 (any self). " +
             "Example: '9;3,4,5;6;-1' allows 3,4 or 5 at position 1.")]
string? flexiblePattern = null,
```

### Fazit

Hätte ich von den Wildcards gewusst, hätte ich:
1. Direkt nach Siteswaps mit bestimmten lokalen Patterns filtern können
2. Viel weniger API-Calls gebraucht
3. Den Kreis systematischer aufbauen können

---

## Zusammenfassung

Der Prozess war lehrreich, aber zeitaufwändig. Die wichtigsten Verbesserungen wären:

1. **Dokumentation der Wildcards** (-1, -2, -3) im Pattern-Filter
2. **Batch-Abfragen** für lokale Siteswaps
3. **Graph-basierte Suche** für Passing-Kreise mit Constraints
4. **Bessere Dokumentation** der mathematischen Zusammenhänge

Diese Features bzw. deren Dokumentation würden die Suche nach spezifischen Passing-Patterns deutlich vereinfachen.
