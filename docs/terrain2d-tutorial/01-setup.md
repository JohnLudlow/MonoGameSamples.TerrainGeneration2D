# Phase 01 - Setup a blank MonoGame project (powder blue screen)

In this phase you will:

- Create a new solution with two projects:
  - Game (MonoGame DesktopGL)
  - Core library for map, generation, and diagnostics
- Show a powder-blue screen to confirm rendering works
- Write simple tests first (TDD) for logic-only pieces

Note: This tutorial is designed to be implemented in a separate repository. The code snippets are standalone and do not rely on this repo’s implementation. Keep your tutorial solution independent from this codebase.

Target: .NET 10

## 1. Create folders

```bash
mkdir Terrain2DTutorial
cd Terrain2DTutorial
mkdir src docs
```

## 2. Create solution and projects

```bash
cd src
# Solution
dotnet new sln -n Terrain2D

# Game project (DesktopGL host via SDK project)
dotnet new console -n TerrainGeneration2D

# Core library
dotnet new classlib -n TerrainGeneration2D.Core

# Add to solution
dotnet sln add TerrainGeneration2D/TerrainGeneration2D.csproj
dotnet sln add TerrainGeneration2D.Core/TerrainGeneration2D.Core.csproj
```

## 3. Write minimal tests (TDD)

Create `TerrainGeneration2D.Tests/SetupTests.cs`:

```csharp
namespace TerrainGeneration2D.Tests;

public class SetupTests
{
    [Fact]
    public void Solution_CompilesWithNet10()
    {
        // Placeholder test to bootstrap runner. Real render tests are hard without a headless harness.
        Assert.True(true);
    }
}
```

Run:

```bash
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```

## 4. Add MonoGame dependencies to the Game project

Edit `TerrainGeneration2D/TerrainGeneration2D.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="MonoGame.Framework.DesktopGL" Version="3.8.5-preview.2" />
    </ItemGroup>
</Project>
```

Note: The latest 3.8.5 prerelease is currently `3.8.5-preview.2`. Check NuGet for updates.

## 5. Add a Content Pipeline project

Create a new project to build and package content using the MonoGame Content Pipeline.

```bash
mkdir TerrainGeneration2D.Content
```

Create `TerrainGeneration2D.Content/TerrainGeneration2D.Content.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <RootNameSpace>JohnLudlow.MonoGameSamples.TerrainGeneration2D.Content</RootNameSpace>
    </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="MonoGame.Framework.Content.Pipeline" Version="3.8.5-preview.2" />
        <PackageReference Include="MonoGame.Framework.Native" Version="3.8.5-preview.2">
            <PrivateAssets>All</PrivateAssets>
        </PackageReference>
        <PackageReference Include="MonoGame.Library.FreeType" Version="2.13.2.2" />
        <PackageReference Include="MonoGame.Library.MojoShader" Version="1.0.0.3" />
        <PackageReference Include="MonoGame.Tool.Basisu" Version="1.60.0.3" />
        <PackageReference Include="MonoGame.Tool.Crunch" Version="1.0.4.5" />
        <PackageReference Include="MonoGame.Tool.Dxc" Version="1.8.2505.10" />
        <PackageReference Include="MonoGame.Tool.FFmpeg" Version="7.0.0.9" />
        <PackageReference Include="MonoGame.Tool.FFprobe" Version="7.0.0.9" />
    </ItemGroup>
    <ItemGroup>
        <Content Include="Assets\**\*.*">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </Content>
    </ItemGroup>
</Project>
```

Implement the Content Builder. Create a `Builder` folder with two files:

1) `TerrainGeneration2D.Content/Builder/Builder.cs` (entry point)

```csharp
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Content.Builder;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

var contentCollectionArgs = new ContentBuilderParams()
{
    Mode = ContentBuilderMode.Builder,
    WorkingDirectory = AppContext.BaseDirectory,
    SourceDirectory = "Assets",
    Platform = TargetPlatform.DesktopGL
};
var builder = new TerrainGeneration2DContentBuilder();

if (args is not null && args.Length > 0)
{
    builder.Run(args);
}
else
{
    builder.Run(contentCollectionArgs);
}

return builder.FailedToBuild > 0 ? -1 : 0;
```

2) `TerrainGeneration2D.Content/Builder/TerrainGeneration2DContentBuilder.cs` (what to include)

```csharp
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Content.Builder;

internal sealed class TerrainGeneration2DContentBuilder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var content = new ContentCollection();

        // Images (compiled via the pipeline)
        content.Include<WildcardRule>("images/atlas.png");
        content.Include<WildcardRule>("images/background-pattern.png");
        content.Include<WildcardRule>("images/terrain-atlas.png");

        // Sidecar files copied as-is (no processing)
        content.IncludeCopy<WildcardRule>("images/atlas-definition.xml");
        content.IncludeCopy<WildcardRule>("images/tileset-definition.xml");
        content.IncludeCopy<WildcardRule>("images/terrain-tileset-definition.xml");

        // Audio
        content.Include<WildcardRule>("audio/ui.wav");
        content.Include<WildcardRule>("audio/bounce.wav");
        content.Include<WildcardRule>("audio/collect.wav");
        content.Include<WildcardRule>("audio/theme.ogg");

        // Fonts
        content.Include<WildcardRule>("fonts/04B_30.spritefont");
        content.Include<WildcardRule>("fonts/04B_30_5x.spritefont");
        content.IncludeCopy<WildcardRule>("fonts/04b_30.fnt");

        // Effects/Shaders
        content.Include<WildcardRule>("effects/grayscaleEffect.fx");

        return content;
    }
}
```

How it works:

- `Builder.cs` configures the platform, source directory (`Assets`), and runs the builder.
- `TerrainGeneration2DContentBuilder` declares which assets to process (`Include`) vs copy as-is (`IncludeCopy`).
- The Game project’s MSBuild target runs this Content project after each build and outputs into the game’s build folder.

Add an `Assets` folder for your raw content:

```bash
mkdir TerrainGeneration2D.Content/Assets
mkdir TerrainGeneration2D.Content/Assets/images
```

Copy the provided atlas image into the content project so the game can load it as `images/terrain-atlas`:

```bash
# Use the attached atlas file and name it terrain-atlas.png
# (Place it here so the pipeline picks it up on build)
copy atlas.png TerrainGeneration2D.Content/Assets/images/terrain-atlas.png
```

You can also run the content builder directly to test it:

```bash
dotnet run --project TerrainGeneration2D.Content/TerrainGeneration2D.Content.csproj -- build -p DesktopGL -s Assets -o TerrainGeneration2D/bin/Debug/net10.0/Content
```

Note: If you don’t have some of the listed assets yet, either add placeholder files under `Assets` with the same names or remove the corresponding `Include`/`IncludeCopy` lines to avoid build errors.

Add both projects to the solution:

```bash
dotnet sln add TerrainGeneration2D.Content/TerrainGeneration2D.Content.csproj
```

Integrate the content build into the Game project by adding this target to `TerrainGeneration2D/TerrainGeneration2D.csproj`:

```xml
    <Target Name="BuildContent" AfterTargets="Build">
        <PropertyGroup>
            <ContentOutput>$(ProjectDir)$(OutputPath)Content</ContentOutput>
            <ContentTemp>$(ProjectDir)/$(IntermediateOutputPath)</ContentTemp>
            <ContentArgs>build -p $(MonoGamePlatform) -s Assets -o $(ContentOutput)</ContentArgs>
        </PropertyGroup>
        <MSbuild Projects="..\TerrainGeneration2D.Content\TerrainGeneration2D.Content.csproj" Targets="Build;Run" Properties="RunArguments=$(ContentArgs)" />
    </Target>
```

## 6. Create the GameHostBase base class

Create `TerrainGeneration2D.Core/GameHostBase.cs` as a reusable base class your game can derive from:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;

public class GameHostBase : Game
{
    protected GraphicsDeviceManager GraphicsManager { get; }
    protected SpriteBatch SpriteBatch { get; private set; } = null!;

    public GameHostBase(string windowTitle, int windowWidthInPixels, int windowHeightInPixels, bool isFullScreenWindow = false)
    {
        GraphicsManager = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = windowWidthInPixels,
            PreferredBackBufferHeight = windowHeightInPixels,
            IsFullScreen = isFullScreenWindow
        };
        GraphicsManager.ApplyChanges();

        Window.Title = windowTitle;
        Window.AllowUserResizing = true; // optional, can disable later

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }
}
```

This provides a base `GameHostBase` that centralizes window, graphics, and content setup without hiding members from `Game`.

Before using `GameHostBase` from your Game project, add a project reference from the Game to the Core library:

```bash
# From the repo root or within src
dotnet add src/TerrainGeneration2D/TerrainGeneration2D.csproj reference src/TerrainGeneration2D.Core/TerrainGeneration2D.Core.csproj
```

## 7. Create the Game host

Define your game class by deriving from the `GameHostBase` you created, then run it from `Program.cs`.

TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D;

internal sealed class TerrainGenerationGame : GameHostBase
{
    public TerrainGenerationGame() : base("Terrain Generation 2D", 1280, 720, false) { }

    protected override void Initialize()
    {
        base.Initialize();
        // Any additional initialization
    }

    protected override void Draw(GameTime gameTime)
    {
        // Powder blue to verify render loop
        GraphicsDevice.Clear(new Color(176, 196, 222));
        base.Draw(gameTime);
    }
}
```

`TerrainGeneration2D/Program.cs`:

```csharp
namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D;

public static class Program
{
    public static void Main()
    {
        using var game = new TerrainGenerationGame();
        game.Run();
    }
}
```

## 8. Build & run

```bash
# From the repo root (one level above src), build the solution
dotnet build src/Terrain2D.sln

# Run the game project
dotnet run --project src/TerrainGeneration2D/TerrainGeneration2D.csproj
```

You should see a window with a powder-blue background.

### 8.1 Window & display controls

- Fullscreen toggle: Press F11 to toggle fullscreen at runtime. The repo maps F11 in the input layer and applies it in the running scene, so no additional code is required.
- Optional resize: If you prefer a resizable window during development, enable it by setting the window to allow user resizing in the base game host:

```csharp
// In GameHostBase constructor (see TerrainGeneration2D.Core/GameHostBase.cs)
Window.AllowUserResizing = true;
```

Note: Resizing applies when not in fullscreen.

## 9. Verify the Content Pipeline output

Add a quick placeholder asset and confirm it lands in the game output folder on build.

```bash
# Add a tiny placeholder file (use any small image you have instead of .txt if preferred)
echo test > TerrainGeneration2D.Content/Assets/images/verify.txt

dotnet build TerrainGeneration2D.slnx

# Inspect the game's Content folder for the copied asset
# (The content build target outputs into the game's Content folder under the build output.)
dir TerrainGeneration2D/bin/Debug/net10.0/Content/images
```

You should see your asset (e.g., verify.txt or your image) under the game's Content/images folder. If not, re-check the MSBuild target in `TerrainGeneration2D/TerrainGeneration2D.csproj` and the content project path.

## Troubleshooting

- If a window doesn’t appear, check your GPU drivers and OpenGL support.
- If the project fails to restore, confirm the package reference and internet access.

## See also

- Next phase: [02 — Single tile](02-single-tile.md)
- Tutorial index: [README.md](README.md)
