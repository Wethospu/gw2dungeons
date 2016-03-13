using DataCreator.Utility;
using System.Collections.Generic;

namespace DataCreator.Enemies
{
  public enum EffectType
  {
    Damage, Condition, Boon, Control, Agony, DamageFixed, DamagePercent, Buff, Healing, HealingPercent
  }

  class SubEffect
  {
    public EffectType Name;
    public bool StacksDuration;

    public SubEffect(EffectType name, bool stacksDuration)
    {
      Name = name;
      StacksDuration = stacksDuration;
    }


    public static string GetTag(EffectType type)
    {
      if (type == EffectType.Agony)
        return "agony";
      else if (type == EffectType.Boon)
        return "boon";
      else if (type == EffectType.Buff)
        return "buff";
      else if (type == EffectType.Condition)
        return "condition";
      else if (type == EffectType.Control)
        return "control";
      else if (type == EffectType.Damage)
        return "damage";
      else if (type == EffectType.DamageFixed)
        return "fixed damage";
      else if (type == EffectType.DamagePercent)
        return "percent damage";
      else if (type == EffectType.Healing || type == EffectType.HealingPercent)
        return "healing";
      ErrorHandler.ShowWarningMessage("Internal error. Effect type not implemented.");
      return "";
    }

    /// <summary>
    /// Returns HTML class for a given effect type.
    /// </summary>
    public static string GetHTMLClass(EffectType type)
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
      ErrorHandler.ShowWarningMessage("Internal error. Effect type not implemented.");
      return "";
    }

    public static Dictionary<string, SubEffect> EffectTypes;
    public static void CreateEffectTypes()
    {
      EffectTypes = new Dictionary<string, SubEffect>();
      EffectTypes.Add("damage", new SubEffect(EffectType.Damage, false));
      EffectTypes.Add("damage-constant", new SubEffect(EffectType.DamageFixed, false));
      EffectTypes.Add("damage-percent", new SubEffect(EffectType.DamagePercent, false));
      EffectTypes.Add("healing", new SubEffect(EffectType.Healing, false));
      EffectTypes.Add("healing-percent", new SubEffect(EffectType.HealingPercent, false));
      EffectTypes.Add("agony", new SubEffect(EffectType.Agony, false));
      EffectTypes.Add("buff", new SubEffect(EffectType.Buff, false));
      EffectTypes.Add("daze", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("float", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("knockback", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("knockdown", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("displacement", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("launch", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("pull", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("sink", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("stun", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("taunt", new SubEffect(EffectType.Control, false));
      EffectTypes.Add("bleeding", new SubEffect(EffectType.Condition, false));
      EffectTypes.Add("blind", new SubEffect(EffectType.Condition, true));
      EffectTypes.Add("burning", new SubEffect(EffectType.Condition, false));
      EffectTypes.Add("chilled", new SubEffect(EffectType.Condition, true));
      EffectTypes.Add("confusion", new SubEffect(EffectType.Condition, false));
      EffectTypes.Add("crippled", new SubEffect(EffectType.Condition, true));
      EffectTypes.Add("fear", new SubEffect(EffectType.Condition, true));
      EffectTypes.Add("immobilized", new SubEffect(EffectType.Condition, true));
      EffectTypes.Add("poison", new SubEffect(EffectType.Condition, false));
      EffectTypes.Add("slow", new SubEffect(EffectType.Condition, true));
      EffectTypes.Add("torment", new SubEffect(EffectType.Condition, false));
      EffectTypes.Add("vulnerability", new SubEffect(EffectType.Condition, false));
      EffectTypes.Add("weakness", new SubEffect(EffectType.Condition, true));
      EffectTypes.Add("revealed", new SubEffect(EffectType.Condition, false));
      EffectTypes.Add("aegis", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("fury", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("might", new SubEffect(EffectType.Boon, false));
      EffectTypes.Add("protection", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("quickness", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("regeneration", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("resistance", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("retaliation", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("stability", new SubEffect(EffectType.Boon, false));
      EffectTypes.Add("swiftness", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("vigor", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("stealth", new SubEffect(EffectType.Boon, true));
      EffectTypes.Add("defiance", new SubEffect(EffectType.Boon, false));
    }
  }
}
