using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

public sealed class MetalResource : ResourceType
{
    public MetalResource() : base(ResourceTypeIds.Metal, "Metal") { }
    public override bool EvaluateRules(ResourceRuleContext context) => true;
}
