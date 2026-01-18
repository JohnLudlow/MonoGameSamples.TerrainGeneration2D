using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class GoldResource : ResourceType
{
    public GoldResource() : base(ResourceTypeIds.Gold, "Gold") { }
    public override bool EvaluateRules(ResourceRuleContext context) => true;
}
