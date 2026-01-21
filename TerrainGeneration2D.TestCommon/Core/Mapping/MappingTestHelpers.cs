using System.Reflection;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.TestCommon.Core.Mapping;

public static class MappingTestHelpers
{
  public static T? GetPrivateField<T>(object obj, string fieldName)
  {
    ArgumentNullException.ThrowIfNull(obj);
    ArgumentException.ThrowIfNullOrEmpty(fieldName);
    
    var type = obj.GetType();
    var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
    return (T?)field?.GetValue(obj);
  }

  public static object? InvokePrivateMethod(object obj, string methodName, params object[]? parameters)
  {
    ArgumentNullException.ThrowIfNull(obj);
    ArgumentException.ThrowIfNullOrEmpty(methodName);
    
    var type = obj.GetType();
    var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
    return method?.Invoke(obj, parameters);
  }
}
