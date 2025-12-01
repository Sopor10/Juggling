# System Context

## Business Context

```mermaid
graph TB
    Juggler[Juggler/User]
    AI[AI Assistant]
    Browser[Web Browser]
    
    System[Siteswap System]
    
    Juggler -->|generates patterns| System
    Juggler -->|analyzes patterns| System
    AI -->|MCP protocol| System
    System -->|visualizations| Browser
    System -->|pattern data| Juggler
    System -->|analysis results| AI
```

### External Entities

| Entity | Interface | Purpose |
|--------|-----------|---------|
| **Juggler** | Web UI | Explore and learn patterns, plan routines |
| **AI Assistant** | MCP Server | Generate patterns via natural language, analyze specific patterns |
| **Web Browser** | HTTP/WebAssembly | Render UI, run client-side logic |

## Technical Context

```mermaid
graph TB
    subgraph "Client Side"
        Browser[Web Browser]
        WASM[Blazor WebAssembly]
    end
    
    subgraph "Server Side"
        MCP[MCP Server]
        WebHost[Web Host]
    end
    
    subgraph "Core Libraries"
        Details[Siteswap.Details]
        Generator[Generator.Core]
        Components[UI Components]
    end
    
    Browser --> WASM
    WASM --> Components
    Components --> Details
    Components --> Generator
    
    MCP --> Details
    MCP --> Generator
    
    WebHost --> WASM
```

### Technical Interfaces

| Interface | Technology | Data Format |
|-----------|------------|-------------|
| **Web UI** | Blazor WebAssembly + HTTP | HTML/CSS/JS, JSON |
| **MCP Server** | stdio/HTTP | JSON-RPC |
| **Library API** | .NET assemblies | In-memory objects |

