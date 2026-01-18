using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class StoneResource : ResourceType
{
    public StoneResource() : base(ResourceTypeIds.Stone, "Stone") { }
    public override bool EvaluateRules(ResourceRuleContext context) => true;
}
