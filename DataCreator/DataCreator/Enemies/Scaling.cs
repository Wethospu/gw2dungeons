using DataCreator.Utility;

namespace DataCreator.Enemies
{

  /***********************************************************************************************
   * Scaling / 2014-08-01 / Wethospu                                                             *
   *                                                                                             *
   * Scaling functions to get zero-point values for fractals.                                    *
   *                                                                                             *
   ***********************************************************************************************/

  static class Scaling
  {
    // These values must match statcalculator.js
    private const double HealthNormalScaling = 0.0428557;
    private const double HealthChampionScalingSmall = 0.013333333;
    private const double HealthChampionScalingBig = 0.026666667;
    private const double HealthLegendaryScalingSmall = 0.0125;
    private const double HealthLegendaryScalingBig = 0.025;
    private const double HealthLevel81 = 1.0290;
    private const double HealthLevel82 = 1.05848;
    private const double HealthLevel83 = 1.08796;
    private const double HealthLevel84 = 1.1179;

    static public int HealthToZeroValue(int health, string scalingType, int scale, int level)
    {
      // Apply scale.
      if (scale > 0)
      {
        if (scalingType.Equals("constant") || scalingType.Equals("level"))
        {

        }
        else if (scalingType.Equals("normal"))
        {
          //// Calculate scaling multiplier.
          var scalingMultiplier = 1.0 + HealthNormalScaling * scale;
          //// Multiplier calculated.
          health = (int)(health / scalingMultiplier);
        }
        else if (scalingType.Equals("champion"))
        {
          //// Calculate scaling multiplier.
          // Add 0->1.
          var scalingMultiplier = 1.0 + HealthChampionScalingSmall;
          scale--;
          // Add pattern.
          var tail = scale % 4;
          var pattern = scale - tail;
          scalingMultiplier += HealthChampionScalingBig * pattern / 2 + HealthChampionScalingSmall * pattern / 2;
          // Add tail.
          if (tail > 0)
            scalingMultiplier += HealthChampionScalingBig;
          if (tail > 1)
            scalingMultiplier += HealthChampionScalingBig;
          if (tail > 2)
            scalingMultiplier += HealthChampionScalingSmall;
          //// Multiplier calculated.
          health = (int)(health / scalingMultiplier);
        }
        else if (scalingType.Equals("legendary"))
        {
          //// Calculate scaling multiplier.
          // Add 0->1.
          var scalingMultiplier = 1.0 + HealthLegendaryScalingBig;
          scale--;
          // Add pattern.
          var tail = scale % 4;
          var pattern = scale - tail;
          scalingMultiplier += HealthLegendaryScalingBig * pattern / 2 + HealthLegendaryScalingSmall * pattern / 2;
          // Add tail.
          if (tail > 0)
            scalingMultiplier += HealthLegendaryScalingBig;
          if (tail > 1)
            scalingMultiplier += HealthLegendaryScalingBig;
          if (tail > 2)
            scalingMultiplier += HealthLegendaryScalingSmall;
          //// Multiplier calculated.
          health = (int)(health / scalingMultiplier);
        }
        else
        {
          Helper.ShowWarning("0-point scaling for type " + scalingType + " not implemented. Start coding!");
          return health;
        }
      }
      // Apply level.
      if (level == 80)
      {
      }
      else if (level == 81)
        health = (int)(health / HealthLevel81);
      else if (level == 82)
        health = (int)(health / HealthLevel82);
      else if (level == 83)
        health = (int)(health / HealthLevel83);
      else if (level == 84)
        health = (int)(health / HealthLevel84);
      else
        Helper.ShowWarning("0-point scaling for level " + level + " not implemented. Start coding!");
      return health;
    }

    // Some tests.
    private const double DamageNormalScaling = 0.03;
    // Zero tests.
    private const double DamageChampionScalingSmall = 0.0322;
    private const double DamageChampionScalingBig = 0.0322;
    // Some tests.
    private const double DamageLegendaryScalingSmall = 0.0322;
    private const double DamageLegendaryScalingBig = 0.0322;
    // Zero tests.
    private const double DamageLevel81 = 1.03;
    private const double DamageLevel82 = 1.06;
    private const double DamageLevel83 = 1.09;
    private const double DamageLevel84 = 1.12;

    static public int DamageToZeroValue(int damage, string scalingType, int scale, int level)
    {
      // Apply scale.
      if (scale > 0)
      {
        if (scalingType.Equals("constant") || scalingType.Equals("level"))
        {

        }
        else if (scalingType.Equals("normal"))
        {
          //// Calculate scaling multiplier.
          var scalingMultiplier = 1.0 + DamageNormalScaling * scale;
          //// Multiplier calculated.
          damage = (int)(damage / scalingMultiplier);
        }
        else if (scalingType.Equals("champion"))
        {
          //// Calculate scaling multiplier.
          // Add 0->1.
          var scalingMultiplier = 1.0 + DamageChampionScalingSmall;
          scale--;
          // Add pattern.
          var tail = scale % 4;
          var pattern = scale - tail;
          scalingMultiplier += DamageChampionScalingBig * pattern / 2 + DamageChampionScalingSmall * pattern / 2;
          // Add tail.
          if (tail > 0)
            scalingMultiplier += DamageChampionScalingBig;
          if (tail > 1)
            scalingMultiplier += DamageChampionScalingBig;
          if (tail > 2)
            scalingMultiplier += DamageChampionScalingSmall;
          //// Multiplier calculated.
          damage = (int)(damage / scalingMultiplier);
        }
        else if (scalingType.Equals("legendary"))
        {
          //// Calculate scaling multiplier.
          // Add 0->1.
          var scalingMultiplier = 1.0 + DamageLegendaryScalingBig;
          scale--;
          // Add pattern.
          var tail = scale % 4;
          var pattern = scale - tail;
          scalingMultiplier += DamageLegendaryScalingBig * pattern / 2 + DamageLegendaryScalingSmall * pattern / 2;
          // Add tail.
          if (tail > 0)
            scalingMultiplier += DamageLegendaryScalingBig;
          if (tail > 1)
            scalingMultiplier += DamageLegendaryScalingBig;
          if (tail > 2)
            scalingMultiplier += DamageLegendaryScalingSmall;
          //// Multiplier calculated.
          damage = (int)(damage / scalingMultiplier);
        }
        else
        {
          Helper.ShowWarning("0-point scaling for type " + scalingType + " not implemented. Start coding!");
          return damage;
        }
      }
      // Apply level.
      if (level == 80)
      {
      }
      else if (level == 81)
        damage = (int)(damage / DamageLevel81);
      else if (level == 82)
        damage = (int)(damage / DamageLevel82);
      else if (level == 83)
        damage = (int)(damage / DamageLevel83);
      else if (level == 84)
        damage = (int)(damage / DamageLevel84);
      else
        Helper.ShowWarning("0-point scaling for level " + level + " not implemented. Start coding!");
      return damage;
    }

    // 1: no scaling, 2 = normal scaling, 3 = champion, 4 = legendary
    // 5: only level scales
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
      Helper.ShowWarning("Scaling type " + scalingType + " is not recognized! Use 'normal', 'champion', 'level', 'constant' or 'legendary'!");
      return "0";
    }

    // Tested with some accuracy. Hard to get exact numbers because of damage rounding.
    private const double ArmorLevel81 = 1.02668;
    private const double ArmorLevel82 = 1.05132;
    private const double ArmorLevel83 = 1.07642;

    static public int ArmorToZeroValue(int armor, string scalingType, int level)
    {
      // Armor doesn't scale with fractal level.
      // Apply effect of enemy level.
      if (scalingType.Equals("level") || scalingType.Equals("normal"))
      {
        if (level == 80)
        {
        }
        else if (level == 81)
          armor = (int)(armor / ArmorLevel81);
        else if (level == 82)
          armor = (int)(armor / ArmorLevel82);
        else if (level == 83)
          armor = (int)(armor / ArmorLevel83);
        else
          Helper.ShowWarning("0-point scaling for level " + level + " not implemented. Start coding!");
      }
      return armor;
    }
  }
}
