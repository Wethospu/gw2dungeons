using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCreator.Shared
{

  /***********************************************************************************************
   * Tactic / 2014-08-01 / Wethospu                                                              *
   *                                                                                             *
   * Object for one encounter tactic. Contains text and tips.                                    *
   *                                                                                             *
   ***********************************************************************************************/

  public class Tactic
  {
    public string Name { get; set; }
    public List<string> Lines { get; set; }
    public int Scale { get; set; }
    private bool _isActive;

    public Tactic(string type)
    {
      // Tactics are often added when they are needed so make them active by default. / 2015-06-30 / Wethospu
      _isActive = true;
      Name = type;
      Scale = 0;
      Lines = new List<string>();
    }

    public Tactic(string type, int scale) : this(type)
    {
      Scale = scale;
    }

    public Tactic()
    {

    }

    /***********************************************************************************************
     * AddLine / 2014-08-01 / Wethospu                                                             *
     *                                                                                             *
     * Adds one line to this tactic unless inactive.                                               *
     * Lines starting with '|' will be merged to the previous line.                                *
     * Returns whether the tactic was active.                                                      *
     *                                                                                             *
     * line: Line to add.                                                                          *
     *                                                                                             *
     ***********************************************************************************************/

    public bool AddLine(string line)
    {
      if (!_isActive || line.Length == 0)
        return false;
      // Check whether to merge to the previous line.
      if (line[0] == '|')
      {
        line = line.Substring(1);
        if (Lines.Count > 0)
        {
          Lines[Lines.Count - 1] += line;
          return true;
        }
      }
      Lines.Add(line);
      return true;
    }

    /***********************************************************************************************
     * Activate / 2014-08-01 / Wethospu                                                            *
     *                                                                                             *
     * Activates or deactivates this tactic based on whether it was found on given tactic types.   *
     *                                                                                             *
     * tacticTypes: Tactic types to activate.                                                      *
     * minScale-maxScale: Scales to activate.                                                      *
     *                                                                                             *
     ***********************************************************************************************/

    public void Activate(string tacticTypes, int minScale, int maxScale)
    {
      var splitTypes = tacticTypes.Split('|');
      // Check that name matches. / 2015-10-29 / Wethospu
      if (splitTypes.Any(str => Name == str))
      {
        // Check that scale matches (if any). / 2015-10-29 / Wethospu
        if (Scale == 0 || (Scale >= minScale && Scale <= maxScale))
        {
          _isActive = true;
          return;
        }  
      }
      _isActive = false;
    }
  }
}
