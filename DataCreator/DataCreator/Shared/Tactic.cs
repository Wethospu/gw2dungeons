using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCreator.Shared
{

  /// <summary>
  /// Object for one encounter tactic or tip.
  /// </summary>
  public class Tactic
  {
    public string Name { get; set; }
    public List<string> Lines { get; set; }
    /// <summary>
    /// Fractal scale where this tactic is used.
    /// </summary>
    // Higher scales have a higher difficulty so tactics might be different.
    public int FractalScale { get; set; }
    public bool IsActive;

    public Tactic(string type)
    {
      // Tactics are added when they are needed so make them active by default.
      IsActive = true;
      Name = type;
      FractalScale = 0;
      Lines = new List<string>();
    }

    public Tactic(string type, int scale) : this(type)
    {
      FractalScale = scale;
    }

    public Tactic()
    {

    }

    /// <summary>
    /// Adds a line to this tactic. Lines starting with '|' will be merged to the previous line.
    /// </summary>
    public void AddLine(string line)
    {
      if (line[0] == '|')
      {
        line = line.Substring(1);
        if (Lines.Count > 0)
          Lines[Lines.Count - 1] += line;
      }
      else
        Lines.Add(line);
    }

    /// <summary>
    /// Activates or deactivates this tactic based on whether it was found on given tactic types.
    /// </summary>
    public void Activate(string tacticTypes, int minScale, int maxScale)
    {
      var splitTypes = tacticTypes.Split('|');
      if (splitTypes.Any(str => Name == str))
      {
        if (FractalScale == 0 || (FractalScale >= minScale && FractalScale <= maxScale))
        {
          IsActive = true;
          return;
        }  
      }
      IsActive = false;
    }
  }
}
