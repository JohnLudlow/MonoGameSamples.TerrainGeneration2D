# Phase 05 - Fill the map with random tiles (deterministic)

In this phase you will:
- Create a tilemap array
- Fill it with random tiles from the atlas (using a fixed seed)
- Draw only visible tiles
- Emit logs and counters to observe generation
- Write a unit test first (TDD)
- Wire input controls for camera movement, zoom, and pan
- Add a debugging UI toggle (F12) and tooltip showing tile details

## 0. Write the test (TDD)
Create `TerrainGeneration2D.Tests/RandomTilesTests.cs`:
```csharp
namespace TerrainGeneration2D.Tests;

public class RandomTilesTests
{
    [Fact]
    public void RandomFill_IsDeterministic()
    {
        var mapA = new Tilemap(64, 64);
        var mapB = new Tilemap(64, 64);
        var rngA = new Random(12345);
        var rngB = new Random(12345);

        int tileCount = 64; // pretend atlas size
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            mapA[x,y] = rngA.Next(tileCount);
            mapB[x,y] = rngB.Next(tileCount);
        }

        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
            Assert.Equal(mapA[x,y], mapB[x,y]);
    }

    [Fact]
    public void RandomFill_UsesValidTileRange()
    {
        var map = new Tilemap(16, 16);
        var rng = new Random(1);
        int tileCount = 10;
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            map[x,y] = rng.Next(tileCount);

        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Assert.InRange(map[x,y], 0, tileCount - 1);
    }
}
```
Run:
```bash
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```
Tests should pass as they only exercise `Tilemap` and `Random`.

## 1. Add a Tilemap class (Core)
Conceptual example (this repo uses `TerrainGeneration2D.Core/Graphics/ChunkedTilemap` instead). Prefer properties over public fields:
Create `TerrainGeneration2D.Core/Tilemap.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;

public class Tilemap
{
    public int Width { get; }
    public int Height { get; }
    private readonly int[,] _tiles;

    public Tilemap(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new int[width, height];
    }

    public int this[int x, int y]
    {
        get => _tiles[x, y];
        set => _tiles[x, y] = value;
    }
}
```

## 2. Fill with deterministic random tiles (Game)
Edit your game code to add map generation and drawing (this repo already renders via chunked tilemap; use this as conceptual guidance).

TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
// inside GameHost
private Tilemap _map = null!;
private readonly int _mapWidth = 256;
private readonly int _mapHeight = 256;
private const int TileSize = 16;

// tileset assets used when drawing
private Texture2D _atlas = null!;
private Tileset _tileset = null!;

protected override void Initialize()
{
    base.Initialize();
    _map = new Tilemap(_mapWidth, _mapHeight);

    var rng = new Random(12345);
    int tileCount = _tileset.Count;

    GenLog.GenerateBegin(_log, _mapWidth, _mapHeight);
    for (int y = 0; y < _mapHeight; y++)
    for (int x = 0; x < _mapWidth; x++)
        _map[x, y] = rng.Next(tileCount);
    // Optional instrumentation (see Diagnostics README):
    // TerrainPerformanceEventSource.Log.ReportActiveChunkCount(_activeChunkCount);
    GenLog.GenerateEnd(_log, success: true);
}

protected override void LoadContent()
{
    base.LoadContent();
    // Load the atlas processed by the Content Pipeline
    _atlas = Content.Load<Texture2D>("images/terrain-atlas");
    // Build a tileset from a TextureRegion of the atlas; tile size must match the atlas
    _tileset = new Tileset(new TextureRegion(_atlas), TileSize, TileSize);
}

protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(new Color(176, 196, 222));
    _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

    int viewTilesX = GraphicsDevice.Viewport.Width / TileSize + 1;
    int viewTilesY = GraphicsDevice.Viewport.Height / TileSize + 1;

    for (int y = 0; y < viewTilesY; y++)
    for (int x = 0; x < viewTilesX; x++)
    {
        int tileIndex = _map[x, y];
        var region = _tileset.GetTile(tileIndex);
        _spriteBatch.Draw(region.Texture, new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize), region.SourceRectangle, Color.White);
    }

    _spriteBatch.End();
    base.Draw(gameTime);
}
```

Note: For a `TextureRegion` overview and `Tileset` setup, see Phase 02 — Implement TextureRegion in [02-single-tile.md](02-single-tile.md).

## See also
- Previous phase: [04 — Performance](04-performance.md)
- Next phase: [06 — Adjacency rules](06-adjacency-rules.md)
- Tutorial index: [README.md](README.md)

Run:
```bash
dotnet run --project TerrainGeneration2D/TerrainGeneration2D.csproj
```
You’ll see a mosaic of randomly chosen tiles and logs/counters indicating generation work.

Tip: Keep the seed constant (`new Random(12345)`) to make the result reproducible.

### Prerequisite: Minimal GameController (input helper)
Add a simple input helper so the code below compiles and behaves consistently.

Create `TerrainGeneration2D/GameController.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D;

public static class GameController
{
    private static KeyboardState _prevKeyboard, _keyboard;
    private static MouseState _prevMouse, _mouse;
    private static GamePadState _prevPad, _pad;

    public static void Update()
    {
        _prevKeyboard = _keyboard;
        _prevMouse = _mouse;
        _prevPad = _pad;
        _keyboard = Keyboard.GetState();
        _mouse = Mouse.GetState();
        _pad = GamePad.GetState(PlayerIndex.One);
    }

    public static Vector2 GetCameraMovement()
    {
        Vector2 move = Vector2.Zero;
        if (_keyboard.IsKeyDown(Keys.W) || _keyboard.IsKeyDown(Keys.Up)) move.Y -= 1f;
        if (_keyboard.IsKeyDown(Keys.S) || _keyboard.IsKeyDown(Keys.Down)) move.Y += 1f;
        if (_keyboard.IsKeyDown(Keys.A) || _keyboard.IsKeyDown(Keys.Left)) move.X -= 1f;
        if (_keyboard.IsKeyDown(Keys.D) || _keyboard.IsKeyDown(Keys.Right)) move.X += 1f;

        // Add gamepad analog stick
        var analog = new Vector2(_pad.ThumbSticks.Left.X, -_pad.ThumbSticks.Left.Y);
        move += analog;
        if (move.LengthSquared() > 1f) move.Normalize();
        return move;
    }

    public static int GetZoomDelta()
    {
        int delta = 0;
        int wheel = _mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        if (wheel > 0) delta += 1;
        if (wheel < 0) delta -= 1;
        if (_pad.IsButtonDown(Buttons.RightShoulder) && !_prevPad.IsButtonDown(Buttons.RightShoulder)) delta += 1;
        if (_pad.IsButtonDown(Buttons.LeftShoulder) && !_prevPad.IsButtonDown(Buttons.LeftShoulder)) delta -= 1;
        return Math.Clamp(delta, -1, 1);
    }

    public static bool IsCameraPanActive() => _mouse.RightButton == ButtonState.Pressed;

    public static Vector2 GetMousePosition() => new(_mouse.X, _mouse.Y);

    public static bool ToggleFullscreen() => _keyboard.IsKeyDown(Keys.F11) && _prevKeyboard.IsKeyUp(Keys.F11);

    public static bool ToggleDebugOverlay() => _keyboard.IsKeyDown(Keys.F12) && _prevKeyboard.IsKeyUp(Keys.F12);
}
```

### Prerequisite: Minimal Camera2D (2D transform)
Add a tiny camera utility used for movement, zoom, and sprite batching transforms.

Create `TerrainGeneration2D.Core/Graphics/Camera2D.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;

public class Camera2D
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1f;
    public float Rotation { get; set; } = 0f;

    private Viewport _viewport;

    public Camera2D(Viewport viewport)
    {
        _viewport = viewport;
    }

    public Matrix GetTransformMatrix()
    {
        var origin = new Vector2(_viewport.Width * 0.5f, _viewport.Height * 0.5f);
        return
            Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1f) *
            Matrix.CreateTranslation(new Vector3(origin.X, origin.Y, 0f));
    }

    public void UpdateViewport(Viewport viewport)
    {
        _viewport = viewport;
    }
}
```

## 3. Wire input controls (camera movement, zoom, pan)
Update your scene or game host to use `GameController` helpers. Example:

```csharp
// Fields
private Camera2D _camera = null!; // assume you have a Camera2D utility
private float _moveSpeed = 400f;   // pixels per second
private float _zoomStep = 0.1f;

protected override void Initialize()
{
    base.Initialize();
    _camera = new Camera2D(GraphicsDevice.Viewport);
}

protected override void Update(GameTime gameTime)
{
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

    // Poll input once per frame
    GameController.Update();

    // Movement
    Vector2 move = GameController.GetCameraMovement();
    if (move != Vector2.Zero)
    {
        _camera.Position += move * _moveSpeed * dt;
    }

    // Zoom (clamped)
    int zoomDelta = GameController.GetZoomDelta();
    if (zoomDelta != 0)
    {
        _camera.Zoom = Math.Clamp(_camera.Zoom + (zoomDelta * _zoomStep), 0.25f, 4f);
    }

    // Pan (right mouse drag)
    if (GameController.IsCameraPanActive())
    {
        var mouse = GameController.GetMousePosition();
        // convert to world delta relative to last frame
        // simple approach: move opposite to mouse delta
        // track previous mouse position
        if (_lastMouse.HasValue)
        {
            var delta = mouse - _lastMouse.Value;
            _camera.Position -= delta / _camera.Zoom;
        }
        _lastMouse = mouse;
    }
    else
    {
        _lastMouse = null;
    }

    // Toggles
    if (GameController.ToggleFullscreen())
    {
        var gdm = (GraphicsDeviceManager)Services.GetService(typeof(GraphicsDeviceManager))!;
        gdm.IsFullScreen = !gdm.IsFullScreen;
        gdm.ApplyChanges();
    }

    // Debug overlay toggle could set a flag checked during Draw
    if (GameController.ToggleDebugOverlay())
    {
        _showDebug = !_showDebug;
    }

    base.Update(gameTime);
}

// Keep last mouse position
private Vector2? _lastMouse;
private bool _showDebug;
```

Apply `_camera.GetTransformMatrix()` when drawing sprites:
```csharp
_spriteBatch.Begin(transformMatrix: _camera.GetTransformMatrix(), samplerState: SamplerState.PointClamp);
// draw tiles here
_spriteBatch.End();
```

This ensures consistent input wiring via `GameController` without touching low-level input directly.

## 4. Debug UI toggle (F12) with chunk boundaries
Add a simple overlay that draws chunk boundaries when F12 is toggled:

```csharp
// constants
const int ChunkSize = 64; // tiles per chunk

protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(new Color(176, 196, 222));

    _spriteBatch.Begin(transformMatrix: _camera.GetTransformMatrix(), samplerState: SamplerState.PointClamp);
    // draw tiles ...
    _spriteBatch.End();

    if (_showDebug)
    {
        // draw chunk boundaries in screen space; use a simple primitive drawing
        _spriteBatch.Begin(transformMatrix: _camera.GetTransformMatrix());
        var viewportTilesX = GraphicsDevice.Viewport.Width / TileSize + 2;
        var viewportTilesY = GraphicsDevice.Viewport.Height / TileSize + 2;

        // vertical chunk lines
        for (int x = 0; x <= viewportTilesX; x++)
        {
            if (x % ChunkSize == 0)
            {
                var rect = new Rectangle(x * TileSize, 0, 2, viewportTilesY * TileSize);
                _spriteBatch.Draw(_whitePixel, rect, Color.Red * 0.6f);
            }
        }
        // horizontal chunk lines
        for (int y = 0; y <= viewportTilesY; y++)
        {
            if (y % ChunkSize == 0)
            {
                var rect = new Rectangle(0, y * TileSize, viewportTilesX * TileSize, 2);
                _spriteBatch.Draw(_whitePixel, rect, Color.Red * 0.6f);
            }
        }
        _spriteBatch.End();
    }
}

// initialize a 1x1 white pixel texture once
private Texture2D _whitePixel = null!;
protected override void LoadContent()
{
    // ...existing loads...
    _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
    _whitePixel.SetData(new[] { Color.White });
}
```

This overlays red lines at chunk boundaries. Press F12 to toggle `_showDebug` as shown earlier in the input section.

## 5. Tooltip showing tile details
Display a tooltip with the hovered tile and its chunk coordinates:

```csharp
// fields
private SpriteFont _uiFont = null!;
private string _tooltipText = string.Empty;

protected override void LoadContent()
{
    // ...existing loads...
    _uiFont = Content.Load<SpriteFont>("fonts/04B_30");
}

protected override void Update(GameTime gameTime)
{
    // ...existing update...
    // compute hovered tile in world space
    var mouseScreen = GameController.GetMousePosition();
    var mouseWorld = Vector2.Transform(mouseScreen, Matrix.Invert(_camera.GetTransformMatrix()));
    int tileX = (int)Math.Floor(mouseWorld.X / TileSize);
    int tileY = (int)Math.Floor(mouseWorld.Y / TileSize);
    int chunkX = tileX >= 0 ? tileX / ChunkSize : (tileX - (ChunkSize - 1)) / ChunkSize;
    int chunkY = tileY >= 0 ? tileY / ChunkSize : (tileY - (ChunkSize - 1)) / ChunkSize;

    if (tileX >= 0 && tileY >= 0 && tileX < _mapWidth && tileY < _mapHeight)
    {
        int tileId = _map[tileX, tileY];
        _tooltipText = $"Tile:[{tileX},{tileY}] Type:{tileId} Chunk:[{chunkX},{chunkY}]";
    }
    else
    {
        _tooltipText = string.Empty;
    }
}

protected override void Draw(GameTime gameTime)
{
    // ...existing drawing...

    // draw tooltip in screen space (no camera transform)
    if (!string.IsNullOrEmpty(_tooltipText))
    {
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_uiFont, _tooltipText, new Vector2(12, 12), Color.Yellow);
        _spriteBatch.End();
    }
}
```

This keeps the tooltip fixed to the screen corner while reporting the hovered tile indices, tile ID, and chunk coordinates.
