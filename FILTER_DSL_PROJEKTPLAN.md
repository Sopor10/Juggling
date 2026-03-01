# Projektplan: Filter-DSL Parser mit Pidgin

## Übersicht

Ersetzung des aktuellen Filter-Systems im `generate_siteswaps` Tool durch eine einheitliche, ausdrucksstarke Domain-Specific Language (DSL) mit einem Parser auf Basis der [Pidgin](https://github.com/benjamin-hodgson/Pidgin) Library.

**Referenzen:**
- [Pidgin GitHub Repository](https://github.com/benjamin-hodgson/Pidgin)
- [Pidgin Dokumentation v3.5.0](https://www.benjamin.pizza/Pidgin/v3.5.0/Pidgin)

---

## Phase 1: Grundlagen & Architektur

### 1.1 Projekt-Setup

**Ziel:** Neues Projekt für den Filter-Parser erstellen

- NuGet-Paket `Pidgin` (Version 3.5.x) hinzufügen
- implementierung wird im Ordner Tools/GenerateSiteswaps/FilterParser abgelegt

### 1.2 AST-Definition (Abstract Syntax Tree)

**Ziel:** Rein syntaktisches Modell ohne Wissen über konkrete Filter

**Designprinzip:** Der AST kennt keine Filter-Semantik. Er weiß nicht, was `minOcc` bedeutet oder welche Argumente erlaubt sind. Er repräsentiert nur die **syntaktische Struktur** des Ausdrucks.

```
FilterExpression (Union Type via Dunet)
├── And(FilterExpression Left, FilterExpression Right)
├── Or(FilterExpression Left, FilterExpression Right)
├── Not(FilterExpression Inner)
├── FunctionCall(string Name, Argument[] Args)
└── Identifier(string Name)                              // Parameterlose Keywords

Argument (Union Type via Dunet)
├── Number(int Value)
├── Wildcard                                             // *
├── NumberList(int[] Values)                             // [5,7,9]
└── Identifier(string Value)                             // für zukünftige Erweiterungen
```

**Was der AST NICHT weiß:**
- ❌ Ob `minOcc` ein gültiger Funktionsname ist
- ❌ Ob `minOcc(5,2)` die richtige Anzahl Argumente hat
- ❌ Ob ein Wildcard in `minOcc(*,2)` erlaubt ist
- ❌ Was die Argumente bedeuten

**Was der AST WEIß:**
- ✅ Es gibt einen Funktionsaufruf namens `"minOcc"` mit 2 Argumenten
- ✅ Das erste Argument ist eine Zahl `5`, das zweite ist `2`
- ✅ Der Ausdruck ist mit `AND` verknüpft

**Beispiel-AST für `"minOcc(5,2) AND ground"`:**
```
And(
    FunctionCall("minOcc", [Number(5), Number(2)]),
    Identifier("ground")
)
```

**Zusammenhänge:**
- Der AST ist **rein syntaktisch** - keine Domänenlogik
- Semantische Validierung erfolgt in **Phase 3** (separate Schicht)
- Ermöglicht einfaches Testen des Parsers ohne Filter-Wissen
- AST ist immutable, verwendet records mit Dunet (NuGet-Paket)
- Neue Filter-Funktionen erfordern **keine Änderung** am AST
---

## Phase 2: Lexer & Parser

### 2.1 Token-Definition

**Ziel:** Grundlegende Tokens der DSL definieren

| Token-Typ | Beispiele |
|-----------|-----------|
| Keyword | `AND`, `OR`, `NOT` |
| Identifier | `minOcc`, `pattern`, `ground` |
| Number | `0`, `5`, `42` |
| Punctuation | `(`, `)`, `,`, `[`, `]`, `*`, `\|` |
| Whitespace | Leerzeichen, Tabs (werden ignoriert) |

### 2.2 Parser-Komponenten (Bottom-Up)

**Ziel:** Modulare Parser-Bausteine mit Pidgin erstellen

#### Ebene 1: Primitive Parser
- **NumberParser**: Parst Integer-Zahlen (`0-9+`)
- **IdentifierParser**: Parst Funktionsnamen (`[a-zA-Z][a-zA-Z0-9]*`)
- **WildcardParser**: Parst `*` als Wildcard

#### Ebene 2: Argument-Parser
- **SingleArgParser**: Parst `5` oder `*`
- **NumberListParser**: Parst `[5,7,9]`
- **NumberRangeParser**: Parst `5|7` (OR-Semantik für Zahlen)
- **ArgListParser**: Parst `(arg1, arg2, ...)` mit beliebigen Argumenten

#### Ebene 3: Funktions-Parser
- **FunctionCallParser**: Parst `functionName(args)`
- **KeywordParser**: Parst parameterlose Keywords wie `ground`, `noZeros`
- **AtomParser**: Kombiniert FunctionCall und Keyword → liefert `FilterFunction`

#### Ebene 4: Ausdruck-Parser (Operator Precedence)
- **FactorParser**: Parst `NOT? (Atom | '(' Expr ')')`
- **TermParser**: Parst `Factor (AND Factor)*`
- **ExpressionParser**: Parst `Term (OR Term)*`

**Zusammenhänge:**
- Pidgin's `Parser.Rec()` ermöglicht rekursive Grammatiken (für verschachtelte Klammern)
- Operator-Präzedenz wird durch die Parser-Hierarchie abgebildet (Expression → Term → Factor)
- Jeder Parser liefert einen AST-Knoten zurück

### 2.3 Fehlerbehandlung

**Ziel:** Aussagekräftige Fehlermeldungen bei Syntaxfehlern

- Pidgin's `Expected()` für erwartete Tokens nutzen
- `Labelled()` für benannte Parser-Abschnitte
- Custom `ParseError`-Typ mit Position und Kontext
- Fehler-Recovery wo sinnvoll (z.B. bei fehlender schließender Klammer)

---

## Phase 3: Semantische Analyse

### 3.1 Funktions-Registry

**Ziel:** Validierung der Funktionsnamen und Argumenttypen

```
FilterFunctionRegistry
├── RegisterFunction(name, parameterTypes, evaluator)
├── GetFunction(name) → FunctionDefinition?
└── ValidateCall(name, arguments) → ValidationResult
```

**Registrierte Funktionen:**

| Funktion | Parameter | Beschreibung |
|----------|-----------|--------------|
| `minOcc` | `(int throw, int count)` | Mindest-Vorkommen |
| `maxOcc` | `(int throw, int count)` | Maximal-Vorkommen |
| `exactOcc` | `(int throw, int count)` | Exaktes Vorkommen |
| `occ` | `(int throw, int min, int max)` | Vorkommen im Bereich |
| `pattern` | `(int\|wildcard...)` | Muster-Match |
| `startsWith` | `(int...)` | Präfix-Match |
| `endsWith` | `(int...)` | Suffix-Match |
| `contains` | `(int...)` | Subsequenz-Match |
| `height` | `(int min, int max)` | Höhen-Bereich |
| `maxHeight` | `(int max)` | Maximale Höhe |
| `minHeight` | `(int min)` | Minimale Höhe |
| `orbits` | `(int count)` | Anzahl Orbits |
| `passes` | `(int count)` | Anzahl Pässe |
| `passRatio` | `(float min, float max)` | Pass-Verhältnis |
| `state` | `(int...)` | Start-State |
| `ground` | (keine) | Ground State |
| `excited` | (keine) | Excited State |
| `noZeros` | (keine) | Keine Nullen |
| `hasZeros` | (keine) | Hat Nullen |
| `symmetric` | (keine) | Symmetrisch |
| `prime` | (keine) | Prim-Siteswap |
| `singleOrbit` | (keine) | Ein Orbit |
| `compatible` | (keine) | Passing-kompatibel |

### 3.2 AST-Validierung

**Ziel:** Semantische Prüfung des geparsten AST

- Alle Funktionsnamen sind bekannt
- Argument-Anzahl stimmt
- Argument-Typen sind kompatibel
- Wildcards nur wo erlaubt (z.B. in `pattern`, nicht in `minOcc`)

---

## Phase 4: Mapping zum Generator

- nachdem die Validierung erfolgreich war, wird der AST in eine ausführbare Filter-Funktion umgewandelt
- der filterbuilder des siteswap generators wird verwendet, um die einzelnen funktionen zu erstellen
- rekursive Traversierung des ASTs, um die Filter-Funktion zusammenzusetzen

---

## Phase 5: Integration

### 5.1 API-Facade

**Ziel:** Einfache API für den MCP-Server

```
FilterParser
├── Parse(string dsl) → Result<IFilterExpression, ParseError>
├── Validate(IFilterExpression) → Result<Unit, ValidationError>
└── CreateFilter(string dsl) → Result<Func<Siteswap, bool>, FilterError>
```

**Zusammenhänge:**
- `CreateFilter` ist die Hauptmethode für den MCP-Server
- Kombiniert Parsing → Validierung → Kompilierung
- Gibt aussagekräftige Fehler zurück

### 5.2 MCP-Tool-Integration

**Ziel:** `generate_siteswaps` Tool anpassen

- Alten `filter`-Parameter durch neuen DSL-String ersetzen
- Alte Parameter (`minOccurrence`, `pattern`, etc.) als deprecated markieren
- Abwärtskompatibilität: Alte Parameter intern zu DSL konvertieren
- Fehlerausgabe im neuen strukturierten Format

### 5.3 Ressourcen-Dokumentation

**Ziel:** DSL-Dokumentation als MCP-Ressourcen bereitstellen

- `filter:dsl:syntax` → Syntax-Referenz
- `filter:dsl:functions` → Funktions-Übersicht
- `filter:dsl:examples` → Anwendungsbeispiele

---

## Phase 6: Testing

### 6.1 Unit Tests - Parser

| Test-Kategorie | Beispiele |
|----------------|-----------|
| Primitive | `"5"`, `"minOcc"`, `"*"` |
| Funktionsaufrufe | `"minOcc(5,2)"`, `"ground"` |
| Einfache Ausdrücke | `"A AND B"`, `"A OR B"`, `"NOT A"` |
| Verschachtelte Ausdrücke | `"(A OR B) AND C"`, `"NOT (A AND B)"` |
| Komplexe Ausdrücke | `"(minOcc(7,2) OR exactOcc(9,1)) AND ground"` |
| Whitespace | `"A  AND  B"`, `" A AND B "` |
| Fehler-Cases | `"minOcc()"`, `"AND"`, `"(A"`, `"A AND"` |

### 6.2 Unit Tests - Validierung

| Test-Kategorie | Beispiele |
|----------------|-----------|
| Unbekannte Funktion | `"unknownFunc(5)"` |
| Falsche Argumentanzahl | `"minOcc(5)"`, `"ground(true)"` |
| Falscher Argumenttyp | `"minOcc(*,2)"` |
| Gültige Aufrufe | Alle registrierten Funktionen |

### 6.3 Unit Tests - Evaluierung

| Test-Kategorie | Beispiele |
|----------------|-----------|
| Occurrence | `"minOcc(5,2)"` gegen `"5551"`, `"531"` |
| Pattern | `"pattern(5,*,1)"` gegen `"531"`, `"541"`, `"441"` |
| State | `"ground"` gegen `"531"`, `"a7242"` |
| Logik | `"A AND B"`, `"A OR B"`, `"NOT A"` |
| Komplex | Kombinationen aller Funktionen |

### 6.4 Integration Tests

- End-to-End: DSL-String → gefilterte Siteswap-Liste
- Performance: Große Siteswap-Mengen mit komplexen Filtern
- Fehlerbehandlung: Ungültige DSL → strukturierte Fehlermeldung

### 6.5 Property-Based Tests

- Für jeden gültigen AST gibt es einen äquivalenten DSL-String
- Parsing ist deterministisch (gleiches Input → gleiches Output)
- Logische Äquivalenzen: `NOT (A AND B)` ≡ `(NOT A) OR (NOT B)`

---

## Phase 7: Dokumentation & Rollout

### 7.1 Technische Dokumentation

- Parser-Architektur und Erweiterungspunkte
- Hinzufügen neuer Filter-Funktionen
- Fehlerbehandlung und Debugging

### 7.2 Benutzer-Dokumentation

- DSL-Syntax-Referenz
- Funktions-Katalog mit Beispielen
- Migration von alten Parametern

### 7.3 Rollout-Strategie

1. **Alpha**: Neuer `filter`-Parameter neben alten Parametern
2. **Beta**: Alte Parameter als deprecated, Warnungen
3. **Release**: Alte Parameter entfernt

---

## Architektur-Übersicht

```
┌─────────────────────────────────────────────────────────────────┐
│                        MCP Server                                │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                 GenerateSiteswapsTool                      │  │
│  │                                                            │  │
│  │   filter: "(minOcc(7,2) OR exactOcc(9,1)) AND ground"     │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                     FilterParser                           │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐   │  │
│  │  │   Lexer     │→ │   Parser    │→ │    Validator    │   │  │
│  │  │  (Pidgin)   │  │  (Pidgin)   │  │   (Registry)    │   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────┘   │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                         AST                                │  │
│  │                                                            │  │
│  │   AndExpression                                            │  │
│  │   ├── OrExpression                                         │  │
│  │   │   ├── FunctionCall("minOcc", [7, 2])                  │  │
│  │   │   └── FunctionCall("exactOcc", [9, 1])                │  │
│  │   └── FunctionCall("ground", [])                          │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    FilterEvaluator                         │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐   │  │
│  │  │ Occurrence  │  │   Pattern   │  │      State      │   │  │
│  │  │  Evaluator  │  │  Evaluator  │  │    Evaluator    │   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────┘   │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                   Siteswap Generator                       │  │
│  │                                                            │  │
│  │   Generiert Siteswaps → Filtert mit Evaluator → Ergebnis  │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Zeitschätzung

| Phase | Aufwand | Abhängigkeiten |
|-------|---------|----------------|
| 1. Grundlagen & Architektur | 2-3 Tage | - |
| 2. Lexer & Parser | 3-4 Tage | Phase 1 |
| 3. Semantische Analyse | 2-3 Tage | Phase 2 |
| 4. Evaluator | 3-4 Tage | Phase 3 |
| 5. Integration | 2-3 Tage | Phase 4 |
| 6. Testing | 3-4 Tage | Parallel zu Phase 2-5 |
| 7. Dokumentation & Rollout | 2-3 Tage | Phase 5 |

**Gesamt: ~17-24 Tage**

---

## Risiken & Mitigationen

| Risiko | Wahrscheinlichkeit | Mitigation |
|--------|-------------------|------------|
| Pidgin-Lernkurve | Mittel | Dokumentation studieren, kleine Beispiele erst |
| Performance bei komplexen Filtern | Niedrig | Compile-Schritt für Optimierung |
| Abwärtskompatibilität | Mittel | Alte Parameter intern zu DSL konvertieren |
| Unerwartete Grammatik-Ambiguitäten | Niedrig | Frühzeitig viele Edge Cases testen |

---

## Erfolgskriterien

1. ✅ Alle in der DSL-Spezifikation definierten Funktionen sind implementiert
2. ✅ Parser liefert aussagekräftige Fehlermeldungen mit Position
3. ✅ Performance ist vergleichbar oder besser als aktuelle Lösung
4. ✅ Testabdeckung > 90% für Parser und Evaluator
5. ✅ Dokumentation ist vollständig und mit Beispielen versehen
6. ✅ Migration von alten Parametern ist dokumentiert

---

*Erstellt: 21. Dezember 2025*

