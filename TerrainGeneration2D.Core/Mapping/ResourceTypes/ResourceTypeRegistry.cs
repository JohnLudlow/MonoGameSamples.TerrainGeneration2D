using System;
using System.Collections.Generic;
using System.Linq;
using JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.ResourceTypes;

public sealed class ResourceTypeRegistry
{
  private readonly Dictionary<int, ResourceType> _resourceTypes;
  private readonly List<int> _resourceOrder;
  private readonly List<int> _validResourceIds;

  public ResourceTypeRegistry(IEnumerable<ResourceType> tileTypes)
  {
    _resourceTypes = tileTypes.ToDictionary(t => t.ResourceId);
    _resourceOrder = [.. tileTypes.Select(t => t.ResourceId)];
    _validResourceIds = [.. _resourceOrder.Where(id => id != ResourceTypeIds.Void)];
  }

  public ResourceType GetResourceType(int resourceId)
  {
    if (!_resourceTypes.TryGetValue(resourceId, out var resourceType))
    {
      throw new InvalidOperationException($"Resource type {resourceId} is not registered.");
    }

    return resourceType;
  }

  public int TileCount => _resourceOrder.Count;

  public IReadOnlyList<int> ResourceIds => _resourceOrder;
  public IReadOnlyList<int> ValidResourceIds => _validResourceIds;

  public static ResourceTypeRegistry CreateDefault()
  {
    var resourceTypes = new List<ResourceType>
    {
      new FoodResource(),
      new WoodResource(),
      new StoneResource(),
      new GoldResource(),
      new MetalResource()
    };
    return new ResourceTypeRegistry(resourceTypes);
  }
}
