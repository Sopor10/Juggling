# Runtime View

## Scenario 1: Generate Siteswaps via Web UI

```mermaid
sequenceDiagram
    actor User
    participant UI as Blazor UI
    participant Gen as Generator
    participant Filter as Filters
    participant Domain as Siteswap

    User->>UI: Set parameters<br/>(period, objects, heights)
    User->>UI: Click Generate
    UI->>Gen: GenerateAsync(input, filters)
    
    loop Backtracking
        Gen->>Filter: IsValid(partialSiteswap)
        Filter-->>Gen: true/false
        alt Valid & Complete
            Gen->>Domain: new Siteswap(throws)
            Domain-->>Gen: validated siteswap
            Gen-->>UI: yield siteswap
            UI->>User: Display pattern
        end
    end
    
    Gen-->>UI: complete
```

### Steps

1. User configures generation parameters in web interface
2. UI creates `SiteswapGeneratorInput` and filters
3. Generator starts backtracking algorithm
4. For each position, try all possible throw heights
5. Filters prune invalid branches early
6. Complete valid patterns are yielded incrementally
7. UI displays patterns as they arrive

## Scenario 2: Analyze Pattern via MCP

```mermaid
sequenceDiagram
    actor AI as AI Assistant
    participant MCP as MCP Server
    participant Tool as AnalyzeTool
    participant Domain as Siteswap

    AI->>MCP: AnalyzeSiteswap("531")
    MCP->>Tool: AnalyzeSiteswap("531")
    Tool->>Domain: Siteswap.TryCreate("531")
    Domain-->>Tool: siteswap instance
    
    Tool->>Domain: GetOrbits()
    Domain-->>Tool: orbit list
    
    Tool->>Domain: State, NumberOfObjects, etc.
    Domain-->>Tool: properties
    
    Tool->>MCP: analysis result (JSON)
    MCP->>AI: formatted response
```

### Steps

1. AI assistant calls MCP tool with siteswap string
2. MCP server routes to `AnalyzeSiteswapTool`
3. Tool parses and validates siteswap
4. Tool queries domain model for properties
5. Tool calculates orbits, states, transitions
6. Results serialized to JSON
7. AI receives structured analysis data

## Scenario 3: Calculate Transitions

```mermaid
sequenceDiagram
    actor User
    participant UI as UI
    participant Domain as Siteswap
    participant Trans as TransitionCalculator

    User->>UI: Request transition<br/>"531" -> "441"
    UI->>Domain: from = new Siteswap("531")
    UI->>Domain: to = new Siteswap("441")
    
    UI->>Trans: CreateTransitions(from, to, length)
    
    loop For each candidate
        Trans->>Trans: Generate candidate sequence
        Trans->>Domain: Validate each throw
        alt Valid transition
            Trans->>Trans: Add to results
        end
    end
    
    Trans-->>UI: transition list
    UI->>User: Display transitions
```

### Steps

1. User requests transition between two patterns
2. Both patterns validated as siteswaps
3. TransitionCalculator explores possible sequences
4. Each candidate validated for mathematical correctness
5. Valid transitions collected and sorted
6. UI displays clickable transition sequences




