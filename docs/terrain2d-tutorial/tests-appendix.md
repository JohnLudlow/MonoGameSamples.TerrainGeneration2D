# Tests Appendix (TDD helpers)

This appendix helps you set up and run tests during the tutorial.

## Create the test project

```bash
cd src
dotnet new xunit -n TerrainGeneration2D.Tests -f net10.0
cd TerrainGeneration2D.Tests
dotnet add reference ../TerrainGeneration2D.Core/TerrainGeneration2D.Core.csproj
```

## Add packages

```bash
dotnet add package Microsoft.NET.Test.Sdk
```

## Useful commands

```bash
dotnet test TerrainGeneration2D.Tests/TerrainGeneration2D.Tests.csproj
```

## Sample helpers

Create `TerrainGeneration2D.Tests/TestHelpers.cs`:

```csharp
namespace TerrainGeneration2D.Tests;

public static class TestHelpers
{
    public static int[,] CreateMap(int w, int h, int fill = 0)
    {
        var m = new int[w, h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) m[x,y] = fill;
        return m;
    }
}
```

Use these in each phase’s tests.
