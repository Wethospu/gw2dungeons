using System.Collections.Generic;

namespace DataCreator.Encounters
{
  /// <summary>
  /// An object which represents an instance. Contains its encounters and paths.
  /// </summary>
  public class InstanceData
  {
    /// <summary>
    /// Encounters can be shared between paths so they can't be under them.
    /// </summary>
    public List<Encounter> Encounters = new List<Encounter>();
    public List<PathData> Paths = new List<PathData>();
  }
}
