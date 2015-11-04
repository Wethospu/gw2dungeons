using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataCreator.Enemies;

namespace DataCreator.Utility
{
  /***********************************************************************************************
   * GW2Helper / 2014-08-01 / Wethospu                                                           *
   *                                                                                             *
   * Project specific helper functions (GW2 and website stuff).                                  *
   *                                                                                             *
   ***********************************************************************************************/

  public static class Gw2Helper
  {
    /***********************************************************************************************
     * PathToLevel / 2014-08-01 / Wethospu                                                         *
     *                                                                                             *
     * Returns level for given path.                                                               *
     *                                                                                             *
     ***********************************************************************************************/
    private static Dictionary<string, int> pathToLevel = new Dictionary<string, int>
    {
        { "acs", 30 }, { "ac1", 35 }, { "ac2", 35 }, { "ac3", 35 },
        { "cms", 40 }, { "cm1", 45 }, { "cm2", 45 }, { "cm3", 45 },
        { "tas", 50 }, { "taf", 55 }, { "tau", 55 }, { "taae", 80 },
        { "ses", 60 }, { "se1", 65 }, { "se2", 65 }, { "se3", 65 },
        { "cofs", 70 }, { "cof1", 75 }, { "cof2", 75 }, { "cof3", 75 },
        { "hotws", 76 }, { "coes", 78 }
    };

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

    public static int PathToLevel(string path)
    {
      if (pathToLevel.ContainsKey(path))
        return pathToLevel[path];
      return 80;
    }

    public static string AddTab(int amount)
    {
      var toReturn = new StringBuilder();
      for (; amount > 0; amount--)
        toReturn.Append(Constants.Tab);
      return toReturn.ToString();
    }

    /***********************************************************************************************
    * FindEnemies / 2014-07-27 / Wethospu                                                          * 
    *                                                                                              * 
    * Finds enemies based on given requirements. Empty requirements are excluded.                  *
    *                                                                                              *
    * Returns enemies matching the requirements.                                                   *
    * enemies: List of enemies to search.                                                          *
    * name, rank, path: Requirements. Use "" if you want to exclude a requirement.                 *
    *                                                                                              * 
    ***********************************************************************************************/
    public static List<Enemy> FindEnemies(List<Enemy> enemies, string name, string rank, string path)
    {
      if (enemies == null)
      {
        Helper.ShowWarning("Critical error while finding enemies. No enemy data!");
        return new List<Enemy>();
      }
      // Ensure requirements are lowercase.
      // Name should also be simplified because javascript can't handle special characters.
      name = Helper.Simplify(name);
      rank = rank.ToLower();
      path = path.ToLower();
      var paths = path.Split('|');
      // Enemies whos base name matches exactly.
      var nameMatches = new List<Enemy>();
      // Enemies whos any alt name matches exactly.
      var altMatches = new List<Enemy>();
      // Enemies who have partial base name match.
      var partialNameMatches = new List<Enemy>();
      // Enemies who have partial alt name match.
      var partialAltMatches = new List<Enemy>();
      foreach (var enemy in enemies)
      {
        if (rank.Length > 0)
        {
          if (!enemy.Rank.ToLower().Equals(rank))
            continue;
        }
        if (path.Length > 0)
        {
          var fail = paths.Any(str => !enemy.Paths.Contains(str));
          if (fail)
            continue;
        }
        // Name matches have a different priority.
        // This is needed to allow partial matches without them interfering with perfect matches.
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

    // 1: no scaling, 2 = normal scaling, 3 = champion, 4 = legendary, 5: only level scales
    // Anything else: custom.
    static public string ScalingTypeToMode(string scalingType)
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
      if (scalingType.Contains(":"))
        return scalingType;
      Helper.ShowWarningMessage("Scaling type " + scalingType + " is not recognized! Use 'normal', 'champion', 'level', 'constant' or 'legendary'!");
      return scalingType;
    }

    static public string ScalingTypeToString(string scalingType)
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
  }
}
