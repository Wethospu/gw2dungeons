using DataCreator.Shared;
using DataCreator.Utility;
using System.Text;

namespace DataCreator.Enemies
{
  public enum EffectType
  {
    Damage, Condition, Boon, Control, Agony, None, DamageFixed, DamagePercent, Buff, Healing, HealingPercent
  }

  public struct SubEffectInformation
  {
    public string name;
    public double amount;
    public double totalAmount;
    public double duration;
    public double totalDuration;
    public double totalLength;
    public double hitFrequency;
    public int hitCount;
    public double hitLength;
    public int stacks;
    public string buff;
    public string icon ;
    public bool stacksAdditively;
    public string suffix;
    public bool variableHitCount;
  }

  /// <summary>
  /// Functions for constructing an attack subeffect.
  /// </summary>
  class EffectHandler
  {

    /***********************************************************************************************
    * HandleEffect / 2015-04-02 / Wethospu                                                         *
    *                                                                                              *
    * Converts raw effect data to final html output.                                               *
    * Returns the converted output.                                                                * 
    *                                                                                              *
    * effectStr: Raw effect data.                                                                  *
    * weapon: Weapon slot for this skill.                                                          *
    * hitCount: How many times the attack part hits.                                               *
    * hitLength: How long it takes for all hits to hit.                                            *
    * hitFrequency: How often the effect tics. Only relevant for auras.                            *
    * baseEnemy: Enemy which owns this effect. Needed to add tags to the enemy.                    *
    *                                                                                              *
    ***********************************************************************************************/
    public static string HandleEffect(string effectStr, Effect baseEffect, Attack baseAttack, Enemy baseEnemy)
    {
      /* What is wanted:
      * Damage: <span>(count*stack*amount)</span> over time (<span>amount</span> per hit).
      * Swiftness: (count*stack*amount) over time (amount per hit).
      * count*stack stability: for amount over time (stack per hit)
      * count*stack might: <span>count*stack</span>% more damage for amount over time (<span>stack</span>% more damage per hit)
      */
      // Note: Listing per hit for might/stability may not make sense if there is only stack.

      // Keep the original string for error logging purposes.
      var original = string.Copy(effectStr);
      // Copy values from the base effect to avoid changing it. If this ever gets refactored feel free to use them directly.
      SubEffectInformation information = new SubEffectInformation();
      information.hitCount = baseEffect.HitCount;
      information.hitLength = baseEffect.HitLength;
      information.hitFrequency = baseEffect.HitFrequency;
      information.variableHitCount = false;
      effectStr = Helper.ToUpper(LinkGenerator.CreatePageLinks(effectStr));
      var split = effectStr.Split('|');
      effectStr = split[0];
      // Some effects get applied randomly. Don't add total damage/effect in those cases. Refactor this if possible.
      var effectChance = "";
      if (split.Length > 1)
        effectChance = split[1];
      if (effectChance.Length > 0)
        information.hitCount = 1;
      if (information.hitCount < 0)
      {
        information.variableHitCount = true;
        information.hitCount = 1;
      }
      // First effect type determines the icon. / 2015-09-05 / Wethospu
      var firstType = "";
      // The icon gets stack numbers. / 2015-09-05 / Wethospu
      var firstStacks = 0;
      var startIcon = "";
      var index = 0;
      while (true)
      {
        TagData text = TagData.FromString(effectStr, ':', ref index, new[] { ' ', '|', '(' }, new[] { ' ', '|', ')' });
        if (text == null)
          break;
        // Ignore other stuff like enemies. / 2015-08-09 / Wethospu
        if (text.Tag.Contains("="))
          continue;
        if (text.Tag.Length == 0 || text.Data.Length == 0)
        {
          ErrorHandler.ShowWarningMessage("Enemy " + baseEnemy.Name + ": Something wrong with line '" + original + "'. Note: Use '\\:' instead of ':' in the text!");
          effectStr = effectStr.Remove(index, 1);
          continue;
        }
        var effectType = GetEffectType(text.Tag);
        // Effect info format varies based on tag (separated by :). / 2015-08-09 / Wethospu
        information = ExtractInformation(information, effectType, text, baseAttack, baseEnemy);
        AddTags(effectType, baseEnemy);
        information = HandleStackingRules(information);
        StringBuilder replace = GenerateReplace(information, effectType, baseAttack);
        var toReplace = text.Tag + ":" + text.Data;
        effectStr = effectStr.Replace(toReplace, replace.ToString());
        index = index - toReplace.Length + replace.Length;
        // Get the first subeffect type to display an icon. / 2015-09-09 / Wethospu
        if (firstType.Equals(""))
        {
          if (information.icon.Equals("-") && effectType == EffectType.Buff)
            startIcon = "Buff";
          else
            startIcon = "<span class=" + Constants.IconClass + " data-src=\"" + information.icon.ToLower() + "\" title=\"" + Helper.ToUpper(information.name.Replace('_', ' ')) + "\">" + Helper.ToUpper(information.name.Replace('_', ' ')) + "</span>";
          firstType = information.name;
          firstStacks = information.stacks;
        }
      }
      return BuildHTML(firstType, startIcon, firstStacks, effectStr, effectChance).ToString();
    }

    private static SubEffectInformation ExtractInformation(SubEffectInformation effect, EffectType effectType, TagData text, Attack baseAttack, Enemy baseEnemy)
    {
      var effectData = text.Data.Split(':');
      effect.name = text.Tag.ToLower();
      effect.amount = -1;
      effect.duration = 0.0;
      effect.stacks = 1;
      effect.buff = "";
      effect.icon = effect.name;
      effect.stacksAdditively = true;
      effect.suffix = "damage";
      if (effectType == EffectType.Damage || effectType == EffectType.DamageFixed || effectType == EffectType.DamagePercent)
      {
        if (effectData[0].Equals("-"))
          effect.amount = baseAttack.Coefficient;
        else if (effectData[0].Equals("?"))
          effect.amount = -1;
        else
          effect.amount = Helper.ParseD(effectData[0]);
        effect.icon = "damage";
        effect.stacks = 1;
      }
      else if (effectType == EffectType.Healing || effectType == EffectType.HealingPercent)
      {
        if (effectData[0].Equals("-"))
          effect.amount = baseAttack.Coefficient;
        else if (effectData[0].Equals("?"))
          effect.amount = -1;
        else
          effect.amount = Helper.ParseD(effectData[0]);
        effect.suffix = "healing";
        effect.icon = "healing";
        effect.stacks = 1;
      }
      if (effectType == EffectType.Boon || effectType == EffectType.Condition || effectType == EffectType.Control)
      {
        baseEnemy.Tags.Add(effect.name);
      }
      if (effectType == EffectType.Agony || effectType == EffectType.Boon || effectType == EffectType.Condition)
      {
        if (effectData[0].Equals("-"))
          effect.duration = baseAttack.Coefficient;
        else if (effectData[0].Equals("?"))
          effect.duration = -1;
        else
          effect.duration = Helper.ParseD(effectData[0]);
        if (effectType == EffectType.Agony || effect.name.Equals("bleeding") || effect.name.Equals("torment") || effect.name.Equals("burning")
          || effect.name.Equals("poison") || effect.name.Equals("confusion") || effect.name.Equals("regeneration") || effect.name.Equals("might"))
          effect.amount = effect.duration;
        if (effectData.Length > 1)
          effect.stacks = Helper.ParseI(effectData[1]);
        effect.stacksAdditively = EffectStacksDuration(effect.name);
        if (effect.stacksAdditively || effectType == EffectType.Boon)
          effect.suffix = "seconds";
        if (effect.name.Equals("regeneration"))
          effect.suffix = "healing";
        if (effect.name.Equals("retaliation"))
          effect.suffix = "damage per hit";
        if (effect.name.Equals("might"))
          effect.suffix = "more damage";
        if (effect.name.Equals("retaliation") || effect.name.Equals("might"))
          effect.amount = 1;
      }
      if (effectType == EffectType.Control)
      {
        effect.stacksAdditively = false;
        effect.duration = Helper.ParseD(effectData[0]);
        if (effectData.Length > 1)
          effect.stacks = Helper.ParseI(effectData[1]);
      }
      if (effectType == EffectType.Buff)
      {
        effect.buff = effectData[0];
        effect.icon = effect.buff;
        if (effectData.Length > 1 && effectData[1].Length > 0)
          effect.duration = Helper.ParseD(effectData[1]);
        if (effectData.Length > 2 && effectData[2].Length > 0)
          effect.stacks = Helper.ParseI(effectData[2]);
        if (effectData.Length > 3 && effectData[3].Length > 0)
          effect.icon = effectData[3];
        if (effectData.Length > 4)
          effect.stacksAdditively = Helper.ParseI(effectData[4]) > 0 ? true : false;
      }
      return effect;
    }

    private static SubEffectInformation HandleStackingRules(SubEffectInformation effectInformation)
    {
      if (effectInformation.stacksAdditively)
      {
        effectInformation.totalAmount = effectInformation.hitCount * effectInformation.amount * effectInformation.stacks;
        effectInformation.totalDuration = effectInformation.hitCount * effectInformation.duration * effectInformation.stacks;
        effectInformation.totalLength = effectInformation.hitLength;
        // 3 different values makes the effect very confusing so just remove the hit length.
        if (effectInformation.totalAmount > 0 && effectInformation.totalDuration > 0)
          effectInformation.totalLength = 0;
        effectInformation.stacks = 0;
      }
      else
      {
        effectInformation.amount = effectInformation.amount * effectInformation.stacks;
        effectInformation.totalAmount = effectInformation.hitCount * effectInformation.amount;
        effectInformation.totalDuration = 0;
        effectInformation.totalLength = effectInformation.hitLength + effectInformation.duration;
        // Without damage don't do "over X seconds". Instead, just add the duration (makes control work properly).
        if (effectInformation.amount == 0)
        {
          effectInformation.totalDuration = effectInformation.duration;
          effectInformation.duration = 0;
        }
        effectInformation.stacks = effectInformation.hitCount * effectInformation.stacks;
      }
      return effectInformation;
    }

    private static StringBuilder GenerateReplace(SubEffectInformation effect, EffectType effectType, Attack baseAttack)
    {
      // Syntax: <span class="TAGValue">VALUE</span>
      var replace = new StringBuilder();
      var HTMLClass = GetHTMLClass(effectType);
      //// Put both total and damage per hit. / 2015-09-08 / Wethospu
      if (effect.amount > -1)
      {
        // All amounts need clientside formatting. / 2015-09-27 / Wethospu
        // Add information as data values so it can be recalculated in the browser when enemy level changes. / 2015 - 09 - 27 / Wethospu
        replace.Append("<span class=\"").Append(HTMLClass).Append("\" data-effect=\"").Append(effect.name);
        if (effect.name.Equals("confusion"))
          replace.Append("1");
        replace.Append("\" data-amount=\"").Append(effect.totalAmount).Append("\" data-weapon=\"").Append(baseAttack.Weapon).Append("\"></span>");
        replace.Append(" ").Append(effect.suffix);
      }
      if (effect.totalDuration > 0)
      {
        if (effect.amount > -1)
          replace.Append(" over ");
        replace.Append(effect.totalDuration).Append(" second");
        if (effect.totalDuration != 1.0)
          replace.Append("s");
      }
      replace.Append(HitLengthStr(effect.totalLength));
      if (effect.hitCount > 1 && (effect.amount > -1 || effect.duration > 0))
      {
        // Same as above but for a single hit. / 2015-09-27 / Wethospu
        replace.Append("<span class=\"secondary-info\"> (");
        if (effect.amount > -1)
        {
          // All amounts need clientside formatting. / 2015-09-27 / Wethospu
          // Add information as data values so it can be recalculated in the browser when enemy level changes. / 2015 - 09 - 27 / Wethospu
          replace.Append("<span class=\"").Append(HTMLClass).Append("\" data-effect=\"").Append(effect.name);
          if (effect.name.Equals("confusion"))
            replace.Append("1");
          replace.Append("\" data-amount=\"").Append(effect.amount).Append("\"></span>");
          replace.Append(" ").Append(effect.suffix);
        }
        if (effect.duration > 0)
        {
          if (effect.amount > -1)
            replace.Append(" over ");
          replace.Append(effect.duration).Append(" second");
          if (effect.totalDuration != 1.0)
            replace.Append("s");
        }
        replace.Append(" per hit)</span>");
      }
      if (effect.name.Equals("confusion"))
      {
        effect.suffix = "damage per skill usage";
        replace.Append(". ");
        if (effect.amount > -1)
        {
          replace.Append("<span class=\"").Append(HTMLClass).Append("\" data-effect=\"").Append(effect.name).Append("2");
          replace.Append("\" data-amount=\"").Append(effect.totalAmount).Append("\"></span>");
          replace.Append(" ").Append(effect.suffix);
        }
        if (effect.duration > 0)
        {
          if (effect.amount > -1)
            replace.Append(" over ");
          replace.Append(effect.totalLength).Append(" second");
          if (effect.totalLength != 1.0)
            replace.Append("s");
        }
        replace.Append(HitLengthStr(effect.hitLength));
        if (effect.hitCount > 1)
        {
          replace.Append("<span class=\"secondary-info\"> (");
          if (effect.amount > -1)
          {
            replace.Append("<span class=\"").Append(HTMLClass).Append("\" data-effect=\"").Append(effect.name).Append("2");
            if (effect.name.Equals("confusion"))
              replace.Append("1");
            replace.Append("\" data-amount=\"").Append(effect.amount).Append("\"></span>");
            replace.Append(" ").Append(effect.suffix);
          }
          if (effect.duration > 0)
          {
            if (effect.amount > -1)
              replace.Append(" over ");
            replace.Append(effect.duration).Append(" second");
            if (effect.duration != 1.0)
              replace.Append("s");
          }
          replace.Append(" per hit)</span>");
        }
      }
      // Some effects can have variable or unknown hit count. Just add " per hit" in those cases. / 2015-09-29 / Wethospu
      if (effect.variableHitCount)
        replace.Append(" per hit");
      if (effect.hitFrequency > 0.01)
      {
        replace.Append(" every ").Append(effect.hitFrequency).Append(" second");
        if (effect.hitFrequency != 1.0)
          replace.Append("s");
      }
      if (effectType == EffectType.Buff)
      {
        // Add the buff name (people probably won't recognize all icons). / 2015-09-23 / Wethospu
        replace.Append(" (").Append(effect.buff.Replace('_', ' ')).Append(")");
      }
      return replace;
    }

    private static StringBuilder BuildHTML(string firstType, string startIcon, int firstStacks, string effectStr, string effectChance)
    {
      // Syntax: <li>'stacks'<span class="icon" title="effectType'"></span>: 'effect'</li>
      var effectBuilder = new StringBuilder();
      if (!firstType.Equals(""))
      {

        effectBuilder.Append(startIcon);
        if (firstStacks > 1)
          effectBuilder.Append("x").Append(firstStacks);
        if (effectStr.Length > 0)
          effectBuilder.Append(": ");
      }
      if (effectStr.Length > 0)
      {
        effectBuilder.Append(effectStr);
        if (effectBuilder.Length > 0 && effectBuilder[effectBuilder.Length - 1] != '.')
          effectBuilder.Append(".");
        if (firstType.Equals("torment"))
          effectBuilder.Append("<span class=\"secondary-info\"> Double damage when moving.</span>");
        if (effectChance.Length > 0)
          effectBuilder.Append(" ").Append(effectChance).Append(" chance per hit.");
      }
      return effectBuilder;
    }

    /// <summary>
    /// Converts a given string to EffectType.
    /// </summary>
    private static EffectType GetEffectType(string str)
    {
      str = str.ToLower();
      if (str.Equals("damage"))
        return EffectType.Damage;
      if (str.Equals("damage-constant"))
        return EffectType.DamageFixed;
      if (str.Equals("damage-percent"))
        return EffectType.DamagePercent;
      if (str.Equals("healing"))
        return EffectType.Healing;
      if (str.Equals("healing-percent"))
        return EffectType.HealingPercent;
      if (str.Equals("agony"))
        return EffectType.Agony;
      if (str.Equals("buff"))
        return EffectType.Buff;
      if (str.Equals("daze") || str.Equals("float") || str.Equals("knockback") || str.Equals("knockdown") || str.Equals("displacement")
          || str.Equals("launch") || str.Equals("pull") || str.Equals("sink") || str.Equals("stun") || str.Equals("taunt"))
        return EffectType.Control;
      if (str.Equals("blind") || str.Equals("chilled") || str.Equals("crippled")
           || str.Equals("fear") || str.Equals("immobilized") || str.Equals("slow")
           || str.Equals("vulnerability") || str.Equals("weakness") || str.Equals("revealed"))
        return EffectType.Condition;
      if (str.Equals("aegis") || str.Equals("fury") || str.Equals("might")
          || str.Equals("protection") || str.Equals("resistance") || str.Equals("stability") || str.Equals("swiftness")
           || str.Equals("quickness") || str.Equals("vigor") || str.Equals("stealth") || str.Equals("defiance"))
        return EffectType.Boon;
      if (str.Equals("bleeding") || str.Equals("burning") || str.Equals("confusion")
           || str.Equals("poison") || str.Equals("torment"))
        return EffectType.Condition;
      if (str.Equals("regeneration") || str.Equals("retaliation"))
        return EffectType.Boon;

      ErrorHandler.ShowWarningMessage("Effect type " + str + " not recognized.");
      return EffectType.None;
    }

    /// <summary>
    /// Adds necessary tags to a given enemy based on a given effect type.
    /// </summary>
    private static void AddTags(EffectType type, Enemy baseEnemy)
    {
      if (type == EffectType.Agony)
        baseEnemy.Tags.Add("agony");
      else if (type == EffectType.Boon)
        baseEnemy.Tags.Add("boon");
      else if (type == EffectType.Buff)
        baseEnemy.Tags.Add("buff");
      else if (type == EffectType.Condition)
        baseEnemy.Tags.Add("condition");
      else if (type == EffectType.Control)
        baseEnemy.Tags.Add("control");
      else if (type == EffectType.Damage)
        baseEnemy.Tags.Add("damage");
      else if (type == EffectType.DamageFixed)
        baseEnemy.Tags.Add("fixed damage");
      else if (type == EffectType.DamagePercent)
        baseEnemy.Tags.Add("percent damage");
      else if (type == EffectType.Healing || type == EffectType.HealingPercent)
        baseEnemy.Tags.Add("healing");
      else if (type != EffectType.None)
        ErrorHandler.ShowWarningMessage("Internal error. Effect type not implemented.");
    }

    /// <summary>
    /// Returns HTML class for a given effect type.
    /// </summary>
    private static string GetHTMLClass(EffectType type)
    {
      if (type == EffectType.Agony)
        return "agony-value";
      if (type == EffectType.Condition || type == EffectType.Boon || type == EffectType.Control || type == EffectType.Buff)
        return "effect-value";
      if (type == EffectType.Damage)
        return "damage-value";
      if (type == EffectType.DamageFixed)
        return "fixed-value";
      if (type == EffectType.DamagePercent)
        return "percent-value";
      if (type == EffectType.Healing)
        return "healing-value";
      if (type == EffectType.HealingPercent)
        return "healing-percent-value";
      if (type == EffectType.None)
        return "";
      ErrorHandler.ShowWarningMessage("Internal error. Effect type not implemented.");
      return "";
    }

    /// <summary>
    /// Returns string representation for a given hit length.
    /// </summary>
    private static string HitLengthStr(double hitLength)
    {
      if (hitLength < 0.001)
        return "";
      if (hitLength == 1.0)
        return " over 1 second";
      return " over " + hitLength + " seconds";
    }

    /// <summary>
    /// Returns whether a given effect stacks additively or intensively.
    /// </summary>
    private static bool EffectStacksDuration(string str)
    {
      if (str.Equals("daze") || str.Equals("float") || str.Equals("knockback") || str.Equals("knockdown")
          || str.Equals("launch") || str.Equals("pull") || str.Equals("sink") || str.Equals("stun") || str.Equals("taunt"))
        return false;
      if (str.Equals("blind") || str.Equals("chilled") || str.Equals("crippled") || str.Equals("fear") || str.Equals("immobilized") || str.Equals("slow")
            || str.Equals("weakness") || str.Equals("revealed"))
        return true;
      if (str.Equals("aegis") || str.Equals("fury") || str.Equals("retaliation") || str.Equals("regeneration")
          || str.Equals("protection") || str.Equals("resistance") || str.Equals("swiftness")
           || str.Equals("quickness") || str.Equals("vigor") || str.Equals("stealth"))
        return true;
      if (str.Equals("bleeding") || str.Equals("burning") || str.Equals("confusion")
           || str.Equals("poison") || str.Equals("torment") || str.Equals("vulnerability"))
        return false;
      if (str.Equals("might") || str.Equals("stability"))
        return false;
      return true;
    }
  }
}
