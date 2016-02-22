using System.Text;
using DataCreator.Utility;

namespace DataCreator.Enemies
{
  public enum DamageType { Damage, ConditionEffect, ConditionDamage, CrowdControl, Agony, MawAgony, None, Constant, Percent, Buff }

  /***********************************************************************************************
   * Damage / 2014-08-01 / Wethospu                                                              *
   *                                                                                             *
   * Object for one effect subeffect. Used for damage, conditions, boons, etc.                   *
   *                                                                                             *
   ***********************************************************************************************/

  public class SubEffect
  {
    public string Amount = "";
    public DamageType Category = DamageType.None;
    public string Type = "";
    public string Duration = "";
    public string Stacks = "";
    public string Additional = "";

    public SubEffect()
    {
    }

    public SubEffect(SubEffect toCopy)
    {
      Amount = string.Copy(toCopy.Amount);
      Category = toCopy.Category;
      Type = string.Copy(toCopy.Type);
      Duration = string.Copy(toCopy.Duration);
      Stacks = string.Copy(toCopy.Stacks);
      Additional = string.Copy(toCopy.Additional);
    }

    /***********************************************************************************************
     * ToHtml / 2014-08-01 / Wethospu                                                              * 
     *                                                                                             * 
     * Converts this damage object to html representration.                                        *
     *                                                                                             *
     * Returns representation.                                                                     *
     *                                                                                             *
     ***********************************************************************************************/

    public string ToHtml()
    {
      // Syntax: <span class="damageValue">VALUE</span> <span class="icon" title="Damage"></span>
      // Get type for icon and scaling.
      string categoryString;
      if (Category == DamageType.Damage)
        categoryString = "damageValue";
      else if (Category == DamageType.ConditionDamage)
        categoryString = "conditionValue";
      else if (Category == DamageType.Agony)
        categoryString = "agonyValue";
      else if (Category == DamageType.MawAgony)
        categoryString = "mawValue";
      else if (Category == DamageType.ConditionEffect)
        categoryString = "buffValue";
      else if (Category == DamageType.CrowdControl)
        categoryString = "crowdValue";
      else if (Category == DamageType.Constant)
        categoryString = "constantValue";
      else if (Category == DamageType.Percent)
        categoryString = "percentValue";
      else if (Category == DamageType.Buff)
      {
        // Custom buffs don't get any icon so no need to care about that.
        var buffBuilder = new StringBuilder();
        buffBuilder.Append(Type);
        HandleStacksAndDuration(buffBuilder);
        return buffBuilder.ToString();
      }
      else
      {
        // Normal text doesn't need any tweak.
        return Helper.ConvertSpecial(Additional);
      }
      var toReturn = new StringBuilder();
      // Add amount (damag or duration).
      int useless;
      if (int.TryParse(Amount, out useless))
      {
        // Add scaling for numbers.
        if (!Amount.Equals("0"))
          toReturn.Append("<span class=\"").Append(categoryString).Append("\">").Append(Amount).Append("</span>");
      }
      else
      {
        toReturn.Append(Helper.ConvertSpecial(Helper.ToUpper(Amount)));
      }
      // Add icon.
      toReturn.Append(" <span class=").Append(Constants.IconClass).Append(" data-src=\"").Append(Type.ToLower()).Append("\">").Append(Helper.ToUpper(Type)).Append("</span>");
      HandleStacksAndDuration(toReturn);
      return toReturn.ToString();
    }

    /***********************************************************************************************
     * HandleStacksAndDuration / 2014-08-01 / Wethospu                                             * 
     *                                                                                             * 
     * Helper function to convert stacks and duration to a correct string.                         *
     *                                                                                             *
     * builder: Base string to add the converted string.                                           *
     *                                                                                             *
     ***********************************************************************************************/

    private void HandleStacksAndDuration(StringBuilder builder)
    {
      if (Duration.Length == 0 && Stacks.Length == 0)
        return;
      builder.Append(" (");
      if (!Stacks.Equals(""))
      {
        int useless;
        if (int.TryParse(Stacks, out useless))
        {
          builder.Append(Stacks);
          if (Stacks.Equals("1"))
            builder.Append(" " + Constants.Stack);
          else
            builder.Append(" " + Constants.Stacks);
          if (!Duration.Equals(""))
            builder.Append(" " + Constants.For + " ");
        }
        else
        {
          ErrorHandler.ShowWarningMessage("Stack amount " + Stacks + " is not a number. Critical program error (should have been detected earlier)");
        }
      }
      if (!Duration.Equals(""))
        builder.Append(Duration).Append(Constants.Space).Append(Constants.Second);
      builder.Append(")");
    }
  }
}
