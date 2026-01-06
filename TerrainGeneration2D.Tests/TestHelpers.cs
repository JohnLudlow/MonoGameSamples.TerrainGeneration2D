using System.Reflection;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Tests;

public static class TestHelpers
{
    public static Tileset CreateMockTileset(int tileCount, int tileSize = 20)
    {
        // Create a mock tileset without needing GraphicsDevice
        // We use reflection to bypass the normal constructor requirements
        
        // Calculate rows and columns for the tile count
        var columns = (int)Math.Ceiling(Math.Sqrt(tileCount));
        var rows = (int)Math.Ceiling((double)tileCount / columns);
        
        // Create the tileset using FormatterServices to bypass constructor
        #pragma warning disable SYSLIB0050 // FormatterServices is obsolete but needed for mocking
        var tileset = (Tileset)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Tileset));
        #pragma warning restore SYSLIB0050
        
        // Set the properties using reflection
        SetProperty(tileset, "TileWidth", tileSize);
        SetProperty(tileset, "TileHeight", tileSize);
        SetProperty(tileset, "Columns", columns);
        SetProperty(tileset, "Rows", rows);
        SetProperty(tileset, "Count", tileCount);
        
        return tileset;
    }

    private static void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
        else
        {
            // Property is read-only, need to set the backing field
            var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }

    public static T? GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field != null ? (T?)field.GetValue(obj) : default;
    }

    public static object? InvokePrivateMethod(object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType()
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        return method?.Invoke(obj, parameters);
    }
}
