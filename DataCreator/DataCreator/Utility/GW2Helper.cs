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
    * CalculateArmor / 2015-09-16 / Wethospu                                                       *
    *                                                                                              *
    * Calculates armor for given level and toughness multiplier.                                   *
    *                                                                                              *
    ***********************************************************************************************/

    static public double CalculateArmor(int level, double multiplier)
    {
      return MonsterDefenseLut[level] + MonsterAttributeLut[level] * multiplier;
    }


    /***********************************************************************************************
    * CalculateTickDamage / 2015-09-15 / Wethospu                                                  *
    *                                                                                              *
    * Calculates tick damage for given effect, enemy level and attribute.                          *
    *                                                                                              *
    ***********************************************************************************************/

    static public double CalculateTickDamage(string effect, int level, int attribute)
    {
      // Multiply values by 10000 to prevent rounding. / 2015-09-15 / Wethospu
      // Note: Can represent accurately 210k tick damage. / 2015-09-15 / Wethospu
      var damage = 0.0d;
      if (effect.Equals("bleeding"))
        damage = 20000 + 2500 * level + 600 * attribute;
      else if (effect.Equals("burning"))
        damage = 75000 + 15500 * level + 1550 * attribute;
      else if (effect.Equals("poison"))
        damage = 35000 + 3750 * level + 600 * attribute;
      else if (effect.Equals("confusion"))
        damage = 20000 + 1000 * level + 350 * attribute;
      else if (effect.Equals("torment"))
        damage = 15000 + 1800 * level + 450 * attribute;
      else if (effect.Equals("regeneration"))
        damage = 50000 + 15625 * level + 1250 * attribute;
      else
        Helper.ShowWarningMessage("No tick damage exists for " + effect + ".");
      return Math.Floor(damage / 10000);
    }

    /***********************************************************************************************
    * CalculateActivationkDamage / 2015-09-15 / Wethospu                                           *
    *                                                                                              *
    * Calculates activation damage for given effect, enemy level and attribute.                    *
    *                                                                                              *
    ***********************************************************************************************/

    static public double CalculateActivationDamage(string effect, int level, int attribute)
    {
      // Multiply values by 10000 to prevent rounding. / 2015-09-15 / Wethospu
      // Note: Can represent accurately 210k activation damage. / 2015-09-15 / Wethospu
      var damage = 0.0d;
      if (effect.Equals("confusion"))
      {
        damage = 35000 + 5750 * level + 625 * attribute;
        damage = Math.Floor(damage / 10000);
      }
      else if (effect.Equals("retaliation"))
      {
        damage = 80000 + 300 * level * level + 750 * attribute;
        damage = Math.Round(damage / 10000);
      }
      else
        Helper.ShowWarningMessage("No activation damage exists for " + effect + ".");
      return damage;
    }

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
     * name, category, level, path: Requirements. Use "" if you want to exclude a requirement.     *
     *                                                                                             * 
     ***********************************************************************************************/
    public static List<Enemy> FindEnemies(List<Enemy> enemies, string name, string category, int level, string path)
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
        if (level > 0)
        {
          if (enemy.Level != level)
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
      // If no level was given, enemies with same level as the path should have a priority.
      if (level == 0 && foundEnemies.Count > 1)
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


    static int[] MonsterHealthLut = new int[101]{
      0,   20,   40,   60,   80,  100,  120,  140,  160,  180,
      200,  220,  240,  260,  265,  270,  292,  306,  319,  332,
      370,  408,  462,  501,  538,  575,  611,  646,  713,  750,
      818,  855,  892,  928,  995, 1032, 1103, 1141, 1178, 1215,
      1328, 1410, 1531, 1614, 1695, 1776, 1892, 1974, 2101, 2183,
      2265, 2346, 2464, 2547, 2677, 2760, 2842, 2924, 3040, 3122,
      3303, 3432, 3561, 3688, 3852, 3980, 4226, 4357, 4487, 4616,
      4774, 4904, 5164, 5295, 5426, 5557, 5710, 5841, 6112, 6245,
      6580, 6780, 6980, 7180, 7380, 7580, 7780, 7980, 8180, 8380,
      8580, 8780, 8980, 9180, 9380, 9580, 9780, 9980,10180,10380,
      10580
    };

    static int[] MonsterDefenseLut = new int[101]{
      123,  128,  134,  138,  143,  148,  153,  158,  162,  167,
      175,  183,  185,  187,  190,  192,  202,  206,  210,  214,
      220,  224,  239,  245,  250,  256,  261,  267,  285,  291,
      311,  320,  328,  337,  356,  365,  385,  394,  402,  411,
      432,  443,  465,  476,  486,  497,  517,  527,  550,  561,
      575,  588,  610,  624,  649,  662,  676,  690,  711,  725,
      752,  769,  784,  799,  822,  837,  878,  893,  909,  924,
      949,  968, 1011, 1030, 1049, 1067, 1090, 1109, 1155, 1174,
      1223, 1247, 1271, 1295, 1319, 1343, 1367, 1391, 1415, 1439,
      1463, 1487, 1511, 1535, 1559, 1583, 1607, 1631, 1655, 1679,
      1703
    };

    static int[] MonsterAttributeLut = new int[101]{
      5,   10,   17,   22,   27,   35,   45,   50,   55,   60,
      68,   76,   84,   92,   94,   95,  103,  108,  112,  116,
      123,  129,  140,  147,  153,  160,  166,  171,  186,  192,
      208,  219,  230,  238,  253,  259,  274,  279,  284,  290,
      304,  317,  339,  353,  366,  380,  401,  416,  440,  454,
      471,  488,  514,  532,  561,  579,  598,  617,  643,  662,
      696,  718,  741,  765,  795,  818,  866,  891,  916,  941,
      976, 1004, 1059, 1089, 1119, 1149, 1183, 1214, 1274, 1307,
      1374, 1413, 1453, 1493, 1534, 1575, 1616, 1658, 1700, 1743,
      1786, 1829, 1873, 1917, 1961, 2006, 2052, 2098, 2144, 2190,
      2237
    };
  }
}
