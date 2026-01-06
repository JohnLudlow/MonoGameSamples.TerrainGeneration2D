using System;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;

/// <summary>
/// Kinds of reversible mutations captured by the change log.
/// </summary>
internal enum ChangeKind { DomainRemoved, CellCollapsed, OutputSet }
