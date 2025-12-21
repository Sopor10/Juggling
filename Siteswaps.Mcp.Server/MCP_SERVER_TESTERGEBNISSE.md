# MCP Server "siteswap-generator" - Testergebnisse

**Testdatum:** 21. Dezember 2025  
**Tester:** AI-basierte systematische Analyse

---

## Zusammenfassung

Der MCP-Server bietet 14 Tools für Siteswap-Operationen und 161 Ressourcen mit Dokumentation. Die grundlegende Funktionalität ist teilweise vorhanden, jedoch wurden **schwerwiegende Probleme** identifiziert:

### Statistik der gefundenen Probleme:
- 🔴 **4 kritische Fehler** (Pattern-Filter, personalizedNumberFilter funktionieren nicht)
- 🟠 **2 Typfehler** (numberOfJugglers, numberOfPasses - machen Passing unbenutzbar)
- 🟡 **8+ stille Fehlschläge** (ungültige Eingaben werden ignoriert)
- 🔵 **10+ Dokumentationsmängel** (undokumentierte/nicht funktionierende Parameter)

---

## 🟠 Typfehler

### 5. numberOfJugglers Parameter-Typfehler

```
Eingabe: period=4, numberOfObjects=5, numberOfJugglers=2
Fehler: "Parameter 'numberOfJugglers' must be of type integer,null, got number"
```

### 6. numberOfPasses Parameter-Typfehler

```
Eingabe: period=3, numberOfObjects=7, numberOfPasses=1
Fehler: "Parameter 'numberOfPasses' must be of type integer,null, got number"
```

**Problem:** Die Parameter-Typen `numberOfJugglers` und `numberOfPasses` sind in der API-Definition als `integer,null` definiert, aber der Server akzeptiert keine Zahlen. Dies macht Passing-Siteswap-Generierung komplett unbenutzbar.

---

## 🟡 Fehlende/Schlechte Fehlermeldungen

### 7. Generische Fehlermeldungen ohne Details

| Tool | Situation | Fehlermeldung |
|------|-----------|---------------|
| `calculate_transitions` | Ungültiger Siteswap | `"An error occurred invoking 'calculate_transitions'."` |
| `swap_positions` | Negative Position (-1) | `"An error occurred invoking 'swap_positions'."` |
| `get_local_siteswap` | Ungültiger Juggler-Index (5 bei 2 Jongleuren) | `"An error occurred invoking 'get_local_siteswap'."` |
| `generate_causal_diagram` | Ungültiger Siteswap | `"An error occurred invoking 'generate_causal_diagram'."` |
| `generate_siteswaps` | period=0 | `"An error occurred invoking 'generate_siteswaps'."` |

**Empfehlung:** Fehlermeldungen sollten konkret beschreiben, was falsch ist:
- "Invalid siteswap: '513' has collisions at beat 2"
- "Position -1 is out of range (valid: 0-2)"
- "Juggler index 5 exceeds number of jugglers (2)"

### 8. Stille Fehlschläge

| Tool | Situation | Verhalten | Erwartetes Verhalten |
|------|-----------|-----------|---------------------|
| `swap_positions` | position2=99 (außerhalb) | ✅ Wirft jetzt ArgumentException mit Range-Hinweis | Fehlermeldung |
| `generate_siteswaps` | numberOfObjects=-1 | Leeres Array `[]` | Fehlermeldung |
| `generate_siteswaps` | minHeight > maxHeight | Leeres Array `[]` | Fehlermeldung |
| `generate_siteswaps` | minOccurrence:invalid | Normale Ergebnisse | Fehlermeldung |
| `generate_siteswaps` | state:invalid | Leeres Array `[]` | Fehlermeldung |
| `generate_siteswaps` | period:100, timeout:1 | Leeres Array `[]` | Timeout-Meldung |
| `calculate_transitions` | "3" → "5" (unterschiedliche Ballanzahl) | ✅ Wirft jetzt ArgumentException mit Hinweis auf unterschiedliche Ballanzahl | Erklärung, dass Transition unmöglich |

---

## 🔵 Dokumentationsmängel

### 6. Fehlende Erklärungen in Tool-Beschreibungen

**generate_siteswaps:**
- `minOccurrence`, `maxOccurrence`, `exactOccurrence`: Format "5:2" wird erwähnt, aber Bedeutung unklar
- `flexiblePattern`: "semicolon-separated groups" ohne Beispiel der Auswirkung - Tests zeigen keine klare Filterung
- `notFilter`: Komplexe Syntax ohne Anwendungsbeispiele, funktioniert nur teilweise
- `patternRotation`: Wird in der Beschreibung erwähnt, Parameter existiert nicht in der API
- `pattern`: Parameter mit negativen Werten (-1, -2, -3) dokumentiert, aber scheint nicht zu funktionieren
- `personalizedNumberFilter`: Dokumentiert, wird aber ignoriert
- `rotationAwarePattern`: Dokumentiert, keine sichtbare Auswirkung in Tests
- `jugglerIndex`: Dokumentiert, aber zusammen mit numberOfJugglers wegen Typfehler nicht nutzbar
- `useDefaultFilter`/`useNoFilter`: Keine sichtbare Auswirkung in Tests

**get_local_siteswap:**
- Ergebnis enthält `"globalNotation":"768"` für Eingabe `"786"` - es ist unklar, warum die Reihenfolge geändert wird
- `"averageObjectsPerJuggler":3.5` - Bedeutung von halben Bällen nicht erklärt

### 7. Unklare Konzepte ohne Kontext

**calculate_transitions:**
- Ergebnis `"throws":[],"length":0` bedeutet "direkte Transition möglich" - das ist nicht dokumentiert
- Unterschied zwischen "keine Transition möglich" (leeres Array) und "direkte Transition" (Array mit length=0) ist unklar

**generate_causal_diagram:**
- `"time":0.5` - Bedeutung der halben Zeitschritte nicht erklärt
- Unterschied zwischen `fromHeight` und `toHeight` in Transitions unklar

---

## ✅ Korrekt funktionierende Features

### Tools die wie erwartet funktionieren:

1. **analyze_siteswap** - Liefert detaillierte Analyse mit Orbits, States, und Eigenschaften
2. **generate_state_graph** - Korrekte Zustandsdiagramme
3. **generate_transition_graph** - Visualisiert Übergänge zwischen Siteswaps
4. **normalize_siteswap** - Rotationen werden korrekt normalisiert (`315`, `153` → `531`)
5. **simulate_throw** - Zeigt korrekt Zustandsübergänge
6. **swap_positions** - Funktioniert für gültige Positionen (`531` pos 0↔1 → `441`)

### Ressourcen:

Die 161 Ressourcen bieten umfangreiche Dokumentation zu:
- Definitionen (Siteswap, Orbit, State, etc.)
- Regeln (Validity, Averaging Theorem, etc.)
- Beispiele (valide/invalide Patterns)
- 4-Handed Siteswap / Passing Patterns

---

## Empfehlungen

### Priorität 1 (Kritisch - Blockierend):
1. **Pattern-Filter reparieren:** `pattern` Parameter filtert aktuell nicht - Kernfunktionalität defekt
2. **Typfehler beheben:** `numberOfJugglers` und `numberOfPasses` als Integer akzeptieren - Passing-Generierung komplett unbenutzbar
3. **personalizedNumberFilter implementieren:** Wird aktuell ignoriert

### Priorität 2 (Wichtig):
4. **Validierung verbessern:** Unrealistische Wurfhöhen (>15) sollten eine Warnung auslösen
5. **Spezifische Fehlermeldungen:** Statt "An error occurred" konkrete Ursachen nennen
6. **Stille Fehler eliminieren:** Ungültige Parameter sollten Fehler werfen:
   - `minHeight > maxHeight`
   - `numberOfObjects < 0`
   - `state:invalid`
   - `minOccurrence:invalid`

### Priorität 3 (Dokumentation):
7. **Nicht funktionierende Parameter dokumentieren oder entfernen:**
   - `useDefaultFilter` / `useNoFilter` - keine sichtbare Auswirkung
   - `rotationAwarePattern` - keine sichtbare Auswirkung
   - `patternRotation` - in Beschreibung erwähnt, existiert nicht
8. **Beispiele hinzufügen:** Für `flexiblePattern`, `notFilter`, `pattern` mit negativen Werten
9. **Ergebnis-Erklärungen:** Was bedeuten die einzelnen Felder in den Responses?
10. **Edge Cases dokumentieren:** Was passiert bei Timeouts, leeren Eingaben, Grenzen?

---

## Anhang: Test-Protokoll

### validate_siteswap Tests
```
"531" → true ✓
"441" → true ✓
"a7242" → true ✓
"" → false ✓
"513" → false ✓
"543" → false ✓
"-1" → false ✓
"999" → true ✓
"123" → true (prüfen ob korrekt)
"xyz" → true (potentiell problematisch)
```

### analyze_siteswap Tests
```
"531" → period:3, objects:3, orbits:[501,030] ✓
"441" → period:3, objects:3, orbits:[441] ✓
"a7242" → period:5, objects:5, isExcitedState:true ✓
"xyz" → period:3, objects:34, maxHeight:35 (⚠️ keine Warnung)
```

### generate_siteswaps Tests

#### Basis-Tests
```
period:3, objects:3, max:5 → [531,522,504,441,423] ✓
period:1, objects:3, max:9 → ["3"] ✓
period:2, objects:3, max:9 → [60,51,42] ✓
period:1, objects:4, max:9 → ["4"] ✓
period:1, objects:5, max:9 → ["5"] ✓
```

#### Occurrence-Filter Tests
```
minOccurrence:5:1 → Filtert korrekt (nur Siteswaps mit mind. 1x 5) ✓
maxOccurrence:5:1 → Filtert korrekt (max 1x 5) ✓
exactOccurrence:5:2 → Filtert korrekt (genau 2x 5) ✓
minOccurrence:5:1,7:1 → AND-Logik funktioniert (mind. 1x 5 UND 1x 7) ✓
minOccurrence:5:1|7:1 → OR-Logik funktioniert (mind. 1x 5 ODER 1x 7) ✓
minOccurrence:3,4:2 → Mehrere Zahlen mit gleicher Mindestanzahl ✓
```

#### Pattern-Filter Tests (⚠️ PROBLEME)
```
pattern:5,3,1 → Gibt ALLE Siteswaps zurück, filtert NICHT! ❌
pattern:4,4,1 → Gibt ALLE Siteswaps zurück, filtert NICHT! ❌
pattern:-1 (empty/egal) → Scheint ignoriert zu werden ❌
pattern:-2 (any self/P) → Scheint ignoriert zu werden ❌
```

#### notFilter Tests
```
notFilter:pattern:4,4,1 → Gibt ALLE Siteswaps inkl. 441 zurück! ❌
notFilter:minOccurrence:5:1 → Funktioniert (keine 5er) ✓
notFilter:minOccurrence:7:1|maxOccurrence:3:0 → OR-Logik ✓
```

#### flexiblePattern Tests
```
flexiblePattern:5,7;3,4;0,1,2 → Gibt Ergebnisse zurück, unklar ob korrekt gefiltert
flexiblePattern:7;5;3;1 → Identische Ergebnisse wie 7,9;5;3;1 ⚠️
```

#### state-Filter Tests
```
state:1,1,1,1 → Filtert auf Ground State ✓
state:1,1,0,1,1 → Filtert auf Excited State ✓
state:invalid → Leeres Array ohne Fehlermeldung ❌
```

#### personalizedNumberFilter Tests
```
personalizedNumberFilter:7:1:atleast:0 → Scheint ignoriert zu werden ❌
personalizedNumberFilter:5:2:exact:0 → Scheint ignoriert zu werden ❌
```

#### Fehler-Tests
```
period:0 → "An error occurred invoking 'generate_siteswaps'" ❌
numberOfObjects:-1 → Leeres Array [] ohne Fehler ❌
minHeight:5, maxHeight:3 → Leeres Array [] ohne Fehler ❌
maxResults:0 → Leeres Array [] (korrektes Verhalten?)
minOccurrence:invalid → Wird ignoriert, normale Ergebnisse ❌
numberOfJugglers:2 → Typfehler (integer,null vs number) ❌
numberOfPasses:1 → Typfehler (integer,null vs number) ❌
period:100, timeout:1 → Leeres Array ohne Timeout-Meldung ⚠️
```

#### useDefaultFilter/useNoFilter Tests
```
useDefaultFilter:false → Keine sichtbare Auswirkung ⚠️
useNoFilter:true → Keine sichtbare Auswirkung ⚠️
```

### swap_positions Tests
```
"531" pos 0↔1 → "441" ✓
"531" pos 0↔2 → "333" ✓
"531" pos -1↔0 → Fehler
"531" pos 0↔99 → "531" (stiller Fehlschlag)
```

### normalize_siteswap Tests
```
"531" → "531" ✓
"315" → "531" ✓
"153" → "531" ✓
"xyz" → "zxy" (normalisiert, aber keine Warnung)
```

### get_local_siteswap Tests
```
"786" juggler:0 → localNotation:"3,5 3 4" ✓
"786" juggler:1 → localNotation:"4 3,5 3" ✓
"786" juggler:5 → Fehler (ohne Details)
```

---

## Vorschlag: Filter-DSL für generate_siteswaps

### Problem der aktuellen Struktur

Die aktuelle API hat **15+ separate Filter-Parameter**, die:
1. Nicht flexibel kombinierbar sind (nur implizites AND)
2. Inkonsistente Syntax haben (`5:1`, `5:1,7:1`, `5:1|7:1`)
3. Teilweise nicht funktionieren
4. Keine echte Baumstruktur erlauben

**Beispiel: Unmöglich mit aktueller API:**
> "Finde Siteswaps mit (mindestens 2x die 7 ODER genau 1x die 9) UND (keine 3er) UND (beginnt mit 5 oder 7)"

### Lösung: String-basierte Filter-DSL

Statt vieler Parameter ein einziger `filter`-Parameter mit lesbarer DSL:

```
filter: "(minOcc(7,2) OR exactOcc(9,1)) AND maxOcc(3,0) AND (startsWith(5) OR startsWith(7))"
```

---

## DSL-Spezifikation

### Grundstruktur

```
filter := expression
expression := term (OR term)*
term := factor (AND factor)*
factor := NOT? (atom | '(' expression ')')
atom := function | keyword
```

### Logische Operatoren

| Operator | Bedeutung | Priorität |
|----------|-----------|-----------|
| `NOT` | Negation | 1 (höchste) |
| `AND` | Konjunktion | 2 |
| `OR` | Disjunktion | 3 (niedrigste) |
| `()` | Gruppierung | - |

**Beispiele:**
```
minOcc(5,1) AND maxOcc(3,0)              // Beide müssen erfüllt sein
minOcc(7,2) OR exactOcc(9,1)             // Mindestens eine muss erfüllt sein
NOT contains(441)                         // Darf nicht 441 enthalten
(minOcc(5,1) OR minOcc(7,1)) AND ground   // Gruppierung
```

---

### Filter-Funktionen

#### 1. Occurrence-Filter

| Funktion | Syntax | Beschreibung |
|----------|--------|--------------|
| `minOcc` | `minOcc(throw, count)` | Wurf `throw` kommt mindestens `count` mal vor |
| `maxOcc` | `maxOcc(throw, count)` | Wurf `throw` kommt höchstens `count` mal vor |
| `exactOcc` | `exactOcc(throw, count)` | Wurf `throw` kommt genau `count` mal vor |
| `occ` | `occ(throw, min, max)` | Wurf `throw` kommt zwischen `min` und `max` mal vor |

**Beispiele:**
```
minOcc(5,2)        // Mindestens 2x die 5
maxOcc(0,1)        // Höchstens 1x die 0 (eine Lücke erlaubt)
exactOcc(7,3)      // Genau 3x die 7
occ(5,1,3)         // Zwischen 1 und 3 mal die 5
```

**Mehrere Würfe:**
```
minOcc([5,7],2)    // Mind. 2x die 5 UND mind. 2x die 7
minOcc(5|7,2)      // Mind. 2x die 5 ODER mind. 2x die 7
```

#### 2. Pattern-Filter

| Funktion | Syntax | Beschreibung |
|----------|--------|--------------|
| `pattern` | `pattern(p1,p2,...)` | Exaktes Muster (Rotation egal) |
| `startsWith` | `startsWith(p1,p2,...)` | Beginnt mit diesem Muster |
| `endsWith` | `endsWith(p1,p2,...)` | Endet mit diesem Muster |
| `contains` | `contains(p1,p2,...)` | Enthält diese Sequenz |

**Wildcards:**
- `*` = beliebiger Wurf
- `S` = Self (gerade Zahl bei 4-handed)
- `P` = Pass (ungerade Zahl bei 4-handed)
- `H` = Hold (0 oder 2)

**Beispiele:**
```
pattern(5,3,1)           // Exakt 531 (oder Rotation davon)
pattern(7,*,*,*)         // Beginnt mit 7, Rest beliebig
startsWith(5)            // Beginnt mit 5
startsWith(5,*)          // Beginnt mit 5, dann beliebig
endsWith(1)              // Endet mit 1
contains(4,4,1)          // Enthält irgendwo 441
contains(S,P,S)          // Enthält Self-Pass-Self Sequenz
```

#### 3. State-Filter

| Funktion/Keyword | Syntax | Beschreibung |
|------------------|--------|--------------|
| `ground` | `ground` | Nur Ground-State Siteswaps |
| `excited` | `excited` | Nur Excited-State Siteswaps |
| `state` | `state(1,1,0,1,1)` | Exakter Start-State |

**Beispiele:**
```
ground                   // Ground State (z.B. 531, 441)
excited                  // Excited State (z.B. a7242)
state(1,1,1,1)           // 4-Ball Ground State
state(1,1,0,1,1)         // Spezifischer Excited State
```

#### 4. Struktur-Filter

| Funktion/Keyword | Syntax | Beschreibung |
|------------------|--------|--------------|
| `height` | `height(min,max)` | Wurfhöhe im Bereich |
| `maxHeight` | `maxHeight(n)` | Maximale Wurfhöhe |
| `minHeight` | `minHeight(n)` | Minimale Wurfhöhe |
| `noZeros` | `noZeros` | Keine Nullen (Lücken) |
| `hasZeros` | `hasZeros` | Muss Nullen enthalten |
| `symmetric` | `symmetric` | Symmetrisches Pattern |
| `prime` | `prime` | Prim-Siteswap (nicht zerlegbar) |

**Beispiele:**
```
height(3,7)              // Alle Würfe zwischen 3 und 7
maxHeight(7)             // Kein Wurf über 7
noZeros                  // Keine Lücken
symmetric                // z.B. 531 (5+1=6, 3+3=6)
```

#### 5. Orbit-Filter

| Funktion | Syntax | Beschreibung |
|----------|--------|--------------|
| `orbits` | `orbits(n)` | Genau n Orbits |
| `minOrbits` | `minOrbits(n)` | Mindestens n Orbits |
| `maxOrbits` | `maxOrbits(n)` | Höchstens n Orbits |
| `singleOrbit` | `singleOrbit` | Nur ein Orbit (alle Bälle gleich) |

**Beispiele:**
```
orbits(2)                // Genau 2 Orbits (z.B. 531: [501] + [030])
singleOrbit              // Ein Orbit (z.B. 441: [441])
```

#### 6. Passing-Filter (für 4-Handed Siteswaps)

| Funktion | Syntax | Beschreibung |
|----------|--------|--------------|
| `passes` | `passes(n)` | Genau n Pässe pro Periode |
| `minPasses` | `minPasses(n)` | Mindestens n Pässe |
| `maxPasses` | `maxPasses(n)` | Höchstens n Pässe |
| `passRatio` | `passRatio(min,max)` | Pass-Anteil (0.0-1.0) |
| `localValid` | `localValid(juggler)` | Lokal gültig für Jongleur |
| `compatible` | `compatible` | Kompatibel (beide Jongleure gleich) |

**Beispiele:**
```
passes(2)                // 2 Pässe pro Periode
passRatio(0.3,0.5)       // 30-50% Pässe
localValid(0)            // Gültig aus Sicht von Jongleur A
compatible               // Beide Jongleure machen dasselbe
```

---

### Vollständige Beispiele

#### Beispiel 1: Einfache Suche
> "3-Ball Siteswaps mit mindestens einer 5"

```
filter: "minOcc(5,1)"
```

#### Beispiel 2: Ausschluss
> "4-Ball Siteswaps ohne 441"

```
filter: "NOT contains(441)"
```

#### Beispiel 3: Kombination
> "5-Ball Siteswaps im Ground State mit mindestens 2x die 7 aber ohne 0er"

```
filter: "ground AND minOcc(7,2) AND noZeros"
```

#### Beispiel 4: Alternativen
> "3-Ball Siteswaps die entweder mit 5 oder mit 7 beginnen"

```
filter: "startsWith(5) OR startsWith(7)"
```

#### Beispiel 5: Komplex
> "4-Ball Siteswaps: (mindestens 2x die 7 ODER genau 1x die 9) UND keine 3er UND Ground State"

```
filter: "(minOcc(7,2) OR exactOcc(9,1)) AND maxOcc(3,0) AND ground"
```

#### Beispiel 6: Passing
> "7-Ball Passing Siteswaps mit 2 Pässen, kompatibel für beide Jongleure"

```
filter: "passes(2) AND compatible"
```

#### Beispiel 7: Muster mit Wildcards
> "Siteswaps die mit 7 beginnen, dann beliebig, dann mit 5 enden"

```
filter: "pattern(7,*,*,5)"
```

#### Beispiel 8: Negierte Gruppe
> "Nicht (531 oder 441)"

```
filter: "NOT (pattern(531) OR pattern(441))"
```

---

### Tool-Aufruf mit DSL

```json
{
  "period": 4,
  "numberOfObjects": 4,
  "minHeight": 0,
  "maxHeight": 9,
  "filter": "(minOcc(7,2) OR exactOcc(9,1)) AND ground AND NOT contains(441)",
  "maxResults": 20
}
```

**Parameter-Übersicht:**

| Parameter | Typ | Pflicht | Beschreibung |
|-----------|-----|---------|--------------|
| `period` | int | ✅ | Periode des Siteswaps |
| `numberOfObjects` | int | ✅ | Anzahl Bälle/Objekte |
| `minHeight` | int | ✅ | Minimale Wurfhöhe (global) |
| `maxHeight` | int | ✅ | Maximale Wurfhöhe (global) |
| `filter` | string | ❌ | Filter-DSL Expression |
| `numberOfJugglers` | int | ❌ | Anzahl Jongleure (für Passing) |
| `maxResults` | int | ❌ | Max. Anzahl Ergebnisse (default: 100) |
| `timeout` | int | ❌ | Timeout in Sekunden (default: 30) |

---

### Fehlerbehandlung

Bei Syntaxfehlern in der DSL sollte eine klare Fehlermeldung zurückgegeben werden:

```json
{
  "error": "FilterSyntaxError",
  "message": "Unexpected token 'XYZ' at position 15",
  "filter": "minOcc(5,1) XYZ maxOcc(3,0)",
  "position": 15,
  "expected": ["AND", "OR", ")"]
}
```

---

### Vorteile der DSL

| Aspekt | Alte API | Neue DSL |
|--------|----------|----------|
| **Lesbarkeit** | `"5:2,7:1\|9:1"` | `"minOcc(5,2) AND (minOcc(7,1) OR minOcc(9,1))"` |
| **Kombinierbarkeit** | Nur implizites AND | Beliebige AND/OR/NOT-Bäume |
| **LLM-Nutzung** | 15+ Parameter verstehen | Ein lesbarer String |
| **Erweiterbarkeit** | Neuer Parameter | Neue Funktion hinzufügen |
| **Dokumentation** | Pro Parameter | Eine DSL-Referenz |
| **Fehlermeldungen** | Stille Fehlschläge | Parser-Fehlermeldungen |

---

### Implementierung (Grammatik)

```ebnf
filter      = expression ;
expression  = term { "OR" term } ;
term        = factor { "AND" factor } ;
factor      = [ "NOT" ] ( atom | "(" expression ")" ) ;
atom        = function | keyword ;

function    = identifier "(" arglist ")" ;
arglist     = [ arg { "," arg } ] ;
arg         = number | identifier | list | range ;
list        = "[" number { "," number } "]" ;
range       = number "|" number ;

keyword     = "ground" | "excited" | "noZeros" | "hasZeros" 
            | "symmetric" | "prime" | "singleOrbit" | "compatible" ;

identifier  = letter { letter | digit } ;
number      = digit { digit } ;
```

### Implementierung (C# Pseudocode)

```csharp
public interface IFilter
{
    bool Matches(Siteswap siteswap);
}

public static class FilterParser
{
    public static IFilter Parse(string dsl)
    {
        var tokens = Tokenize(dsl);
        return ParseExpression(tokens);
    }
    
    // Recursive descent parser für die Grammatik
    private static IFilter ParseExpression(TokenStream tokens) { ... }
    private static IFilter ParseTerm(TokenStream tokens) { ... }
    private static IFilter ParseFactor(TokenStream tokens) { ... }
    private static IFilter ParseAtom(TokenStream tokens) { ... }
}

public class AndFilter : IFilter
{
    public List<IFilter> Conditions { get; }
    public bool Matches(Siteswap ss) => Conditions.All(c => c.Matches(ss));
}

public class OrFilter : IFilter
{
    public List<IFilter> Conditions { get; }
    public bool Matches(Siteswap ss) => Conditions.Any(c => c.Matches(ss));
}

public class NotFilter : IFilter
{
    public IFilter Inner { get; }
    public bool Matches(Siteswap ss) => !Inner.Matches(ss);
}

public class MinOccurrenceFilter : IFilter
{
    public int Throw { get; }
    public int Count { get; }
    public bool Matches(Siteswap ss) => ss.Throws.Count(t => t == Throw) >= Count;
}
```

---

## Fazit

Der MCP-Server hat eine solide Grundstruktur mit gut funktionierenden Core-Tools (`analyze_siteswap`, `normalize_siteswap`, `generate_state_graph`, etc.) und einer umfangreichen Ressourcen-Dokumentation.

**Jedoch sind die erweiterten Filter-Funktionen von `generate_siteswaps` größtenteils defekt:**
- `pattern` filtert nicht
- `personalizedNumberFilter` wird ignoriert
- `numberOfJugglers`/`numberOfPasses` haben Typfehler

Dies schränkt den praktischen Nutzen erheblich ein, da die gezielte Suche nach spezifischen Patterns nicht möglich ist.

**Die dringendsten Fixes sollten sein:**
1. Pattern-Filter reparieren
2. Typfehler für Passing-Parameter beheben
3. Bessere Fehlermeldungen implementieren

---

*Generiert durch systematische Tool-Tests am 21.12.2025*

