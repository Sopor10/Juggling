# MCP Server Erweiterungsvorschläge

## Analyse: Siteswap Library vs. MCP Tools

### Verfügbare Funktionalitäten in der Siteswap Library

#### 1. **Kern-Funktionalitäten (bereits teilweise verfügbar)**
- ✅ Siteswap-Erstellung und Validierung → `ValidateSiteswapTool`
- ✅ Orbit-Analyse → `AnalyzeSiteswapTool` (enthält Orbits)
- ✅ State-Analyse → `AnalyzeSiteswapTool` (enthält States)
- ✅ Period, NumberOfObjects, MaxHeight → `AnalyzeSiteswapTool`

#### 2. **Fehlende Funktionalitäten im MCP Server**

##### A. **Transition-Berechnung** ✅ IMPLEMENTIERT
- **Funktion**: `PossibleTransitions(Siteswap to, int length, int? height = null)`
- **Beschreibung**: Berechnet alle möglichen Übergänge zwischen zwei Siteswaps
- **Use Case**: Finden von Wegen zwischen Siteswap-Patterns, Analyse von Routinen
- **Status**: ✅ Implementiert als `CalculateTransitionsTool`

##### B. **LocalSiteswap (Multi-Juggler Support)** ✅ IMPLEMENTIERT
- **Funktion**: `GetLocalSiteswap(int juggler, int numberOfJugglers)`
- **Beschreibung**: Konvertiert globales Siteswap in lokale Notation für einen spezifischen Juggler
- **Use Case**: Passing-Patterns analysieren, lokale vs. globale Notation vergleichen
- **Status**: ✅ Implementiert als `GetLocalSiteswapTool`

##### C. **Swap-Operation**
- **Funktion**: `Swap(int x, int y)`
- **Beschreibung**: Tauscht zwei Positionen im Siteswap und passt Werte an
- **Use Case**: Siteswap-Modifikation, Experimentieren mit Varianten

##### D. **CausalDiagram-Generierung**
- **Funktion**: `CausalDiagramGenerator.Generate(Siteswap, CyclicArray<Hand>)`
- **Beschreibung**: Erstellt ein kausales Diagramm für Visualisierung
- **Use Case**: Visualisierung von Siteswap-Patterns, Verständnis der Ball-Bewegungen

##### E. **StateGraph-Generierung**
- **Funktion**: `StateGraphFromSiteswapGenerator.CalculateGraph(Siteswap)`
- **Beschreibung**: Erstellt einen State-Graph für ein Siteswap
- **Use Case**: Visualisierung von State-Übergängen, Analyse der Pattern-Struktur

##### F. **Throw-Simulation**
- **Funktion**: `Throw()` → `(Siteswap nSiteswap, Throw nThrow)`
- **Beschreibung**: Simuliert einen einzelnen Wurf und gibt neuen State zurück
- **Use Case**: Schritt-für-Schritt-Analyse, Debugging von Siteswaps

##### G. **Siteswap-Vergleich und Normalisierung**
- **Funktion**: `ToUniqueRepresentation()`, `Equals()`, `Rotate()`
- **Beschreibung**: Normalisiert Siteswaps, vergleicht sie, rotiert sie
- **Use Case**: Duplikat-Erkennung, Normalisierung von Eingaben

##### H. **TransitionGraph für Siteswap-Listen**
- **Funktion**: `SiteswapList.TransitionGraph(int length)`
- **Beschreibung**: Erstellt einen Graph aller möglichen Transitionen zwischen mehreren Siteswaps
- **Use Case**: Analyse von Siteswap-Sets, Routinen-Planung

---

## Vorschläge für MCP Server Erweiterungen

### Priorität 1: Hochwertige Erweiterungen

#### 1. **CalculateTransitions Tool** ✅ IMPLEMENTIERT
- **Status**: ✅ Implementiert und getestet
- **Datei**: `Siteswaps.Mcp.Server/Tools/CalculateTransitionsTool.cs`
- **Tests**: `Siteswaps.Mcp.Server.Test/CalculateTransitionsToolTests.cs`

#### 2. **GetLocalSiteswap Tool** ✅ IMPLEMENTIERT
- **Status**: ✅ Implementiert und getestet
- **Datei**: `Siteswaps.Mcp.Server/Tools/GetLocalSiteswapTool.cs`
- **Tests**: `Siteswaps.Mcp.Server.Test/GetLocalSiteswapToolTests.cs`

#### 3. **GenerateStateGraph Tool**
```csharp
public class GenerateStateGraphTool
{
    [McpServerTool]
    [Description("Generates a state graph for a siteswap showing all state transitions.")]
    public StateGraphInfo GenerateStateGraph(
        [Description("Siteswap string")] string siteswap
    )
}
```

**Vorteile:**
- Visualisierung der Pattern-Struktur
- Wichtig für Verständnis von Siteswaps
- Kann als Basis für weitere Visualisierungen dienen

### Priorität 2: Nützliche Erweiterungen

#### 4. **SimulateThrow Tool**
```csharp
public class SimulateThrowTool
{
    [McpServerTool]
    [Description("Simulates a single throw in a siteswap and returns the resulting state and siteswap.")]
    public ThrowSimulationResult SimulateThrow(
        [Description("Siteswap string")] string siteswap
    )
}
```

**Vorteile:**
- Schritt-für-Schritt-Analyse
- Hilfreich für Debugging
- Erklärt Siteswap-Mechanik

#### 5. **NormalizeSiteswap Tool**
```csharp
public class NormalizeSiteswapTool
{
    [McpServerTool]
    [Description("Normalizes a siteswap to its unique representation (canonical form).")]
    public string NormalizeSiteswap(
        [Description("Siteswap string")] string siteswap
    )
}
```

**Vorteile:**
- Duplikat-Erkennung
- Normalisierung von Eingaben
- Vergleich von Siteswaps

### Priorität 3: Erweiterte Funktionalitäten

#### 6. **GenerateCausalDiagram Tool**
```csharp
public class GenerateCausalDiagramTool
{
    [McpServerTool]
    [Description("Generates a causal diagram representation of a siteswap.")]
    public CausalDiagramInfo GenerateCausalDiagram(
        [Description("Siteswap string")] string siteswap,
        [Description("Number of hands")] int numberOfHands = 2
    )
}
```

**Vorteile:**
- Visualisierung der Ball-Bewegungen
- Wichtig für Verständnis komplexer Patterns
- Kann für Diagramm-Generierung verwendet werden

#### 7. **SwapPositions Tool**
```csharp
public class SwapPositionsTool
{
    [McpServerTool]
    [Description("Swaps two positions in a siteswap and adjusts values accordingly.")]
    public string SwapPositions(
        [Description("Siteswap string")] string siteswap,
        [Description("First position index (0-based)")] int position1,
        [Description("Second position index (0-based)")] int position2
    )
}
```

**Vorteile:**
- Experimentieren mit Siteswap-Varianten
- Modifikation von Patterns
- Erforschung von Siteswap-Eigenschaften

#### 8. **CompareSiteswaps Tool**
```csharp
public class CompareSiteswapsTool
{
    [McpServerTool]
    [Description("Compares two siteswaps and returns similarity information.")]
    public SiteswapComparison CompareSiteswaps(
        [Description("First siteswap")] string siteswap1,
        [Description("Second siteswap")] string siteswap2
    )
}
```

**Vorteile:**
- Findet ähnliche Patterns
- Duplikat-Erkennung
- Vergleich von Varianten

#### 9. **GenerateTransitionGraph Tool**
```csharp
public class GenerateTransitionGraphTool
{
    [McpServerTool]
    [Description("Generates a transition graph for a list of siteswaps showing all possible transitions between them.")]
    public TransitionGraphInfo GenerateTransitionGraph(
        [Description("Comma-separated list of siteswaps")] string siteswaps,
        [Description("Maximum transition length")] int maxLength
    )
}
```

**Vorteile:**
- Analyse von Siteswap-Sets
- Routinen-Planung
- Komplexe Pattern-Analyse

---

## Implementierungsempfehlungen

### Phase 1: Kern-Erweiterungen (Priorität 1)
1. ✅ `CalculateTransitionsTool` - Sehr nützlich für Routinen (IMPLEMENTIERT)
2. ✅ `GetLocalSiteswapTool` - Essentiell für Passing-Patterns (IMPLEMENTIERT)
3. `GenerateStateGraphTool` - Wichtig für Visualisierung

### Phase 2: Praktische Tools (Priorität 2)
4. `SimulateThrowTool` - Hilfreich für Verständnis
5. `NormalizeSiteswapTool` - Basis-Funktionalität

### Phase 3: Erweiterte Features (Priorität 3)
6. `GenerateCausalDiagramTool` - Visualisierung
7. `SwapPositionsTool` - Experimentieren
8. `CompareSiteswapsTool` - Vergleich
9. `GenerateTransitionGraphTool` - Komplexe Analyse

---

## Zusätzliche Überlegungen

### Performance
- `TransitionGraph` kann bei vielen Siteswaps langsam sein
- Timeouts für komplexe Berechnungen einbauen
- Caching für wiederholte Anfragen

### Fehlerbehandlung
- Validierung aller Eingaben
- Klare Fehlermeldungen
- Fallback-Verhalten

### Dokumentation
- Detaillierte Beschreibungen für jedes Tool
- Beispiele in den Tool-Beschreibungen
- README-Updates

### Testing
- Unit-Tests für jedes neue Tool
- Integration-Tests
- Edge-Case-Behandlung

