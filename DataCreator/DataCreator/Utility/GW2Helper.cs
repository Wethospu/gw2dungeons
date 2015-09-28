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

    static public int PathToLevel(string path)
    {
      path = path.ToUpper();
      if (path.Contains("SETTING"))
        return 80;
      if (path.Contains("AC1") || path.Contains("AC2") || path.Contains("AC3"))
        return 35;
      if (path.Contains("ACS"))
        return 30;
      if (path.Contains("CM1") || path.Contains("CM2") || path.Contains("CM3"))
        return 45;
      if (path.Contains("CMS"))
        return 40;
      if (path.Contains("TAU") || path.Contains("TAF"))
        return 55;
      if (path.Contains("TAS"))
        return 50;
      if (path.Contains("SE1") || path.Contains("SE2") || path.Contains("SE3"))
        return 65;
      if (path.Contains("TAS"))
        return 60;
      if (path.Contains("COF1") || path.Contains("COF2") || path.Contains("COF3"))
        return 75;
      if (path.Contains("COFS"))
        return 70;
      if (path.Contains("HOTWS"))
        return 76;
      if (path.Contains("COES"))
        return 78;
      return 80;
    }

    static public string AddTab(int amount)
    {
      var toReturn = new StringBuilder();
      for (; amount > 0; amount--)
        toReturn.Append(Constants.Tab);
      return toReturn.ToString();
    }

    /***********************************************************************************************
     * FindEnemies / 2014-07-27 / Wethospu                                                         * 
     *                                                                                             * 
     * Finds enemies based on given requirements. Empty requirements are excluded.                 *
     *                                                                                             *
     * Returns enemies matching the requirements.                                                  *
     * enemies: List of enemies to search.                                                         *
     * name, category, path: Requirements. Use "" if you want to exclude a requirement.            *
     *                                                                                             * 
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
          if (!enemy.Category.ToLower().Equals(category))
            continue;
        }
        if (path.Length > 0)
        {
          var fail = paths.Any(str => !enemy.Path.ToLower().Contains(str));
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
      // Enemies with same level as the path should have a priority.
      if (foundEnemies.Count > 1)
      {
        // Get path specific level.
        var pathLevel = PathToLevel(path);
        // Filter out enemies with different level.
        for (var index = foundEnemies.Count - 1; index >= 0; index--)
        {
          if (foundEnemies[index].Level != pathLevel)
            foundEnemies.RemoveAt(index);
        }
      }
      return foundEnemies;
    }
  }
}
