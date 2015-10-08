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
    * name, category, path: Requirements. Use "" if you want to exclude a requirement.             *
    *                                                                                              * 
    ***********************************************************************************************/
    public static List<Enemy> FindEnemies(List<Enemy> enemies, string name, string category, string path)
    {
      if (enemies == null)
      {
        Helper.ShowWarning("Critical error while finding enemies. No enemy data!");
        return new List<Enemy>();
      }
      // Ensure requirements are lowercase.
      // Name should also be simplified because javascript can't handle special characters.
      name = Helper.Simplify(name);
      category = category.ToLower();
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
        if (category.Length > 0)
        {
          if (!enemy.Rank.ToLower().Equals(category))
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
  }
}
