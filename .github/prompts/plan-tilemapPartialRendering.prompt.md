I'm following the MonoGame tutorial "Chapter 13: Working with Tilemaps" (https://docs.monogame.net/articles/tutorials/building_2d_games/13_working_with_tilemaps/index.html#tilemap-draw-method) and have encountered an issue where **only part of the tilemap is being rendered**, even though all other properties appear to be correct:

- Character and sprite collision with map bounds works as expected
- The `_roomBounds` rectangle is calculated correctly
- Sprites interact with the boundaries properly
- The tilemap loads without errors

**My Tilemap.Draw() Implementation:**
```csharp
public void Draw(SpriteBatch spriteBatch)
{
  for (var i = 0; i < Count; i++)
  {
    var tilesetIndex = _tiles[i];
    var tile = _tileset.GetTile(tilesetIndex);

    var x = i % Columns;
    var y = i / Columns;

    var position = new Vector2(x * TileWidth, y * TileHeight);
    tile.Draw(spriteBatch, position, Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
  }
}
```

**Key Tilemap Properties:**
- `Rows`: Number of rows in tilemap
- `Columns`: Number of columns in tilemap
- `Count`: Total tiles (Rows × Columns)
- `Scale`: Vector2(4.0f, 4.0f)
- `TileWidth`: _tileset.TileWidth × Scale.X
- `TileHeight`: _tileset.TileHeight × Scale.Y

**How the tilemap is used in DungeonSlimeGame.cs:**
```csharp
// In LoadContent:
_tilemap = Tilemap.FromFile(Content, "images/tileset-definition.xml");
_tilemap.Scale = new Vector2(4.0f, 4.0f);

// In Draw:
SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
_tilemap.Draw(SpriteBatch);
_slimeTextureSprite.Draw(SpriteBatch, _slimePosition);
_batTextureSprite.Draw(SpriteBatch, _batPosition);
SpriteBatch.End();
```

**Constructor Bug Note:**
The Tilemap constructor has `Rows` and `Columns` parameters swapped compared to the tutorial:
```csharp
public Tilemap(Tileset tileset, int rows, int columns)
{
    // ...
    Rows = rows;
    Columns = columns;
    Count = Rows * Columns;  // This creates Count = rows × columns
    // ...
}
```

There is **no custom camera or viewport logic** beyond what's in the tutorial.

**Questions:**
1. What could cause only a partial tilemap to render while collision bounds work correctly?
2. Are there any issues with the Draw method's grid-to-screen position calculation?
3. Could the constructor parameter order issue (`rows, columns` vs. `columns, rows`) be causing the partial rendering?
4. What debugging steps would you recommend to identify the root cause?

Please provide specific code fixes or suggest diagnostic approaches to resolve this issue.
