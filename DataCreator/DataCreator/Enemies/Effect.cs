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
    public string _type;
    // How many times this effects hit. -1 means variable or unknown. / 2015-09-29 / Wethospu
    public int HitCount;
    public double HitLength;
    // Auras don't really have length so count doesnt make sense. Frequence is used for them. / 2015-09-29 / Wethospu
    public double HitFrequency;
    public List<string> SubEffects = new List<string>();

    public Effect(string type)
    {
      HitCount = 1;
      HitLength = 0.0;
      HitFrequency = 0.0;
      _type = type;
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

    public string ToHtml(string path, double coefficient, string weapon, List<Enemy> enemies, Enemy baseEnemy, int indent)
    {
      var htmlBuilder = new StringBuilder();
      _type = LinkGenerator.CreateEnemyLinks(HandleEffect(_type, 0, weapon, 1, 0, 0, baseEnemy), path, enemies);
      // Replace end dot with double dot if the effect has sub effects (visually looks better). / 2015-10-01 / Wethospu
      if (_type[_type.Length - 1] == '.' && SubEffects.Count > 0)
        _type = _type.Substring(0, _type.Length - 1) + ':';
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<p>").Append(_type).Append("</p>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<ul>").Append(Constants.LineEnding);
      foreach (var subEffect in SubEffects)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<li>");
        htmlBuilder.Append(LinkGenerator.CreateEnemyLinks(HandleEffect(subEffect, coefficient, weapon, HitCount, HitLength, HitFrequency, baseEnemy), path, enemies));
        htmlBuilder.Append("</li>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("</ul>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }

    public enum EffectType
    {
      Damage, Condition, Boon, Control, Agony, None, DamageFixed, DamagePercent, Buff, Healing, HealingPercent
    }

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
    private string HandleEffect(string effectStr, double coefficient, string weapon, int hitCount, double hitLength, double hitFrequency, Enemy baseEnemy)
    {
      /* What is wanted:
      * Damage: <span>(count*stack*amount)</span> over time (<span>amount</span> per hit).
      * Swiftness: (count*stack*amount) over time (amount per hit).
      * count*stack stability: for amount over time (stack per hit)
      * count*stack might: <span>count*stack</span>% more damage for amount over time (<span>stack</span>% more damage per hit)
      */
      // Note: Listing per hit for might/stability may not make sense if there is only stack. / 2015-09-27 / Wethospu
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
      bool variableHitCount = false;
      if (hitCount < 0)
      {
        variableHitCount = true;
        hitCount = 1;
      }
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
          Helper.ShowWarningMessage("Enemy " + baseEnemy.Name + ": Something wrong with line '" + original + "'. Note: ':' can't be used in text!");
          effectStr = effectStr.Remove(index, 1);
          continue;
        }

        var effect = effectStr.Substring(index + 1, effectIndex - index - 1);
        // Effect info format varies based on tag (separated by :). / 2015-08-09 / Wethospu
        var effectData = effect.Split(':');
        var effectType = GetEffectType(tag);

        var category = tag.ToLower();
        var amount = 0.0;
        var duration = 0.0;
        var stacks = 1;
        var buff = "";
        var icon = category;
        var stacksAdditively = true;
        var suffix = "damage";
        if (effectType == EffectType.Damage || effectType == EffectType.DamageFixed || effectType == EffectType.DamagePercent)
        {
          if (effectData[0].Equals("-"))
            amount = coefficient;
          else
            amount = Helper.ParseD(effectData[0]);
          icon = "damage";
          stacks = 1;
        }
        else if (effectType == EffectType.Healing || effectType == EffectType.HealingPercent)
        {
          amount = Helper.ParseD(effectData[0]);
          suffix = "healing";
          icon = "healing";
          stacks = 1;
        }
        if (effectType == EffectType.Boon|| effectType == EffectType.Condition  || effectType == EffectType.Control)
        {
          baseEnemy.Tags.Add(category);
        }
        if (effectType == EffectType.Agony || effectType == EffectType.Boon || effectType == EffectType.Condition)
        {
          duration = Helper.ParseD(effectData[0]);
          if (effectType == EffectType.Agony || category.Equals("bleeding") || category.Equals("torment") || category.Equals("burning")
            || category.Equals("poison") || category.Equals("confusion") || category.Equals("regeneration") || category.Equals("might"))
            amount = duration;
          if (effectData.Length > 1)
            stacks = Helper.ParseI(effectData[1]);
          stacksAdditively = EffectStacksDuration(category);
          if (stacksAdditively || effectType == EffectType.Boon)
            suffix = "seconds";
          if (category.Equals("regeneration"))
            suffix = "healing";
          if (category.Equals("retaliation"))
            suffix = "damage per hit";
          if(category.Equals("might"))
            suffix = "more damage";
          if (category.Equals("retaliation") || category.Equals("might"))
            amount = 1;
        }
        if (effectType == EffectType.Control)
        {
          stacksAdditively = false;
          duration = Helper.ParseD(effectData[0]);
          if (effectData.Length > 1)
            stacks = Helper.ParseI(effectData[1]);
        }
        if (effectType == EffectType.Buff)
        {
          buff = effectData[0];
          icon = buff;
          if (effectData.Length > 1 && effectData[1].Length > 0)
            duration = Helper.ParseD(effectData[1]);
          if (effectData.Length > 2 && effectData[2].Length > 0)
            stacks = Helper.ParseI(effectData[2]);
          if (effectData.Length > 3 && effectData[3].Length > 0)
            icon = effectData[3];
          stacksAdditively = effectData.Length > 4;
        }

        var totalAmount = 0.0;
        var totalLength = 0.0;
        var totalDuration = 0.0;
        if (stacksAdditively)
        {
          totalAmount = hitCount * amount * stacks;
          totalDuration = hitCount * duration * stacks;
          totalLength = hitLength;
          // 3 different values makes the effect very confusing so just remove the hit length. / 2015-10-11 / Wethospu
          if (totalAmount > 0 && totalDuration > 0)
            totalLength = 0;
          stacks = 0;
        }
        else
        {
          amount = amount * stacks;
          totalAmount = hitCount * amount;
          totalDuration = 0;
          // Without damage don't do "over X seconds". Instead, just add the duration (makes control work properly). / 2015-10-01 / Wethospu
          if (amount == 0)
          {
            totalDuration = duration;
            duration = 0;
          }
          totalLength = hitLength + duration;
          stacks = hitCount * stacks;
        }

        AddTagsFromEffectType(effectType, baseEnemy);
        // Syntax: <span class="TAGValue">VALUE</span>
        var replace = new StringBuilder();
        //// Put both total and damage per hit. / 2015-09-08 / Wethospu
        if (amount > 0)
        {
          // All amounts need clientside formatting. / 2015-09-27 / Wethospu
          // Add information as data values so it can be recalculated in the browser when enemy level changes. / 2015 - 09 - 27 / Wethospu
          replace.Append("<span class=\"").Append(EffectTypeToClass(effectType)).Append("\" data-effect=\"").Append(category);
          if (category.Equals("confusion"))
            replace.Append("1");
          replace.Append("\" data-amount=\"").Append(totalAmount).Append("\" data-weapon=\"").Append(weapon).Append("\"></span>");
          replace.Append(" ").Append(suffix);
        }
        if (totalDuration > 0)
        {
          if (amount > 0)
            replace.Append(" over ");
          replace.Append(totalDuration).Append(" second");
          if (totalDuration != 1.0)
            replace.Append("s");
        }
        replace.Append(HitLengthStr(totalLength));
        if (hitCount > 1 && (amount > 0 || duration > 0))
        {
          // Same as above but for a single hit. / 2015-09-27 / Wethospu
          replace.Append(" (");
          if (amount > 0)
          {
            // All amounts need clientside formatting. / 2015-09-27 / Wethospu
            // Add information as data values so it can be recalculated in the browser when enemy level changes. / 2015 - 09 - 27 / Wethospu
            replace.Append("<span class=\"").Append(EffectTypeToClass(effectType)).Append("\" data-effect=\"").Append(category);
            if (category.Equals("confusion"))
              replace.Append("1");
            replace.Append("\" data-amount=\"").Append(amount).Append("\"></span>");
            replace.Append(" ").Append(suffix);
          }
          if (duration > 0)
          {
            if (amount > 0)
              replace.Append(" over ");
            replace.Append(duration).Append(" second");
            if (totalDuration != 1.0)
              replace.Append("s");
          }
          replace.Append(" per hit)");
        }
        if (category.Equals("confusion"))
        {
          suffix = "damage per skill usage";
          replace.Append(". ");
          if (amount > 0)
          {
            replace.Append("<span class=\"").Append(EffectTypeToClass(effectType)).Append("\" data-effect=\"").Append(category).Append("2");
            replace.Append("\" data-amount=\"").Append(totalAmount).Append("\"></span>");
            replace.Append(" ").Append(suffix);
          }
          if (duration > 0)
          {
            if (amount > 0)
              replace.Append(" over ");
            replace.Append(totalLength).Append(" second");
            if (totalLength != 1.0)
              replace.Append("s");
          }
          replace.Append(HitLengthStr(hitLength));
          if (hitCount > 1)
          {
            replace.Append(" (");
            if (amount > 0)
            {
              replace.Append("<span class=\"").Append(EffectTypeToClass(effectType)).Append("\" data-effect=\"").Append(category).Append("2");
              if (category.Equals("confusion"))
                replace.Append("1");
              replace.Append("\" data-amount=\"").Append(amount).Append("\"></span>");
              replace.Append(" ").Append(suffix);
            }
            if (duration > 0)
            {
              if (amount > 0)
                replace.Append(" over ");
              replace.Append(duration).Append(" second");
              if (duration != 1.0)
                replace.Append("s");
            }
            replace.Append(" per hit)");
          }
        }
        // Some effects can have variable or unknown hit count. Just add " per hit" in those cases. / 2015-09-29 / Wethospu
        if (variableHitCount)
          replace.Append(" per hit");
        if (hitFrequency > 0.01)
        {
          replace.Append(" every ").Append(hitFrequency).Append(" second");
          if (hitFrequency != 1.0)
            replace.Append("s");
        }
        if (effectType == EffectType.Buff)
        {
          // Add the buff name (people probably won't recognize all icons). / 2015-09-23 / Wethospu
          replace.Append(" (").Append(buff.Replace('_', ' ')).Append(")");
        }


        var toReplace = tag + ":" + effect;
        effectStr = effectStr.Replace(toReplace, replace.ToString());
        // Get the first subeffect type to display an icon. / 2015-09-09 / Wethospu
        if (firstType.Equals(""))
        {
          if (icon.Equals("-") && effectType == EffectType.Buff)
            startIcon = "Buff";
          else
            startIcon = "<span class=" + Constants.IconClass + " title=\"" + icon.ToLower() + "\"></span>";
          firstType = category;
          firstStacks = stacks;
        }
      }
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
          effectBuilder.Append(" Double damage when moving.");
        if (effectChance.Length > 0)
          effectBuilder.Append(" ").Append(effectChance).Append(" chance per hit.");
      }  
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
      if (str.Equals("daze") || str.Equals("float") || str.Equals("knockback") || str.Equals("knockdown")
          || str.Equals("launch") || str.Equals("pull") || str.Equals("sink") || str.Equals("stun") || str.Equals("taunt"))
        return EffectType.Control;
      if (str.Equals("blind") || str.Equals("chilled") || str.Equals("crippled")
           || str.Equals("fear") || str.Equals("immobilized") || str.Equals("slow")
           || str.Equals("vulnerability") || str.Equals("weakness") || str.Equals("revealed"))
        return EffectType.Condition;
      if (str.Equals("aegis") || str.Equals("fury") || str.Equals("might")
          || str.Equals("protection") || str.Equals("resistance") || str.Equals("stability") || str.Equals("swiftness")
           || str.Equals("quickness") || str.Equals("vigor") || str.Equals("stealth"))
        return EffectType.Boon;
      if (str.Equals("bleeding") || str.Equals("burning") || str.Equals("confusion")
           || str.Equals("poison") || str.Equals("torment"))
        return EffectType.Condition;
      if (str.Equals("regeneration") || str.Equals("retaliation"))
        return EffectType.Boon;

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
        return "agony-value";
      if (type == EffectType.Condition || type == EffectType.Boon)
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
      Helper.ShowWarningMessage("Internal error. Effect type not implemented.");
      return "";
    }

    /***********************************************************************************************
    * HitLengthStr / 2015-09-26 / Wethospu                                                         *
    *                                                                                              *
    * Converts hitLenght to a string. Simplifies code.                                             *
    *                                                                                              *
    ***********************************************************************************************/
    private string HitLengthStr(double hitLength)
    {
      if (hitLength < 0.001)
        return "";
      if (hitLength == 1.0)
        return " over 1 second";
      return " over " + hitLength + " seconds";
    }

    /***********************************************************************************************
    * EffectStacksDuration / 2015-09-26 / Wethospu                                                 *
    *                                                                                              *
    * Returns whether effect stacks duration or intensity.                                         *
    *                                                                                              *
    ***********************************************************************************************/
    private bool EffectStacksDuration(string str)
    {
      if (str.Equals("daze") || str.Equals("float") || str.Equals("knockback") || str.Equals("knockdown")
          || str.Equals("launch") || str.Equals("pull") || str.Equals("sink") || str.Equals("stun") || str.Equals("taunt"))
        return false;
      if (str.Equals("blind") || str.Equals("chilled") || str.Equals("crippled") || str.Equals("fear") || str.Equals("immobilized") || str.Equals("slow")
            || str.Equals("weakness") || str.Equals("revealed"))
        return true;
      if (str.Equals("aegis") || str.Equals("fury") || str.Equals("retaliation") || str.Equals("regeneration")
          || str.Equals("protection") || str.Equals("resistance")|| str.Equals("swiftness")
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
