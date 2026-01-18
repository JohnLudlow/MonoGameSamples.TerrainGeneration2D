using System.Collections.Generic;
using System;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.TileTypes;

/// <summary>
/// Describes group/rule configuration for a single terrain or resource type.
/// </summary>
public sealed class GroupRuleConfiguration
{
    public int Id { get; set; } // Terrain or resource type id
    public int MinGroupSizeX { get; set; }
    public int MinGroupSizeY { get; set; }
    public int MaxGroupSizeX { get; set; }
    public int MaxGroupSizeY { get; set; }
    public float ElevationMin { get; set; }
    public float ElevationMax { get; set; }
    public string? NoiseProvider { get; set; } // e.g., "mountain", "detail", etc.
    public float? NoiseThreshold { get; set; }
}

/// <summary>
/// Holds a collection of group/rule configurations for all terrain types.
/// </summary>
public sealed class TileTypeRuleConfiguration
{
    public List<GroupRuleConfiguration> Rules { get; } = [];

    public GroupRuleConfiguration? GetRuleForType(int id)
        => Rules.Find(r => r.Id == id);
}