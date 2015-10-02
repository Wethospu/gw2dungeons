using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;

namespace DataCreator.Enemies
{
  /***********************************************************************************************
   * Attack / 2014-08-01 / Wethospu                                                              * 
   *                                                                                             * 
   * Object for one enemy attack. Contains cooldown, effects, damage, animation, etc.            *
   *                                                                                             *
   ***********************************************************************************************/

  public class Attack
  {
    private readonly string _name;
    public readonly List<Effect> Effects = new List<Effect>();
    private string _cooldown = "";
    private int _minimumRange = -1;
    private int _maximumRange = -1;
    private double _coefficient = 0;
    public List<Media> Medias = new List<Media>();
    public string Cooldown
    {
      private get
      {
        return _cooldown;
      }
      set
      {
        // Gets called with empty value wben copying enemies with no cooldown.
        if (value.Length == 0)
        {
          _cooldown = value;
          return;
        }
        // Verify that cooldown is a number.
        _cooldown = "" + Helper.ParseD(value);
      }
    }
    private string _animation = "";
    public string Animation
    {
      private get
      {
        return _animation;
      }
      set
      {
        // Get elements from the data.
        var dataSplit = value.Split(Constants.Delimiter);
        var preCast = dataSplit[0];
        var castDuration = "";
        if (dataSplit.Length > 1)
          castDuration = dataSplit[1];
        var afterCast = "";
        if (dataSplit.Length > 2)
          afterCast = dataSplit[2];
        // If any data exist, cast animation should also exist.
        if (value.Length > 0 && preCast.Length == 0)
          Helper.ShowWarning("Animation " + value + " is missing casting animation. Please fix!");
        //// Build animation string.
        var builder = new StringBuilder();
        builder.Append(preCast);
        // Construct cast duration if needed: " (" + duration + " s)".
        if (!castDuration.Equals(""))
          builder.Append(Constants.Space).Append("(").Append(castDuration).Append(Constants.Space).Append(Constants.Second).Append(")");
        // Add after cast if exists.
        if (!afterCast.Equals(""))
          builder.Append(", ").Append(afterCast);
        _animation = builder.ToString();
      }
    }

    public Attack(string name, EnemyAttributes attributes)
    {
      _name = name;
      // Attempt to find values from the database. / 2015-10-2 / Wethospu
      if (attributes == null || attributes.Weapons == null || attributes.Weapons.Main == null)
        return;
      foreach (var skill in attributes.Weapons.Main.Skills)
      {
        if (skill.Name.Equals(name))
        {
          _minimumRange = skill.MinimumRange;
          _maximumRange = skill.MaxRange;
          if (skill.Tags != null)
            _coefficient = skill.Tags.Coefficient;
        }
      }
    }

    public Attack(Attack toCopy)
    {
      _name = string.Copy(toCopy._name);
      Cooldown = string.Copy(toCopy.Cooldown);
      Animation = string.Copy(toCopy.Animation);
      foreach (var media in toCopy.Medias)
        Medias.Add(new Media(media));
      foreach (var effect in toCopy.Effects)
        Effects.Add(new Effect(effect));
    }

    /***********************************************************************************************
    * AttackToHTML / 2015-09-15 / Wethospu                                                         *
    *                                                                                              *
    * Converts this attack object to html representration.                                         *
    *                                                                                              *
    * Returns representation.                                                                      *
    * path: Name of current path. Needed for enemy links.                                          *
    * enemies: List of enemies in the path. Needed for enemy links.                                *
    * indent: Base indent for the HTML.                                                            *
    *                                                                                              *
    ***********************************************************************************************/

    public string AttackToHTML(string path, List<Enemy> enemies, Enemy baseEnemy, int indent)
    {
      var htmlBuilder = new StringBuilder();
      // Add attack name.
      htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<p class=\"enemy-attack\"><span class=\"enemy-attack-name\">").Append(Helper.ConvertSpecial(Helper.ToUpperAll(LinkGenerator.CreateEnemyLinks(_name, path, enemies))));

      htmlBuilder.Append("</span>").Append(" ");
      // Add other data.
      if (!Animation.Equals(""))
        htmlBuilder.Append("<i>").Append(Helper.ConvertSpecial(Helper.ToUpper(LinkGenerator.CreateEnemyLinks(Animation, path, enemies)))).Append("</i>. ");
      if (!Cooldown.Equals(""))
        htmlBuilder.Append(Cooldown).Append(Constants.Space).Append("<span class=").Append(Constants.IconClass).Append(" title=\"cooldown\">CD</span> ");
      if (_minimumRange > -1 || _maximumRange > -1)
      {
        htmlBuilder.Append(_minimumRange > -1 ? "" + _minimumRange : "?").Append("-").Append(_maximumRange > -1 ? "" + _maximumRange : "?");
        htmlBuilder.Append(Constants.Space).Append("<span class=").Append(Constants.IconClass).Append(" title=\"range\">range</span> ");
      }
      htmlBuilder.Append("</p>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<div class=\"enemy-attack-effect\">").Append(Constants.LineEnding);
      // Add attack effects.
      foreach (var effect in Effects)
        htmlBuilder.Append(effect.ToHtml(path, enemies, baseEnemy, indent + 2));
      htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("</div>").Append(Constants.LineEnding);
      
      return htmlBuilder.ToString();
    }

    /***********************************************************************************************
    * MediaToHTML / 2015-09-15 / Wethospu                                                          *
    *                                                                                              *
    * Converts this attacks media object to html representration.                                  *
    *                                                                                              *
    * Returns representation.                                                                      *
    * indent: Base indent for the HTML.                                                            *
    *                                                                                              *
    ***********************************************************************************************/

    public string MediaToHTML(int indent)
    {
      var htmlBuilder = new StringBuilder();
      foreach (var media in Medias)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 2)).Append(media.ToHtml()).Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<br>").Append(Constants.LineEnding);
      }
      return htmlBuilder.ToString();
    }
  }
}
