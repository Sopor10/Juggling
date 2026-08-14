# Juggling

## Dependency boundaries

The project dependency graph is intentionally kept one-directional. The domain
(`Siteswap.Details`) is the innermost layer; UI, MCP, hosting, and test projects
may depend on it, but the domain must not depend on any of them.

```mermaid
flowchart TD
    Details["Siteswap.Details<br/>domain"]
    Generator["Siteswaps.Generator.Core<br/>generation"]
    Components["Siteswaps.Components<br/>shared UI"]
    Webassembly["Webassembly<br/>application"]
    Mcp["Siteswaps.Mcp.Server<br/>MCP adapter"]
    GeneratorMcp["Siteswaps.Generator.Mcp<br/>generator MCP adapter"]
    AppHost["Juggling.AppHost<br/>orchestration"]
    Tests["test projects"]

    Generator --> Details
    Components --> Details
    Webassembly --> Generator
    Webassembly --> Components
    Mcp --> Generator
    Mcp --> Details
    GeneratorMcp --> Generator
    GeneratorMcp --> Details
    AppHost --> Webassembly
    AppHost --> Mcp
    Tests -.-> Generator
    Tests -.-> Details

    classDef domain fill:#d5f5e3,stroke:#1e8449
    classDef adapter fill:#d6eaf8,stroke:#2874a6
    classDef host fill:#fdebd0,stroke:#ca6f1e
    class Details domain
    class Generator,Components adapter
    class Webassembly,Mcp,GeneratorMcp adapter
    class AppHost,Tests host
```

The solid arrows are allowed production dependencies. Dashed arrows represent
test-only dependencies. New project references should be added to this diagram
and covered by an architecture test before they are merged.
