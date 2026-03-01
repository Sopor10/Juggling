# Solution Strategy

## Key Architectural Decisions

### Layered Architecture

The system uses a clean separation between core logic, generation, and presentation:

- **Core Domain** (`Siteswap.Details`): Pure domain logic, no dependencies
- **Generation** (`Generator.Core`): Backtracking algorithm with filters
- **Presentation** (`Components`, `WebAssembly`): Blazor UI components
- **Integration** (`Mcp.Server`): External API layer

**Rationale**: Enables reuse, testability, and independent evolution of layers.

### Immutable Domain Model

Siteswap patterns are immutable records. Operations return new instances.

**Rationale**: Thread-safety, simpler reasoning, functional style.

### Backtracking Generation

Generator uses recursive backtracking with early pruning via filters.

**Rationale**: Efficient exploration of large pattern spaces with constraints.

### Filter Composition

Filters implement a common interface and can be combined with AND/OR logic.

**Rationale**: Flexible, extensible filtering without modifying core generator.

## Technology Choices

| Decision | Technology | Rationale |
|----------|------------|-----------|
| **Language** | C# 13 | Strong typing, modern features, performance |
| **Web UI** | Blazor WebAssembly | Share code with backend, .NET ecosystem |
| **Visualization** | Mermaid/SVG | Standard formats, lightweight |
| **AI Integration** | MCP Protocol | Standardized LLM tool interface |
| **Testing** | xUnit + Playwright | Unit and E2E coverage |

## Quality Approach

| Goal | Approach |
|------|----------|
| **Correctness** | Mathematical validation in domain model, comprehensive tests |
| **Performance** | Async streaming, early filter pruning, incremental results |
| **Usability** | Layered complexity: simple defaults, advanced filters available |
| **Maintainability** | Clear separation of concerns, immutable data, functional patterns |

