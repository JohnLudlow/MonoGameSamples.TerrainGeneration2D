# UI Features

> **Onboarding:** New to the UI system? Start with the [Onboarding Overview](feature-overview.md) for a guided introduction to the UI architecture, Gum toolkit, and how to extend or customize UI features.

This directory documents the user interface (UI) features, including Gum-based panels, overlays, tooltips, and runtime settings.

## Navigation

- **Quick Start:**
  - [Onboarding Overview](feature-overview.md)
  - [UI Feature Index](README.md)
  - [Full Terrain Generation Tutorial](../../terrain2d-tutorial/README.md)

- **Key Components:**
  - [Game Scene UI](game-scene-ui.md): Main HUD, pause/game over panels
  - [Tooltip Manager](tooltip-manager.md): Tile/tooltips, context info
  - [Runtime Settings Panel](runtime-settings-panel.md): In-game config, WFC/heuristics
  - [Debug Overlay](debug-overlay.md): Chunk/viewport visualization

- **Mapping Integration:**
  - [Mapping Features](../mapping/README.md)

- **Architecture & Performance:**
  - [Architecture Class Diagram](../../architecture-class-diagram.md)
  - [Performance & Debugging](../../performance-and-debugging.md)

## Glossary

| Term             | Meaning                                     |
| ---------------- | ------------------------------------------- |
| Gum              | UI toolkit for MonoGame, declarative layout |
| Panel            | UI container for grouping controls          |
| Tooltip          | Contextual info popup, e.g. tile info       |
| HUD              | Heads-up display, main game overlay         |
| Runtime Settings | In-game panel for adjusting config          |

## Advanced Topics

- [Customizing Gum UI](feature-overview.md#customizing-gum)
- [UI/Mapping Data Flow](../mapping/feature-overview.md#ui-integration)
- [Performance Diagnostics](../../performance-and-debugging.md)

## Changelog & Validation

- 2024-06-01: Initial onboarding doc added
- 2024-06-02: Linked UI feature docs
- 2026-01-15: Improved onboarding pointer, navigation, glossary, advanced topics, and validation

## Contributing & Validation

- When updating UI features:
  - Update this index and add links to new docs.
  - Add new terms to the glossary below.
  - Validate links with `scripts/check-doc-links.ps1`.
  - Ensure onboarding and advanced topics are discoverable.
