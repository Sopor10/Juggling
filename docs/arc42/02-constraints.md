# Architecture Constraints

## Technical Constraints

| Constraint | Description | Rationale |
|------------|-------------|-----------|
| **.NET 9** | Platform and runtime | Modern C# features, performance, cross-platform support |
| **Blazor WebAssembly** | Web UI technology | Run .NET in browser, share code between server and client |
| **MCP Protocol** | AI integration standard | Standardized protocol for LLM tool integration |

## Conventions

| Area | Convention |
|------|------------|
| **Code Style** | C# standard conventions, nullable reference types enabled |
| **Testing** | Unit tests with xUnit, E2E tests with Playwright |
| **Documentation** | arc42 for architecture, inline for code details |

