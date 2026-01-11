# Chunk Cache and Load Sequence Diagram

Render this Mermaid diagram in your preferred viewer (e.g., mermaid.live) and export to PNG via the UI or mermaid-cli (`mmdc -i chunk-cache-sequence.md -o chunk-cache-sequence.png`).

```mermaid
sequenceDiagram
    participant GameScene
    participant ChunkedTilemap
    participant Chunk
    participant Disk

    GameScene->>ChunkedTilemap: UpdateActiveChunks(viewport bounds)
    ChunkedTilemap->>ChunkedTilemap: Compute min/max chunk range (+ buffer)
    loop ensure cached
        ChunkedTilemap->>Chunk: GetOrCreateChunk
        alt chunk present
            Chunk-->>ChunkedTilemap: Return active chunk
        else on disk
            Disk->>Chunk: LoadChunk (validate header, read tiles)
            Chunk-->>ChunkedTilemap: Chunk ready
        else generate
            ChunkedTilemap->>Chunk: GenerateChunk (WFC/random)
        end
    end
    ChunkedTilemap->>ChunkedTilemap: Identify chunks outside buffered range
    loop unload distant
        ChunkedTilemap->>Disk: SaveChunk (gzip, CHNK header)
        ChunkedTilemap->>ChunkedTilemap: Remove from _activeChunks
    end
    ChunkedTilemap-->>GameScene: Active cache ready for Draw
```
