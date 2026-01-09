# Phase 03 - Logging

In this phase you will:

- Add structured logging with Microsoft.Extensions.Logging
- Log generation phases and input actions
- Optimize hot-path logging using `LoggerMessage` source generator
- Configure logging levels via `appsettings.json` (Default=DEBUG, key namespaces)

## 0. Write tests (TDD)

Create `TerrainGeneration2D.Tests/LoggingTests.cs`:

```csharp
using Microsoft.Extensions.Logging;

namespace TerrainGeneration2D.Tests;

public class LoggingTests
{
    [Fact]
    public void LoggerFactory_CreatesLogger()
    {
        using var factory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Information));
        var logger = factory.CreateLogger("test");
        Assert.NotNull(logger);
    }
}
```

## 1. Add logging packages (Game)

```bash
cd src/TerrainGeneration2D
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Logging.Configuration
dotnet add package Microsoft.Extensions.Logging.Console
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.FileExtensions
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.EnvironmentVariables
```

Note: You do not need a separate “Generators” package. The `LoggerMessage` source generator is included with modern `Microsoft.Extensions.Logging` for current target frameworks.

### 1a. Configure logging via appsettings.json

Add `appsettings.json` to your game project so log levels are configurable without code changes. Use a default of DEBUG and add major namespaces for clarity.

Create `TerrainGeneration2D/appsettings.json`:

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Debug",
            "Microsoft": "Warning",
            "System": "Warning",
            "MonoGame": "Information",
            "Gum": "Information",
            "TerrainGeneration2D": "Debug",
            "JohnLudlow.TerrainGeneration2D.Performance": "Information"
        }
    }
}
```

Ensure it’s copied to the output directory by updating your game csproj:

```xml
<ItemGroup>
    <None Update="appsettings.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.Development.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.Production.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.json.*">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

Wire configuration into logging in `Program.cs` or your bootstrap code:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

static class Log
{
        private static readonly IConfiguration _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>
        {
                builder
                        .AddConfiguration(_config.GetSection("Logging"))
                        .AddConsole();
        });

        public static ILogger Create<T>() => Factory.CreateLogger<T>();
}
```

## 2. Optional: minimal factory (no config file)

If you prefer to skip `appsettings.json`, you can use a minimal factory setup:

```csharp
using Microsoft.Extensions.Logging;

static class Log
{
    public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>
    {
        builder
            .SetMinimumLevel(LogLevel.Information)
            .AddConsole();
    });

    public static ILogger Create<T>() => Factory.CreateLogger<T>();
}
```

## 3. Use the logger (basic)

TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
private readonly ILogger _log = Log.Create<TerrainGenerationGame>();

protected override void Initialize()
{
    _log.LogInformation("Initialize window {w}x{h}", 1280, 720);
    base.Initialize();
}

protected override void LoadContent()
{
    _log.LogInformation("Loading tileset");
    base.LoadContent();
}
```

### 3a. Verify config-driven levels

Add a few log lines at different levels and run the game to confirm filtering via `appsettings.json`.

```csharp
protected override void Initialize()
{
        _log.LogTrace("Trace check");
        _log.LogDebug("Debug check");
        _log.LogInformation("Information check");
        _log.LogWarning("Warning check");
        base.Initialize();
}
```

Run with the default config (Default=Debug from appsettings.json) — you should see Debug/Information/Warning (Trace is filtered):

```pwsh
dotnet run --project src/TerrainGeneration2D/TerrainGeneration2D.csproj
```

Tighten the level to Warning to suppress Debug/Information — edit `appsettings.json`:

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Warning"
        }
    }
}
```

Run again — only Warning (and above) appears.

Use environment-specific overrides by creating `appsettings.Development.json`:

```json
{
    "Logging": {
        "LogLevel": { "Default": "Trace" }
    }
}
```

Then set the environment and run:

```pwsh
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --project src/TerrainGeneration2D/TerrainGeneration2D.csproj
# Optional: clear when done
Remove-Item Env:DOTNET_ENVIRONMENT
```

## 4. Optimize with `LoggerMessage` (compile-time generated methods)

Create a partial class to surface strongly-typed logging APIs without runtime boxing/formatting overhead:

```csharp
using Microsoft.Extensions.Logging;

static partial class GenLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Initialize window {width}x{height}")]
    public static partial void InitWindow(ILogger logger, int width, int height);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Loading tileset")]
    public static partial void LoadingTileset(ILogger logger);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Begin generate {width}x{height}")]
    public static partial void GenerateBegin(ILogger logger, int width, int height);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "End generate success={success}")]
    public static partial void GenerateEnd(ILogger logger, bool success);
}
```

Use them in your game host class.
TerrainGeneration2D/TerrainGenerationGame.cs — code excerpt; unrelated members omitted for brevity:

```csharp
private readonly ILogger _log = Log.Create<GameHost>();

protected override void Initialize()
{
    GenLog.InitWindow(_log, 1280, 720);
    base.Initialize();
}

protected override void LoadContent()
{
    GenLog.LoadingTileset(_log);
    base.LoadContent();
}

void Generate()
{
    GenLog.GenerateBegin(_log, _mapWidth, _mapHeight);
    // ... generation work ...
    GenLog.GenerateEnd(_log, success: true);
}
```

Notes:

- The `Microsoft.Extensions.Logging.Generators` package emits source at compile-time.
- Generated methods avoid string interpolation and object array allocations in hot paths.
- Keep `EventId`s stable and documented.

## 5. Correlate with diagnostics

- Emit a log when you call `TerrainPerformanceEventSource.Log.*` events such as `UpdateActiveChunksBegin/End`, `ChunkLoadBegin/End`, `ChunkSaveBegin/End`, or `WaveFunctionCollapseBegin/End`.
- Aligns trace logs with counters like `active-chunk-count` and `chunks-saved-per-second` when debugging performance (see Diagnostics README).

## 6. Next steps

- Add file logging provider.
- Introduce logging scopes around generation and drawing.
- Route errors to a telemetry pipeline (Seq, OpenTelemetry).

See also:

- Previous phase: [02 — Single tile](02-single-tile.md)
- Next phase: [04 — Performance](04-performance.md)
- Tutorial index: [README.md](README.md)
