using System.Collections.Generic;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;

namespace DataCreator.Enemies
{
  /***********************************************************************************************
  * Effect / 2014-08-01 / Wethospu                                                               *
  *                                                                                              *
  * Object for one attack effect. Contains effect type (melee, ranged, etc.) and list of its     *
  * effects.                                                                                     *
  *                                                                                              *
  ***********************************************************************************************/

  public class Effect
  {
    private string _type;
    public int HitCount;
    public double HitLength;
    public readonly List<string> SubEffects = new List<string>();

    public Effect(string type)
    {
      HitCount = 1;
      HitLength = 0.0;
      _type = type;
    }

    public Effect(Effect toCopy)
    {
      _type = string.Copy(toCopy._type);
      HitCount = toCopy.HitCount;
      HitLength = toCopy.HitLength;
      foreach (var subeEffect in toCopy.SubEffects)
        SubEffects.Add(string.Copy(subeEffect));
    }

    /***********************************************************************************************
    * ToHtml / 2014-08-01 / Wethospu                                                               * 
    *                                                                                              * 
    * Converts this effect object to html representration.                                         *
    *                                                                                              *
    * Returns representation.                                                                      *
    * path: Name of current path. Needed for enemy links.                                          *
    * enemies: List of enemies in the path. Needed for enemy links.                                *
    * indent: Base indent for the HTML.                                                            *
    *                                                                                              *
    ***********************************************************************************************/

    public string ToHtml(string path, List<Enemy> enemies, Enemy baseEnemy, int indent)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<p>").Append(Helper.ToUpper(LinkGenerator.CreateEnemyLinks(_type, path, enemies)));
      htmlBuilder.Append("</p>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<ul>").Append(Constants.LineEnding);
      foreach (var subEffect in SubEffects)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<li>");
        htmlBuilder.Append(HandleEffect(LinkGenerator.CreateEnemyLinks(subEffect, path, enemies), HitCount, HitLength, baseEnemy));
        htmlBuilder.Append("</li>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("</ul>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }

    public enum EffectType
    {
      Damage, ConditionEffect, ConditionDamage, BoonEffect, BoonDamage, Control,
      Agony, None, DamageFixed, DamagePercent, Buff, Healing
    }

    /***********************************************************************************************
    * HandleEffect / 2015-04-02 / Wethospu                                                         *
    *                                                                                              *
    * Converts raw effect data to final html output.                                               *
    * Returns the converted output.                                                                * 
    *                                                                                              *
    * effectStr: Raw effect data.                                                                  *
    * hitCount: How many times the attack part hits.                                               *
    * hitLength: How long it takes for all hits to hit.                                            *
    * baseEnemy: Enemy which owns this effect. Needed to add tags to the enemy.                    *
    *                                                                                              *
    ***********************************************************************************************/
    private string HandleEffect(string effectStr, int hitCount, double hitLength, Enemy baseEnemy)
    {
      var original = string.Copy(effectStr);
      effectStr = Helper.ToUpper(LinkGenerator.CreatePageLinks(effectStr));
      var split = effectStr.Split('|');
      effectStr = split[0];
      // Some effects get applied randomly. Don't add total damage/effect in those cases. / 2015-09-15 / Wethospu
      var effectChance = "";
      if (split.Length > 1)
        effectChance = split[1];
      if (effectChance.Length > 0)
        hitCount = 1;
      // First effect type determines the icon. / 2015-09-05 / Wethospu
      var firstType = "";
      // The icon gets stack numbers. / 2015-09-05 / Wethospu
      var firstStacks = 0;
      var startIcon = "";
      for (var index = Helper.FirstIndexOf(effectStr, new[] { ':' }); index < effectStr.Length && index > 0; index = Helper.FirstIndexOf(effectStr, new[] { ':' }, index + 1))
      {
        // Effect format is 'tag':'info'. / 2015-08-09 / Wethospu
        var tagIndex = Helper.LastIndexOf(effectStr, new[] { ' ', '|', '(' }, index);
        var tag = effectStr.Substring(tagIndex + 1, index - tagIndex - 1);
        // Ignore other stuff like enemies. / 2015-08-09 / Wethospu
        if (tag.Contains("="))
          continue;
        var effectIndex = Helper.FirstIndexOf(effectStr, new[] { ' ', '|', ')' }, index);
        // Check for valid ending characters. / 2015-08-09 / Wethospu
        if (effectStr[effectIndex - 1] == '!' || effectStr[effectIndex - 1] == ',' || effectStr[effectIndex - 1] == '.' || effectStr[effectIndex - 1] == ':')
          effectIndex--;
        if (effectIndex - index - 1 < 1)
        {
          Helper.ShowWarningMessage("Something wrong with line " + original + ". Note: ':' can't be used in text!");
          effectStr = effectStr.Remove(index, 1);
          continue;
        }

        var effect = effectStr.Substring(index + 1, effectIndex - index - 1);
        // Effect info format varies based on tag (separated by :). / 2015-08-09 / Wethospu
        var effectData = effect.Split(':');
        var effectType = GetEffectType(tag);

        var category = tag.ToLower();
        var amount = 0.0;
        var amountSecondary = 0.0;
        var duration = 0.0;
        var stacks = 1;
        var buff = "";
        var icon = category;
        if (effectType == EffectType.Damage || effectType == EffectType.DamageFixed || effectType == EffectType.DamagePercent
          || effectType == EffectType.Healing)
        {
          amount = Helper.ParseD(effectData[0]);
        }
        if (effectType == EffectType.BoonDamage && category.Equals("regeneration"))
            amount = Gw2Helper.CalculateTickDamage(category, baseEnemy.Level, baseEnemy.HealingPower);
        if (effectType == EffectType.BoonDamage && category.Equals("retaliation"))
          amount = Gw2Helper.CalculateActivationDamage(category, baseEnemy.Level, baseEnemy.Power);
        if (effectType == EffectType.ConditionDamage && (category.Equals("bleeding")
          || category.Equals("poison") || category.Equals("confusion") || category.Equals("burning") || category.Equals("torment")))
        {
          amount = Gw2Helper.CalculateTickDamage(category, baseEnemy.Level, baseEnemy.ConditionDamage);
        }
        if (effectType == EffectType.ConditionDamage && category.Equals("torment"))
          amountSecondary = 2 * amount;
        if (effectType == EffectType.ConditionDamage && category.Equals("confusion"))
          amountSecondary = Gw2Helper.CalculateActivationDamage(category, baseEnemy.Level, baseEnemy.ConditionDamage);

        if (effectType == EffectType.BoonDamage || effectType == EffectType.BoonEffect || effectType == EffectType.ConditionDamage
          || effectType == EffectType.ConditionEffect || effectType == EffectType.Control)
        {
          baseEnemy.Tags.Add(category);
        }
        if (effectType == EffectType.Agony || effectType == EffectType.BoonDamage || effectType == EffectType.ConditionDamage
          || effectType == EffectType.BoonEffect || effectType == EffectType.ConditionEffect || effectType == EffectType.Control)
        {
          duration = Helper.ParseD(effectData[0]);
          if (effectData.Length > 1)
            stacks = Helper.ParseI(effectData[1]);
        }
        if (effectType == EffectType.Buff)
        {
          icon = "";
          buff = effectData[0];
          if (effectData.Length > 1)
            icon = effectData[1];
          if (effectData.Length > 2)
            duration = Helper.ParseD(effectData[2]);
          if (effectData.Length > 3)
            stacks = Helper.ParseI(effectData[3]);
        }
        AddTagsFromEffectType(effectType, baseEnemy);
        // Syntax: <span class="TAGValue">VALUE</span>
        var replace = new StringBuilder();
        replace.Append("<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
        if (effectType == EffectType.Damage || effectType == EffectType.DamageFixed || effectType == EffectType.DamagePercent)
        {
          icon = "damage";
          stacks = 0;
          // Put both total and damage per hit. / 2015-09-08 / Wethospu
          replace.Append(amount * hitCount).Append("</span> damage");
          if (hitCount > 1)
          {
            replace.Append(HitLengthStr(hitLength)).Append(" (<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
            replace.Append(amount).Append("</span> damage per hit)");
          }
        }
        else if (effectType == EffectType.Healing)
        {
          stacks = 0;
          // Put both total and damage per hit. / 2015-09-08 / Wethospu
          replace.Append(amount * hitCount).Append("</span> healing");
          if (hitCount > 1)
          {
            replace.Append(HitLengthStr(hitLength)).Append(" (<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
            replace.Append(amount).Append("</span> healing per hit)");
          }
        }
        else if (effectType == EffectType.BoonEffect || effectType == EffectType.ConditionEffect || effectType == EffectType.Buff)
        {
          // Put both total and per hit. / 2015-09-08 / Wethospu
          // Note: No overstacking handling. / 2015-09-08 / Wethospu
          replace.Append(duration * hitCount).Append("</span> second");
          if (duration * hitCount != 1.0)
            replace.Append("s");
          if (hitCount > 1)
          {
            replace.Append(HitLengthStr(hitLength)).Append(" (<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
            replace.Append(duration).Append("</span> second");
            if (duration != 1.0)
              replace.Append("s");
            replace.Append(" per hit)");
          }
          // Add the buff name (people probably won't recognize all icons). / 2015-09-23 / Wethospu
          if (effectType == EffectType.Buff)
            replace.Append(" (").Append(buff).Append(")");
        }
        else if (effectType == EffectType.Agony)
        {
          // Damage value based on hitcount, character and fractal scale. / 2015-09-08 / Wethospu
          replace.Append(duration * hitCount).Append("</span> damage").Append(HitLengthStr(duration + hitLength));
          if (hitCount > 1)
          {
            replace.Append(" (<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
            replace.Append(duration).Append("</span> damage").Append(HitLengthStr(duration)).Append(" per hit)");
          }
        }
        else if (effectType == EffectType.BoonDamage || effectType == EffectType.ConditionDamage)
        {
          // Put both total and per hit. / 2015-09-08 / Wethospu
          // Note: No overstacking handling. / 2015-09-08 / Wethospu
          var suffix = "damage";
          if (category.Equals("regeneration"))
            suffix = "healing";
          if (category.Equals("retaliation"))
            suffix = "damage per hit";

          replace.Append(amount * duration * stacks * hitCount).Append("</span> ").Append(suffix).Append(HitLengthStr(duration + hitLength));
          if (hitCount > 1)
          {
            replace.Append(" (<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
            replace.Append(amount * duration * stacks).Append("</span> ").Append(suffix).Append(HitLengthStr(duration)).Append(" per hit)");
          }
          if (category.Equals("confusion"))
          {
            suffix = "damage per skill usage";
            replace.Append(amountSecondary * stacks * hitCount).Append("</span> ").Append(suffix).Append(HitLengthStr(duration + hitLength));
            if (hitCount > 1)
            {
              replace.Append(" (<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
              replace.Append(amountSecondary * stacks).Append("</span> ").Append(suffix).Append(HitLengthStr(duration)).Append(" per hit)");
            }
          }
        }
        else if (effectType == EffectType.Control)
        {
          replace.Append(duration + hitLength).Append("</span> second");
          if (duration * hitCount != 1.0)
            replace.Append("s");
          if (hitCount > 1)
          {
            replace.Append(HitLengthStr(hitLength)).Append(" (<span class=\"").Append(EffectTypeToClass(effectType)).Append("\">");
            replace.Append(duration).Append("</span> second");
            if (duration != 1.0)
              replace.Append("s");
            replace.Append(" per hit)");
          }
        }
        else
        {
          Helper.ShowWarningMessage("Effect type " + category + " not implemented.");
          replace.Clear();
        }
        var toReplace = tag + ":" + effect;
        effectStr = effectStr.Replace(toReplace, replace.ToString());
        // Get the first subeffect type to display an icon. / 2015-09-09 / Wethospu
        if (firstType.Equals(""))
        {
          if (icon.Equals("") && effectType == EffectType.Buff)
            startIcon = "Buff";
          else
            startIcon = "<span class=" + Constants.IconClass + " title=\"" + icon + "\"></span>";
          firstType = category;
          firstStacks = stacks * hitCount;
        }
      }
      // Syntax: <li>'stacks'<span class="icon" title="effectType'"></span>: 'effect'</li>
      var effectBuilder = new StringBuilder();
      if (!firstType.Equals(""))
      {
        if (firstStacks > 1)
          effectBuilder.Append(firstStacks).Append("x");
        effectBuilder.Append(startIcon).Append(": ");
      }
      effectBuilder.Append(effectStr);
      if (effectBuilder.Length > 0 && effectBuilder[effectBuilder.Length - 1] != '.')
        effectBuilder.Append(".");
      if (firstType.Equals("torment"))
        effectBuilder.Append(" Double damage when moving.");
      if (effectChance.Length > 0)
        effectBuilder.Append(" ").Append(effectChance).Append(" chance per hit.");
      return effectBuilder.ToString();
    }

    /***********************************************************************************************
    * GetEffectType / 2015-09-06 / Wethospu                                                        *
    *                                                                                              *
    * Converts a string to its equivalent EffectType.                                              *
    * Displays a warning message if the string is not recognized.                                  *
    *                                                                                              *
    * str: String to convert.                                                                      *
    *                                                                                              *
    ***********************************************************************************************/
    private EffectType GetEffectType(string str)
    {
      str = str.ToLower();
      if (str.Equals("damage"))
        return EffectType.Damage;
      if (str.Equals("constant"))
        return EffectType.DamageFixed;
      if (str.Equals("percent"))
        return EffectType.DamagePercent;
      if (str.Equals("healing"))
        return EffectType.Healing;
      if (str.Equals("agony"))
        return EffectType.Agony;
      if (str.Equals("buff"))
        return EffectType.Buff;
      if (str.Equals("daze") || str.Equals("float") || str.Equals("knockback") || str.Equals("knockdown")
          || str.Equals("launch") || str.Equals("pull") || str.Equals("sink") || str.Equals("stun") || str.Equals("taunt"))
        return EffectType.Control;
      if (str.Equals("blind") || str.Equals("chilled") || str.Equals("crippled")
           || str.Equals("fear") || str.Equals("immobilized") || str.Equals("slow")
           || str.Equals("vulnerability") || str.Equals("weakness") || str.Equals("revealed"))
        return EffectType.ConditionEffect;
      if (str.Equals("aegis") || str.Equals("fury") || str.Equals("might")
          || str.Equals("protection") || str.Equals("resistance") || str.Equals("stability") || str.Equals("swiftness")
           || str.Equals("quickness") || str.Equals("vigor") || str.Equals("stealth"))
        return EffectType.BoonEffect;
      if (str.Equals("bleeding") || str.Equals("burning") || str.Equals("confusion")
           || str.Equals("poison") || str.Equals("torment"))
        return EffectType.ConditionDamage;
      if (str.Equals("regeneration") || str.Equals("retaliation"))
        return EffectType.BoonDamage;

      Helper.ShowWarningMessage("Effect type " + str + " not recognized.");
      return EffectType.None;
    }

    /***********************************************************************************************
    * AddTagsFromEffectType / 2015-06-09 / Wethospu                                                *
    *                                                                                              *
    * Adds necessary tags to given enemy based on effect type (used for search).                   *
    * Displays a warning message if the effect type is not implemented.                            * 
    *                                                                                              *
    * type: Effect type which affects added tags.                                                  *
    *                                                                                              *
    ***********************************************************************************************/
    private void AddTagsFromEffectType(EffectType type, Enemy baseEnemy)
    {
      if (type == EffectType.Agony)
        baseEnemy.Tags.Add("agony");
      else if (type == EffectType.BoonDamage || type == EffectType.BoonEffect)
        baseEnemy.Tags.Add("boon");
      else if (type == EffectType.Buff)
        baseEnemy.Tags.Add("buff");
      else if (type == EffectType.ConditionDamage || type == EffectType.ConditionEffect)
      {
        baseEnemy.Tags.Add("condition");
        if (type == EffectType.ConditionDamage)
          baseEnemy.Tags.Add("condition damage");
        else if (type == EffectType.ConditionEffect)
          baseEnemy.Tags.Add("condition debuff");
      }
      else if (type == EffectType.Control)
        baseEnemy.Tags.Add("control");
      else if (type == EffectType.Damage)
        baseEnemy.Tags.Add("damage");
      else if (type == EffectType.DamageFixed)
        baseEnemy.Tags.Add("fixed damage");
      else if (type == EffectType.DamagePercent)
        baseEnemy.Tags.Add("percent damage");
      else if (type == EffectType.Healing)
        baseEnemy.Tags.Add("healing");
      else if (type != EffectType.None)
        Helper.ShowWarningMessage("Internal error. Effect type not implemented.");
    }

    /***********************************************************************************************
    * EffectTypeToClass / 2015-06-09 / Wethospu                                                    *
    *                                                                                              *
    * Converts an effect type to its corresponding html class.                                     *
    * Displays a warning message if the effect type is not implemented.                            * 
    *                                                                                              *
    * type: Type to convert.                                                                       *
    *                                                                                              *
    ***********************************************************************************************/
    private string EffectTypeToClass(EffectType type)
    {
      if (type == EffectType.Agony)
        return "agonyValue";
      if (type == EffectType.BoonDamage || type == EffectType.BoonEffect)
        return "normalValue";
      if (type == EffectType.Buff)
        return "normalValue";
      if (type == EffectType.ConditionDamage || type == EffectType.ConditionEffect)
        return "conditionValue";
      if (type == EffectType.Control)
        return "normalValue";
      if (type == EffectType.Damage)
        return "damageValue";
      if (type == EffectType.DamageFixed)
        return "fixedValue";
      if (type == EffectType.DamagePercent)
        return "percentValue";
      if (type == EffectType.Healing)
        return "normalValue";
      if (type == EffectType.None)
        return "";
      Helper.ShowWarningMessage("Internal error. Effect type not implemented.");
      return "";
    }

    private string HitLengthStr(double hitLength)
    {
      if (hitLength < 0.001)
        return "";
      if (hitLength == 1.0)
        return " over 1 second";
      return " over " + hitLength + " seconds";
    }
  }
}
