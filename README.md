# MonoGame TerrainGeneration2D

[![Build and Test](https://github.com/JohnLudlow/MonoGameSamples.TerrainGeneration2D/actions/workflows/main.yml/badge.svg)](https://github.com/JohnLudlow/MonoGameSamples.TerrainGeneration2D/actions/workflows/main.yml)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
![MonoGame](https://img.shields.io/badge/MonoGame-DesktopGL-orange)
[![License](https://img.shields.io/github/license/JohnLudlow/MonoGameSamples.TerrainGeneration2D)](https://github.com/JohnLudlow/MonoGameSamples.TerrainGeneration2D)

This repository contains a 2D terrain generation sample built with MonoGame, featuring chunked tilemaps, heightmaps, and Wave Function Collapse.

## Tech Stack & Versions

- .NET: 10.0
- MonoGame (DesktopGL): 3.8.5 (runtime: app/benchmarks), 3.8.4.1 (tests)
- Gum.MonoGame: 2025.12.9.1

## Features

- Chunked tilemap (64×64 chunks) with save/load (gzipped) per chunk
- Deterministic generation with WFC + heightmap fallback
- Camera pan/zoom, tooltip, and debug overlay (F12)
- EventSource-based diagnostics and optional benchmarks

## Getting Started

Prerequisites

- .NET 10 SDK

Build

```pwsh
dotnet build TerrainGeneration2D.slnx
```

Run

```pwsh
dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj
```

Test

```pwsh
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```

Docs

- See the index at docs/README.md

## Roadmap

- Heuristics: optional Shannon entropy and influence tie-breaks
- Backtracking: deeper stats and candidate ordering refinements
- Seam constraints between chunks for stricter continuity
- Live UI to tweak weights and regenerate visible chunks

## Troubleshooting

- Content not updating after asset changes: re-run a full solution build to rebuild the Content pipeline.
- Blank window or GL errors: update GPU drivers; ensure DesktopGL dependencies are installed.
- Missing saves/regeneration: delete TerrainGeneration2D.Core/Content/saves to force new generation.
- Debug overlay not visible: toggle with F12 (see GameController bindings).

## CI & Quality

- GitHub Actions builds and runs tests on push/PR (Windows).
- Docs link lint runs in CI to prevent broken in-repo links.

## License

This project is licensed under the MIT License. See LICENSE for details.

Quick Links

- Start with the docs index: docs/README.md
- Build: `dotnet build TerrainGeneration2D.slnx`
- Run: `dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj`
- Test: `dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj`
