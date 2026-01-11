# Architecture Class Diagram

The diagram below names the key concepts (Map, Chunk, Tile, rules) and their relationships. Export to PNG by opening in a Mermaid-aware editor and selecting export, or run `mmdc -i architecture-class-diagram.md -o architecture-class-diagram.png` if you have mermaid-cli.

```mermaid
classDiagram
    class ChunkedTilemap {
        +Tileset Tileset
        +int MapSizeInTiles
        +Dictionary<Point,Chunk> _activeChunks
        +Chunk GetOrCreateChunk(Point)
        +void UpdateActiveChunks(Rectangle)
        +void Draw(SpriteBatch, Rectangle)
        +void SaveAll()
    }
    class Chunk {
        +Point ChunkPosition
        +int[64,64] TileData
        +bool IsDirty
        +int this[int x, int y]
        +Point WorldTilePosition
    }
    class Camera2D {
        +Vector2 Position
        +float Zoom
        +Rectangle ViewportWorldBounds
        +Vector2 ScreenToWorld(Vector2)
    }
    class TileTypeRegistry {
        +Dictionary<int,TileType> _tileTypes
        +TileType GetTileType(int)
        +int TileCount
    }
    class TileType {
        +int TileId
        +string Name
        +bool EvaluateRules(TileRuleContext)
    }
    class WaveFunctionCollapse {
        +bool Generate()
        +int[,] GetOutput()
    }
    class TerrainRuleConfiguration {
        +int MountainRangeMin
        +int BeachOceanSizeMax
        +int BeachPlainsSizeMin
    }
    ChunkedTilemap o-- Chunk : manages
    ChunkedTilemap --> TileTypeRegistry : WFC relies on
    ChunkedTilemap --> WaveFunctionCollapse : generates tiles
    TileTypeRegistry --> TileType : stores rules
    WaveFunctionCollapse --> TileTypeRegistry : consults
    Chunk --> TileType : contains tile IDs
    Camera2D --> ChunkedTilemap : viewport bounds feed
    TerrainRuleConfiguration --> TileType : configures

```
