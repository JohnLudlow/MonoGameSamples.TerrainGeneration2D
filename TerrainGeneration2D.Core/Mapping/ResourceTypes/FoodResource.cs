using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

public sealed class FoodResource : ResourceType
{
  public FoodResource(int tileId = ResourceTypeIds.Food)
      : base(tileId, "Food")
  {
  }

  public static NullResourceType Instance { get; } = new();

  public override bool EvaluateRules(ResourceRuleContext context)
  {
    return false;
  }
}