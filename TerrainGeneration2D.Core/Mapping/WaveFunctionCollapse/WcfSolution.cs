using System;
using System.Collections.Generic;

namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;


/// <summary>
/// Represents a solved WFC configuration, mapping cells to their assigned values.
/// </summary>
/// <typeparam name="TCell">Cell coordinate type</typeparam>
/// <typeparam name="TValue">Value type</typeparam>
public class WfcSolution<TCell, TValue>
{
    /// <summary>
    /// Gets the mapping of cells to their solved values.
    /// </summary>
    public IReadOnlyDictionary<TCell, TValue> Assignments { get; }

    /// <summary>
    /// Initializes a new instance of the WfcSolution class.
    /// </summary>
    /// <param name="assignments">The solved cell-value assignments</param>
    public WfcSolution(IReadOnlyDictionary<TCell, TValue> assignments)
    {
        Assignments = assignments ?? throw new ArgumentNullException(nameof(assignments));
    }
}


