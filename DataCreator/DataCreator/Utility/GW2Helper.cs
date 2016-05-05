using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataCreator.Enemies;

namespace DataCreator.Utility
{
  /// <summary>
  /// Project specific helper functions (GW2 and website stuff).
  /// </summary>
  public static class Gw2Helper
  {
    /// <summary>
    /// Pre-generated list for recommended agony resist.
    /// </summary>
    public static List<int> RecommendedAgonyResist = new List<int>()
    {
      0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
      0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
      8,   10,  11,  13,  15,  17,  18,  20,  22,  24,
      26,  27,  29,  31,  33,  34,  36,  38,  40,  42,
      43,  45,  47,  49,  50,  52,  54,  56,  58,  59,
      61,  63,  65,  67,  68,  70,  72,  74,  75,  77,
      79,  81,  83,  84,  86,  88,  90,  91,  93,  95,
      97,  99,  100, 102, 104, 106, 107, 109, 111, 113,
      115, 116, 118, 120, 122, 123, 125, 127, 129, 131,
      132, 134, 136, 138, 139, 141, 143, 145, 147, 148,
      150
    };

    /// <summary>
    /// Adds a HTML tab.
    /// </summary>
    public static string AddTab(int amount)
    {
      var toReturn = new StringBuilder();
      for (; amount > 0; amount--)
        toReturn.Append(Constants.Tab);
      return toReturn.ToString();
    }

    /// <summary>
    /// Find enemies based on given requirements. Empty requirements are excluded.
    /// </summary>
    // Needed to generate enemy links.
    public static List<Enemy> FindEnemies(List<Enemy> enemies, string name, string rank, List<string> paths)
    {
      if (enemies == null)
      {
        ErrorHandler.ShowWarning("Critical error while finding enemies. No enemy data!");
        return new List<Enemy>();
      }
      // Ensure requirements are lowercase to remove case sensitivity.
      // Name should also be simplified because javascript can't handle special characters.
      name = Helper.Simplify(name);
      rank = rank.ToLower();
      // Name matches have a different priority.
      // This is needed to allow partial matches without them interfering with perfect matches.
      var nameMatches = new List<Enemy>();
      var altMatches = new List<Enemy>();
      var partialNameMatches = new List<Enemy>();
      var partialAltMatches = new List<Enemy>();
      foreach (var enemy in enemies)
      {
        if (rank.Length > 0)
        {
          if (!enemy.Attributes.Rank.ToLower().Equals(rank))
            continue;
        }
        if (paths.Any(str => !enemy.Paths.Contains(str)))
          continue;
        var match = -1;
        if (name.Length > 0)
        {
          var enemyName = Helper.Simplify(enemy.Name);
          if (enemyName.Equals(name))
            match = 0;
          else if (enemy.AltNames.Contains(name))
            match = 1;
          else if (enemyName.Contains(name))
            match = 2;
          else
          {
            if (enemy.AltNames.Any(altName => altName.Contains(name)))
              match = 3;
          }
        }
        // Without name requirement there is a perfect match.
        if (match == 0 || name.Length == 0)
          nameMatches.Add(enemy);
        else if (match == 1)
          altMatches.Add(enemy);
        else if (match == 2)
          partialNameMatches.Add(enemy);
        else if (match == 3)
          partialAltMatches.Add(enemy);
      }
      // Get enemies from the highest priority.
      List<Enemy> foundEnemies;
      if (nameMatches.Count > 0)
        foundEnemies = nameMatches;
      else if (altMatches.Count > 0)
        foundEnemies = altMatches;
      else if (partialNameMatches.Count > 0)
        foundEnemies = partialNameMatches;
      else
        foundEnemies = partialAltMatches;
      return foundEnemies;
    }

    /// <summary>
    /// Returns scaling id for a given scaling type.
    /// </summary>
    // Common scalings have pre-calculated multipliers.
    public static string ScalingTypeToMode(string scalingType)
    {
      if (scalingType.Equals("") || scalingType.Equals("constant"))
        return "1";
      if (scalingType.Equals("normal"))
        return "2";
      if (scalingType.Equals("champion"))
        return "3";
      if (scalingType.Equals("legendary"))
        return "4";
      if (scalingType.Equals("level"))
        return "5";
      // Some enemies have customized scaling which has to be calculated manually.
      if (scalingType.Contains(":"))
        return scalingType;
      ErrorHandler.ShowWarningMessage("Scaling type " + scalingType + " is not recognized! Use 'normal', 'champion', 'level', 'constant' or 'legendary'!");
      return scalingType;
    }

    /// <summary>
    /// Returns output string for a given scaling type.
    /// </summary>
    public static string ScalingTypeToString(string scalingType)
    {
      if (scalingType.Equals("") || scalingType.Equals("constant"))
        return "None";
      if (scalingType.Equals("normal"))
        return "Normal";
      if (scalingType.Equals("champion"))
        return "Champion";
      if (scalingType.Equals("legendary"))
        return "Legendary";
      if (scalingType.Equals("level"))
        return "Only level";
      return "Custom";
    }

    /// <summary>
    /// Generates an small icon which gives additional information when hovered over.
    /// </summary>
    public static string GenerateHelpIcon(string imageLink, string helpText)
    {
      return "<img class=" + Constants.HelpIconClass + " src =\"" + imageLink + "\" title=\"" + helpText + "\">";
    }
  }
}
