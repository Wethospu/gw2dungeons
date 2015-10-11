using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMerger
{
  class EnemyData
  { 
    [JsonProperty("occurrences")]
    public Occurrence[] Occurrences { get; set; }
  }

  public class Occurrence : IComparable
  {
    public Occurrence()
    {
      MapID = -1;
      Health = -1;
      Level = -1;
      ScaledLevel = -1;
      Position = new Position();
      Stamp = -1;
    }

    [JsonProperty("mapId")]
    public int MapID { get; set; }

    [JsonProperty("health")]
    public int Health { get; set; }

    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("scaled_level")]
    public int ScaledLevel { get; set; }

    [JsonProperty("position")]
    public Position Position { get; set; }

    [JsonProperty("stamp")]
    public int Stamp { get; set; }

    public int CompareTo(object obj)
    {
      var toCompare = (Occurrence)(obj);
      if (toCompare == null)
        return 0;
      if (MapID != toCompare.MapID)
        return MapID > toCompare.MapID ? 1 : -1;
      if (Level != toCompare.Level)
        return Level > toCompare.Level ? 1 : -1;
      if (ScaledLevel != toCompare.ScaledLevel)
        return ScaledLevel > toCompare.ScaledLevel  ? 1 : -1;
      if (Position == null && toCompare.Position != null)
        return 1;
      if (Position == null && toCompare.Position == null)
        return 0;
      if (Position != null && toCompare.Position == null)
        return -1;
      if (Position.CompareTo(toCompare.Position) != 0)
        return Position.CompareTo(toCompare.Position);
      if (Stamp != toCompare.Stamp)
        return Stamp > toCompare.Stamp ? 1 : -1;
      return 0;
    }
  }

  public class Position : IComparable
  {
    public Position()
    {
      X = 0;
      Y = 0;
      Z = 0;
    }

    [JsonProperty("x")]
    public double X { get; set; }

    [JsonProperty("y")]
    public double Y { get; set; }

    [JsonProperty("z")]
    public double Z { get; set; }

    public int CompareTo(object obj)
    {
      var toCompare = (Position)(obj);
      if (toCompare == null)
        return 0;
      if (X != toCompare.X)
        return X > toCompare.X ? 1 : -1;
      if (Y != toCompare.Y)
        return Y > toCompare.Y ? 1 : -1;
      if (Z != toCompare.Z)
        return Z > toCompare.Z ? 1 : -1;
      return 0;
    }
  }
}
