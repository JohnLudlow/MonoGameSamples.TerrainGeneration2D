using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class WoodResource : ResourceType
{
    public WoodResource() : base(ResourceTypeIds.Wood, "Wood") { }
    public override bool EvaluateRules(ResourceRuleContext context) => true;
}
