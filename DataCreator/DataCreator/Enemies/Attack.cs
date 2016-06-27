using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;

namespace DataCreator.Enemies
{
  /// <summary>
  /// An object for a single enemy attack. Contains cooldown, effects, damage, animation, etc. 
  /// </summary>
  public class Attack
  {
    /// <summary>
    /// Name for the attack. This is often found in-game but not always.
    /// </summary>
    public string Name;
    /// <summary>
    /// List of effects which are caused by this attack.
    /// </summary>
    public List<Effect> Effects = new List<Effect>();
    /// <summary>
    /// Whether the attack can't be blocked. Used to detect "Can't reflect" tags.
    /// </summary>
    public bool CantBeBlocked = false;
    /// <summary>
    /// Datamined value. Minimum distance needed for the enemy to use this attack.
    /// </summary>
    public double MinimumRange = -1;
    /// <summary>
    /// Datamined value. Maximum distance needed for the enemy to use this attack.
    /// </summary>
    public double MaximumRange = -1;
    /// <summary>
    /// Datamined value. Relative strength of the attack (total damage includes other factors).
    /// </summary>
    public double Coefficient = 0;
    /// <summary>
    /// Datamined value. How often the attack can be used. Doesn't include cast times or other factors which delay the frequency.
    /// </summary>
    public double InternalCooldown = -1;
    /// <summary>
    /// Datamined value. Minimum enemy level required for this attack. Unsure if this has any effect.
    /// </summary>
    public int RequiredLevel = -1;
    /// <summary>
    /// Datamined value. Weaponset which contains this attack. Valid values are "main, "off" and "water". Affects skill damage.
    /// </summary>
    public string Weapon = "";
    /// <summary>
    /// Media files for animations for this attack.
    /// </summary>
    public List<Media> Medias = new List<Media>();
    /// <summary>
    /// Manually acquired real cooldown value.
    /// </summary>
    public double Cooldown = -1;
    /// <summary>
    /// Explanation of the attack animation including cast times.
    /// </summary>
    private string _animation = "";
    /// <summary>
    /// Explanation of the attack animation including cast times.
    /// </summary>
    public string Animation
    {
      private get
      {
        return _animation;
      }
      set
      {
        // Animation is given as a raw data so convert it to real text.
        // "Animation" ("duration" s) "After animation".
        var dataSplit = value.Split(Constants.Delimiter);
        var preCast = dataSplit[0];
        var castDuration = "";
        if (dataSplit.Length > 1)
          castDuration = dataSplit[1];
        var afterCast = "";
        if (dataSplit.Length > 2)
          afterCast = dataSplit[2];
        if (value.Length > 0 && preCast.Length == 0)
          ErrorHandler.ShowWarning("Animation " + value + " is missing casting animation. Please fix!");
        var builder = new StringBuilder();
        builder.Append(preCast);
        if (!castDuration.Equals(""))
          builder.Append(Constants.Space).Append("(").Append(castDuration).Append(Constants.Space).Append(Constants.Second).Append(")");
        if (!afterCast.Equals(""))
          builder.Append(", ").Append(afterCast);
        _animation = builder.ToString();
      }
    }

    public Attack()
    {
    }

    public Attack(string name)
    {
      Name = name;
    }

    /// <summary>
    /// Loads skill attributes from datamined data.
    /// </summary>
    // Id has been manually deduced from the datamined data. It's used to connect the attack to datamined information.
    public void LoadAttributes(int id, EnemyAttributes attributes)
    {
      if (attributes == null || attributes.Weapons == null)
        return;
      if (attributes.Weapons.Main != null && attributes.Weapons.Main.Skills != null)
      {
        foreach (var skill in attributes.Weapons.Main.Skills)
        {
          if (skill.id == id)
          {
            LoadFromSkillPalette(skill);
            Weapon = "main";
          }
        }
      }
      if (attributes.Weapons.Offhand != null && attributes.Weapons.Offhand.Skills != null)
      {
        foreach (var skill in attributes.Weapons.Offhand.Skills)
        {
          if (skill.id == id)
          {
            LoadFromSkillPalette(skill);
            Weapon = "off";
          }
        }
      }
      if (attributes.Weapons.Underwater != null && attributes.Weapons.Underwater.Skills != null)
      {
        foreach (var skill in attributes.Weapons.Underwater.Skills)
        {
          if (skill.id == id)
          {
            LoadFromSkillPalette(skill);
            Weapon = "water";
          }
        }
      }
    }

    /// <summary>
    /// Loads information from a datamined skill.
    /// </summary>
    private void LoadFromSkillPalette(SkillPalette skill)
    {
      MinimumRange = skill.MinimumRange;
      MaximumRange = skill.MaxRange;
      RequiredLevel = skill.LevelRequirement;
      InternalCooldown = skill.Cooldown;
      if (skill.Tags != null)
        Coefficient = skill.Tags.Coefficient;
    }

    /// <summary>
    /// Replaces enemy and other link tags with html.
    /// </summary>
    public void CreateLinks(List<string> paths, List<Enemy> enemies)
    {
      foreach (var effect in Effects)
        effect.CreateLinks(paths, enemies);
    }

    /// <summary>
    /// Generates the HTML representation.
    /// </summary>
    public string ToHTML(Enemy attackOwner, int baseIndent)
    {
      if (Name.Equals(""))
        ErrorHandler.ShowWarningMessage("Enemy " + attackOwner.Name + " has no attack name.");
      var htmlBuilder = new StringBuilder();
      // Add attack name.
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<p class=\"enemy-attack\"><span class=\"enemy-attack-name\">").Append(Helper.ConvertSpecial(Helper.ToUpperAll(Name)));

      htmlBuilder.Append("</span>").Append(" ");
      // Add other data.
      if (!Animation.Equals(""))
        htmlBuilder.Append("<span class=\"animation-unit\"><i>").Append(Helper.ConvertSpecial(Helper.ToUpper(Animation))).Append("</i>. </span>");
      if (Cooldown > -1 || InternalCooldown > -1)
      {
        htmlBuilder.Append("<span class=\"cooldown-unit\" title=\"Skill cooldown\"><span class=\"cooldown\"");
        if (Cooldown > -1)
          htmlBuilder.Append(" data-amount=\"").Append(Cooldown).Append("\"");
        if (InternalCooldown > -1)
          htmlBuilder.Append(" data-internal=\"").Append(InternalCooldown / 1000).Append("\"");
        htmlBuilder.Append("></span>").Append(Constants.Space).Append("<span class=").Append(Constants.IconClass).Append(" data-src=\"cooldown\">Cooldown</span> </span>");
      }
      if (MinimumRange > -1 || MaximumRange > -1)
      {
        htmlBuilder.Append("<span class=\"range-unit\" title=\"Activation range\">");
        htmlBuilder.Append(MinimumRange > -1 ? "" + MinimumRange : "?").Append("-").Append(MaximumRange > -1 ? "" + MaximumRange : "?");
        htmlBuilder.Append(Constants.Space).Append("<span class=").Append(Constants.IconClass).Append(" data-src=\"range\">Range</span> </span>");
      }
      htmlBuilder.Append("</p>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<div class=\"enemy-attack-effect\">").Append(Constants.LineEnding);
      // Add effects.
      foreach (var effect in Effects)
        htmlBuilder.Append(effect.ToHtml(this, attackOwner, baseIndent + 2));
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("</div>").Append(Constants.LineEnding);
      
      return htmlBuilder.ToString();
    }

    /// <summary>
    /// Generates the HTML representation for media files.
    /// </summary>
    public string MediaToHTML(int baseIndent)
    {
      var htmlBuilder = new StringBuilder();
      foreach (var media in Medias)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 2)).Append(media.GetThumbnailHTML()).Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<br>").Append(Constants.LineEnding);
      }
      return htmlBuilder.ToString();
    }
  }
}
