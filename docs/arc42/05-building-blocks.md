# Building Block View

## Level 1: System Overview

```mermaid
graph TB
    subgraph "Siteswap System"
        Core[Siteswap.Details<br/>Domain Model]
        Gen[Siteswaps.Generator<br/>Pattern Generation]
        UI[Web UI<br/>User Interface]
        MCP[MCP Server<br/>AI Integration]
        Comp[Components<br/>Visualizations]
    end
    
    UI --> Comp
    UI --> Gen
    Comp --> Core
    Gen --> Core
    MCP --> Core
    MCP --> Gen
```

### Core Responsibilities

| Component | Responsibility |
|-----------|---------------|
| **Siteswap.Details** | Domain model, validation, analysis, mathematical operations |
| **Siteswaps.Generator** | Pattern generation with backtracking and filtering |
| **Components** | Reusable UI components for visualization |
| **Web UI** | User-facing application, layout, page routing |
| **MCP Server** | AI tool integration, external API |
