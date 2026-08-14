# Juggling

## Dependency boundaries

The dependency analyzer uses the node IDs in this diagram together with
`architecture.arch.json`. The labels are intentionally user-facing; the JSON
file maps each ID to the actual C# namespace prefixes.

<!-- arch-analyzer -->
```mermaid
flowchart TD
    Domain["Siteswap Details<br/>Domain"]
    GeneratorCore["Siteswaps Generator Core<br/>Pattern generation"]
    GeneratorUI["Generator UI<br/>CardStack and Wizard"]
    SharedComponents["Shared Components<br/>Reusable UI"]
    Mcp["MCP Server<br/>AI integration"]
    Web["Web App<br/>Blazor host"]
    Hosting["AppHost<br/>Orchestration"]
    Tests["Tests"]
    Benchmarks["Benchmarks"]

    GeneratorCore --> Domain
    GeneratorUI --> GeneratorCore
    SharedComponents --> Domain
    Mcp --> GeneratorCore
    Mcp --> Domain
    Web --> GeneratorUI
    Web --> SharedComponents
    Hosting --> Web
    Hosting --> Mcp
    Tests --> Domain
    Tests --> GeneratorCore
    Tests --> GeneratorUI
    Tests --> Mcp
    Tests --> Web
    Benchmarks --> GeneratorCore
```

Only the directed edges shown above are allowed between mapped namespaces.
External and unmapped namespaces remain outside this architecture boundary.
