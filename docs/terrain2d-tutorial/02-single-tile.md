# Phase 02 - Fill the map with a single tile

In this phase you will:

- Add a tileset texture
- Draw a grid of a single tile to cover the screen
- Emit a basic log and performance counter (optional)
- Write tests for logic-only pieces (TDD)

## 0. Write tests (TDD)

Create `TerrainGeneration2D.Tests/TilesetMathTests.cs` to validate the index→rectangle math without needing a graphics device:

```csharp
using Microsoft.Xna.Framework;

namespace TerrainGeneration2D.Tests;

public class TilesetMathTests
{
    private static Rectangle IndexToRect(int tileIndex, int tilesPerRow, int tileWidth, int tileHeight)
    {
        int x = (tileIndex % tilesPerRow) * tileWidth;
        int y = (tileIndex / tilesPerRow) * tileHeight;
        return new Rectangle(x, y, tileWidth, tileHeight);
    }

    [Fact]
    public void IndexToRect_ComputesCorrectCoordinates()
    {
        // Arrange
        int tilePixelSize = 16; int atlasPixelWidth = 256; int tilesPerRow = atlasPixelWidth / tilePixelSize;

        // Act + Assert
        var firstRect = IndexToRect(0, tilesPerRow, tilePixelSize, tilePixelSize);
        Assert.Equal(new Rectangle(0, 0, 16, 16), firstRect);

        var fifthRect = IndexToRect(5, tilesPerRow, tilePixelSize, tilePixelSize);
        Assert.Equal(new Rectangle(80, 0, 16, 16), fifthRect);

        var seventeenthRect = IndexToRect(17, tilesPerRow, tilePixelSize, tilePixelSize);
        Assert.Equal(new Rectangle(16, 16, 16, 16), seventeenthRect);
    }

    [Fact]
    public void TilesPerRow_IsDivisible()
    {
        // Catch mismatched tile size early
        int atlasWidth = 256;
        int tileWidth = 16;
        Assert.Equal(0, atlasWidth % tileWidth);
        int tilesPerRow = atlasWidth / tileWidth;
        Assert.Equal(16, tilesPerRow);
    }
}
```

Run:

```bash
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```

Notes:

- Keep tests logic-only; do not require a `GraphicsDevice`. This phase validates the index→rectangle math; the runtime uses `TextureRegion`-based `Tileset`.

## 1. Add content asset

- Place the provided `atlas.png` into `TerrainGeneration2D.Content/Assets/images/terrain-atlas.png`.
- Build the solution to have the content builder process assets (it will appear under the game's `Content/images`).

### Load the atlas texture (Game)

A minimal example to load the processed atlas in your game:

```csharp
private Texture2D _atlas = null!;

protected override void LoadContent()
{
    base.LoadContent();
    _atlas = Content.Load<Texture2D>("images/terrain-atlas");
}
```

The full render example later in this phase shows using `_atlas` with `Tileset` to draw.

## 2. Reference Core from Game

Add a project reference from the Game to Core (if not already added):

```bash
dotnet add src/TerrainGeneration2D/TerrainGeneration2D.csproj reference src/TerrainGeneration2D.Core/TerrainGeneration2D.Core.csproj
```

### Implement TextureRegion

```csharp
// Terrain2D.Core\Graphics\TextureRegion.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

public class TextureRegion
{
  /// <summary>
  /// Gets or Sets the source texture this texture region is part of.
  /// </summary>
  public Texture2D Texture { get; set; }

  /// <summary>
  /// Gets or Sets the source rectangle boundary of this texture region within the source texture.
  /// </summary>
  public Rectangle SourceRectangle { get; set; }

  /// <summary>
  /// Gets the width, in pixels, of this texture region.
  /// </summary>
  public int Width => SourceRectangle.Width;

  /// <summary>
  /// Gets the height, in pixels, of this texture region.
  /// </summary>
  public int Height => SourceRectangle.Height;

  /// <summary>
  /// Gets the top normalized texture coordinate of this region.
  /// </summary>
  public float TopTextureCoordinate => SourceRectangle.Top / (float)Texture.Height;

  /// <summary>
  /// Gets the bottom normalized texture coordinate of this region.
  /// </summary>
  public float BottomTextureCoordinate => SourceRectangle.Bottom / (float)Texture.Height;

  /// <summary>
  ///  Gets the left normalized texture coordinate of this region.
  /// </summary>
  public float LeftTextureCoordinate => SourceRectangle.Left / (float)Texture.Width;

  /// <summary>
  /// Gets the right normalized texture coordinate of this region.
  /// </summary>
  public float RightTextureCoordinate => SourceRectangle.Right / (float)Texture.Width;


  public TextureRegion(Texture2D texture, Rectangle sourceRectangle)
  {
    Texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
    SourceRectangle = sourceRectangle;
  }

  public TextureRegion(Texture2D texture, int x, int y, int width, int height)
  {
    Texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
    SourceRectangle = new Rectangle(x, y, width, height);
  }

  public TextureRegion(Texture2D texture)
  {
    Texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
    SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
  }
  /// <summary>
  /// Submit this texture region for drawing in the current batch.
  /// </summary>
  /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
  /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
  /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
  public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
  {
    Draw(spriteBatch, position, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
  }

  /// <summary>
  /// Submit this texture region for drawing in the current batch.
  /// </summary>
  /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
  /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
  /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
  /// <param name="rotation">The amount of rotation, in radians, to apply when drawing this texture region on screen.</param>
  /// <param name="origin">The center of rotation, scaling, and position when drawing this texture region on screen.</param>
  /// <param name="scale">The scale factor to apply when drawing this texture region on screen.</param>
  /// <param name="effects">Specifies if this texture region should be flipped horizontally, vertically, or both when drawing on screen.</param>
  /// <param name="layerDepth">The depth of the layer to use when drawing this texture region on screen.</param>
  public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
  {
    Draw(
      spriteBatch,
      position,
      color,
      rotation,
      origin,
      new Vector2(scale, scale),
      effects,
      layerDepth
    );
  }

  /// <summary>
  /// Submit this texture region for drawing in the current batch.
  /// </summary>
  /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
  /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
  /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
  /// <param name="rotation">The amount of rotation, in radians, to apply when drawing this texture region on screen.</param>
  /// <param name="origin">The center of rotation, scaling, and position when drawing this texture region on screen.</param>
  /// <param name="scale">The amount of scaling to apply to the x- and y-axes when drawing this texture region on screen.</param>
  /// <param name="effects">Specifies if this texture region should be flipped horizontally, vertically, or both when drawing on screen.</param>
  /// <param name="layerDepth">The depth of the layer to use when drawing this texture region on screen.</param>
  public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
  {
    System.ArgumentNullException.ThrowIfNull(spriteBatch);

    spriteBatch.Draw(
      Texture,
      position,
      SourceRectangle,
      color,
      rotation,
      origin,
      scale,
      effects,
      layerDepth
    );
  }
}
```

## Implement Tileset

```csharp
namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

public class Tileset
{
  private readonly TextureRegion[] _tiles;


  /// <summary>
  /// Gets the width, in pixels, of each tile in this tileset.
  /// </summary>
  public int TileWidth { get; }

  /// <summary>
  /// Gets the height, in pixels, of each tile in this tileset.
  /// </summary>
  public int TileHeight { get; }

  /// <summary>
  /// Gets the total number of columns in this tileset.
  /// </summary>
  public int Columns { get; }

  /// <summary>
  /// Gets the total number of rows in this tileset.
  /// </summary>
  public int Rows { get; }

  /// <summary>
  /// Gets the total number of tiles in this tileset.
  /// </summary>
  public int Count { get; }

  public Tileset(TextureRegion textureRegion, int tileWidth, int tileHeight)
  {
    System.ArgumentNullException.ThrowIfNull(textureRegion);

    TileWidth = tileWidth;
    TileHeight = tileHeight;
    Columns = textureRegion.Width / tileWidth;
    Rows = textureRegion.Height / tileHeight;
    Count = Columns * Rows;

    _tiles = new TextureRegion[Count];

    for (var i = 0; i < Count; i++)
    {
      var x = (i % Columns) * tileWidth;
      var y = (i / Columns) * tileHeight;

      _tiles[i] = new TextureRegion(
        textureRegion.Texture,
        textureRegion.SourceRectangle.X + x,
        textureRegion.SourceRectangle.Y + y,
        tileWidth,
        tileHeight
      );
    }
  }

  /// <summary>
  /// Gets the texture region for the tile from this tileset at the given index.
  /// </summary>
  /// <param name="index">The index of the texture region in this tile set.</param>
  /// <returns>The texture region for the tile form this tileset at the given index.</returns>
  public TextureRegion GetTile(int index) => _tiles[index];

  /// <summary>
  /// Gets the texture region for the tile from this tileset at the given location.
  /// </summary>
  /// <param name="column">The column in this tileset of the texture region.</param>
  /// <param name="row">The row in this tileset of the texture region.</param>
  /// <returns>The texture region for the tile from this tileset at given location.</returns>
  public TextureRegion GetTile(int column, int row)
  {
    var index = row * Columns + column;
    return GetTile(index);
  }
}
```

## Use TextureRegion and Tileset

```csharp
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

// Whole texture as a region
var fullRegion = new TextureRegion(_atlas);

// Or a sub-rectangle (x, y, width, height) inside the atlas
var subRegion = new TextureRegion(_atlas, 0, 0, 160, 160);
```

You'll pass a `TextureRegion` to `Tileset` to slice it into tile-sized regions.

## 3. Use Tileset in Core (TextureRegion-based)

This repo already provides a `Tileset` that is built from a `TextureRegion` and exposes `GetTile(int)` returning a `TextureRegion`. Instantiate it from your atlas texture:

```csharp
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

// after loading the atlas texture
_tileset = new Tileset(new TextureRegion(_atlas), 16, 16); // tile size per your atlas
```

## 4. Render a single tile grid in the Game

Extend the game class you created in Phase 01 to load the atlas and draw tile 0 across the viewport. This augments your existing `TerrainGenerationGame` (no top-level statements, no overwrites).

TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D;

internal sealed class TerrainGenerationGame : GameHostBase
{
    private Texture2D _atlas = null!;
    private Tileset _tileset = null!;

    public TerrainGenerationGame() : base("Terrain Generation 2D", 1280, 720, false) { }

    protected override void LoadContent()
    {
        base.LoadContent();
        _atlas = Content.Load<Texture2D>("images/terrain-atlas");
        _tileset = new Tileset(new TextureRegion(_atlas), 16, 16); // adjust to your atlas tile size
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        var region = _tileset.GetTile(0);

        // Fill the screen with the single tile
        for (int y = 0; y < GraphicsDevice.Viewport.Height; y += _tileset.TileHeight)
        {
            for (int x = 0; x < GraphicsDevice.Viewport.Width; x += _tileset.TileWidth)
            {
                SpriteBatch.Draw(region.Texture, new Rectangle(x, y, _tileset.TileWidth, _tileset.TileHeight), region.SourceRectangle, Color.White);
            }
        }

        SpriteBatch.End();
        base.Draw(gameTime);
    }
}
```

Run:

```bash
dotnet run --project src/TerrainGeneration2D/TerrainGeneration2D.csproj
```

You should see the screen filled with the first tile from your atlas.

See also:

- Previous phase: [01 — Setup](01-setup.md)
- Next steps with deterministic random fill in [05-random-tiles.md](05-random-tiles.md).
- Tutorial index: [README.md](README.md).
