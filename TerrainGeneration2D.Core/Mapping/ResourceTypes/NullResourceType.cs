using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

public sealed class NullResourceType : ResourceType
{
  public NullResourceType(int tileId = ResourceTypeIds.Void)
      : base(tileId, "Void")
  {
  }

  public static NullResourceType Instance { get; } = new();

  public override bool EvaluateRules(ResourceRuleContext context)
  {
    return false;
  }
}