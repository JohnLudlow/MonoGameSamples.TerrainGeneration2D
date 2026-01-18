using System;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public static class ResourceTypeIds
{
  public const int Void = 0;
  public const int Food = 1;
  public const int Wood = 2;
  public const int Stone = 3;
  public const int Gold = 4;
  public const int Metal = 4;

}

public abstract class ResourceType
{
  protected ResourceType(int resourceId, string name)
  {
    ResourceId = resourceId;
    Name = name;
  }

  public int ResourceId { get; }
  public string Name { get; }

  public abstract bool EvaluateRules(ResourceRuleContext context);
}
