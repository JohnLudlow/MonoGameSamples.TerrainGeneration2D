# WFC Implementation Roadmap

This page outlines the staged approach to implementing Wave Function Collapse (WFC) in this repository and points to the canonical sub-wiki under the Map Generation area.

## Stages

- Domains: candidate sets per cell and initialization heuristics
- Propagation: constraint propagation (arc-consistency)
- Backtracking: decision stack and contradiction recovery
- Heuristics: selection strategies (domain entropy, Shannon entropy, most-constraining)
- Integration: chunked tilemap generation and diagnostics
- **Planned:** Generic domain abstraction — refactor WFC core to support arbitrary cell/value types, decouple terrain logic, and provide adapters for non-tile domains

## Canonical Docs

- Map Generation — WFC sub-wiki: [features/mapping/map-generation/wfc/README.md](../features/mapping/map-generation/wfc/README.md)

See the sub-wiki for detailed sections (01–07) covering overview, domains, propagation, backtracking, heuristics, integration, and performance.

**Generic Domains Roadmap:**
- Refactor WFC core to use generic types for cells and values
- Create generic configuration and rule table interfaces
- Move terrain-specific logic to adapters
- Update propagator and constraint logic to support generic domains
- Provide sample adapters for non-tile domains (e.g., resource placement)
- Validate backward compatibility with terrain generation

Related: [WFC Heuristics](../features/mapping/map-generation/wfc/05-heuristics.md)
