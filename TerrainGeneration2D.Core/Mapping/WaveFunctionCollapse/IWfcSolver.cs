namespace JohnLudlow.MonoGameSamples.TerrainGeneration2D.Core.Mapping.WaveFunctionCollapse;


/// <summary>
/// Generic WFC solver interface for any constraint satisfaction domain.
/// </summary>
/// <typeparam name="TCell">Cell coordinate type (e.g., Point, Vector3)</typeparam>
/// <typeparam name="TValue">Value type placed in cells (e.g., int, enum)</typeparam>
public interface IWfcSolver<TCell, TValue>
{
  /// <summary>
  /// Solves the constraint satisfaction problem using WFC algorithm.
  /// </summary>
  /// <param name="config">Solver configuration and constraints</param>
  /// <returns>Solution if found; null if unsatisfiable within constraints</returns>
  WfcSolution<TCell, TValue>? Solve(WfcConfiguration<TCell, TValue> config);
}
