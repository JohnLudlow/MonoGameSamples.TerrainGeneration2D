# WFC Implementation Roadmap

This page outlines the staged approach to implementing Wave Function Collapse (WFC) in this repository and points to the canonical sub-wiki under the Map Generation area.

## Stages

- Domains: candidate sets per cell and initialization heuristics
- Propagation: constraint propagation (arc-consistency)
- Backtracking: decision stack and contradiction recovery
- Heuristics: selection strategies (domain entropy, Shannon entropy, most-constraining)
- Integration: chunked tilemap generation and diagnostics

## Canonical Docs

- Map Generation — WFC sub-wiki: [features/mapping/map-generation/wfc/README.md](../features/mapping/map-generation/wfc/README.md)

See the sub-wiki for detailed sections (01–07) covering overview, domains, propagation, backtracking, heuristics, integration, and performance.

Related: [WFC Heuristics](../features/mapping/map-generation/wfc/05-heuristics.md)
