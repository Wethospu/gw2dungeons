using System;
using System.Collections.Generic;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;
using System.Linq;

namespace DataCreator.Enemies
{

  /// <summary>
  /// An object for one enemy. Contains name, type, etc. and a list of attacks.
  /// </summary>
  public class Enemy : BaseType, IComparable
  {
    /// <summary>
    /// Alternative names which can be used in links and the site search. Guaranteed to be lower case without special marks.
    /// </summary>
    public List<string> AltNames { get; private set; }
    /// <summary>
    /// Datamined enemy information.
    /// </summary>
    public EnemyAttributes Attributes = new EnemyAttributes();
    /// <summary>
    /// Tracks the validity of current tactic. This is needed to pick the best tactic when copying them from encounters.
    /// </summary>
    public double TacticValidity = 0.0;
    /// <summary>
    /// Search tags for advanced filtering in the page search.
    /// </summary>
    public SortedSet<string> Tags = new SortedSet<string>();
    /// <summary>
    /// Ids for the datamined information.
    /// A single enemy can have multiple ids because different models have different ids. We aren't that accurate.
    /// </summary>
    public SortedSet<int> InternalIds = new SortedSet<int>();
    /// <summary>
    /// Tracks whether the media files were explicitly set or copied. This allows replacing copied media information.
    /// </summary>
    public bool AreAnimationsCopied = false;
    /// <summary>
    /// In-game level of the enemy. By default path specific values is used automatically. This allows explicitly setting it.
    /// </summary>
    public int Level = 0;

    public readonly List<Attack> Attacks = new List<Attack>();
    /// <summary>
    /// Fractal scaling type. Differeny enemy ranks have different coefficients but there are exceptions.
    /// </summary>
    public string ScalingType = "";

    public Enemy()
    {
      AltNames = new List<string>();
    }

    /// <summary>
    /// Adds one alternative name. Ensures the correct format. 
    /// </summary>
    public void AddAlternativeName(string altName)
    {
      // Javascript can't handle special characters.
      // Alt names only used there so can be safely simplified this early.
      AltNames.Add(Helper.Simplify(altName));
    }

    public int CompareTo(object obj)
    {
      var toCompare = (Enemy)(obj);
      if (toCompare == null)
      {
        ErrorHandler.ShowWarningMessage("Unrecognized stuff when sorting enemies. Something is seriously wrong!");
        return 0;
      }
      var id1 = InternalIds.Count > 0 ? InternalIds.First() : 0;
      var id2 = toCompare.InternalIds.Count > 0 ? toCompare.InternalIds.First() : 0;
      if (id1 != id2)
        return id1 - id2;
      var result = string.Compare(Name, toCompare.Name);
      if (result != 0)
        return result;
      result = RankToInt(Attributes.Rank) - RankToInt(toCompare.Attributes.Rank);
      if (result != 0)
        return result;
      result = Level - toCompare.Level;
      if (result == 0)
        result = string.Compare(string.Join("|", Paths), string.Join("|", toCompare.Paths));
      if (result == 0)
        ErrorHandler.ShowWarningMessage("Couldn't figure out order for enemies " + Name + " and " + toCompare.Name + ". Either program failed or there are duplicates.");
      return result;
    }

    /// <summary>
    /// Helper function to make rank comparable.
    /// </summary>
    private static int RankToInt(string rank)
    {
      if (rank.ToLower().Equals("normal"))
        return 0;
      if (rank.ToLower().Equals("veteran"))
        return 1;
      if (rank.ToLower().Equals("elite"))
        return 3;
      if (rank.ToLower().Equals("champion"))
        return 4;
      if (rank.ToLower().Equals("legendary"))
        return 5;
      return 6;
    }

    /// <summary>
    /// Replaces enemy and other link tags with html.
    /// </summary>
    public override void CreateLinks(List<string> paths, List<Enemy> enemies)
    {
      base.CreateLinks(paths, enemies);
      foreach (var attack in Attacks)
        attack.CreateLinks(paths, enemies);
    }

    /// <summary>
    /// Returns HTML representation.
    /// </summary>
    // TODO: Enemies are needed to generate enemy links. Separate this to own function (more code but a clearer structure).
    public string ToHtml()
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<div class=\"enemy\"");
      htmlBuilder.Append(AddIdentifiers());
      htmlBuilder.Append(">").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("<table>").Append(Constants.LineEnding);

      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<tr><td class=\"enemy-info\">").Append(Constants.LineEnding);
      htmlBuilder.Append(AddInformation(3));
      if (Tactics.Count > 0 && Tactics.Tactics[0].Lines.Count > 0)
        htmlBuilder.Append(Tactics.ToHtml(Index, 3));
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("</td>").Append(Constants.LineEnding);

      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<td class=\"enemy-media\">").Append(Constants.LineEnding);
      foreach (var media in Medias)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(4)).Append(media.GetThumbnailHTML()).Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("<br>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("</td></tr>").Append(Constants.LineEnding);

      htmlBuilder.Append(AddAttacks(2));
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("</table>").Append(Constants.LineEnding);
      htmlBuilder.Append("</div>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }

    /// <summary>
    /// Returns HTML for data-* values. These are used for the website's search and for automatic scaling.
    /// </summary>
    // TODO: Check that all values are really needed.
    private StringBuilder AddIdentifiers()
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(" data-name=\"").Append(Helper.Simplify(Name));
      foreach (var altName in AltNames)
        htmlBuilder.Append(" ").Append(Helper.Simplify(altName));
      htmlBuilder.Append("\"");
      htmlBuilder.Append(" data-rank=\"").Append(Helper.Simplify(Attributes.Rank)).Append("\"");
      htmlBuilder.Append(" data-path=\"").Append(Helper.Simplify(string.Join("|", Paths))).Append("\"");
      // Only add non-default path info to allow scaling it on the website.
      if (Level > 0)
        htmlBuilder.Append(" data-level=\"").Append(Level).Append("\"");
      if (Attributes.Family.GetInternal().Length > 0)
        htmlBuilder.Append(" data-race=\"").Append(Attributes.Family.GetInternal()).Append("\"");
      htmlBuilder.Append(Attributes.ToHtml());

      if (ScalingType.Length > 0)
        htmlBuilder.Append(" data-scaling=\"").Append(Gw2Helper.ScalingTypeToMode(ScalingType)).Append("\"");
      return htmlBuilder;
    }

    /// <summary>
    /// Returns HTML for the enemy information like name, rank, race, level, health and armor.
    /// </summary>
    private StringBuilder AddInformation(int baseIndent)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<div class=\"in-line\">").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<span class=\"enemy-name\">").Append(Helper.ConvertSpecial(Helper.ToUpperAll(Name.Replace(" ", Constants.Space)))).Append("</span>").Append(Constants.LineEnding);
      if (!Attributes.Rank.Equals(""))
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<span class=\"rank-unit\">").Append(Helper.ToUpper(Helper.ConvertSpecial(Attributes.Rank)).Replace(" ", Constants.Space)).Append(Constants.Space).Append("</span>").Append(Constants.LineEnding);
      var family = Attributes.Family.GetDisplay();
      if (!family.Equals(""))
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<span class=\"race-unit\">").Append(family).Append(" </span>").Append(Constants.LineEnding);
      // Enemy can have multiple sexes if there are model variations.
      var genders = Attributes.Gender.Split('|');
      for (int i = 0; i < genders.Length; i++)
      {
        if (genders[i].Equals("none") || genders[i].Equals(""))
          continue;
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<span class=\"gender-unit\"><span class=").Append(Constants.IconClass).Append("data-src=\"").Append(genders[i]).Append("\">").Append(Helper.ToUpper(genders[i])).Append("</span></span>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<br>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<span class=\"level-unit\" title=\"Hold ctrl to change 10 levels\">Level").Append(Constants.Space);
      // Numerical values are dynamically calculated based on the enemy level, fractal scale and player level on the website.
      // So also add UI to change them so users can play with them too!
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-left level-minus\"></span><span class=\"level\"></span>");
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-right level-plus\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"fractal-level-unit\" title=\"Hold ctrl to change 10 levels\">Fractal level").Append(Constants.Space);
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-left fractal-level-minus\"></span><span class=\"fractal-level\"></span>");
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-right fractal-level-plus\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"target-level-unit\" title=\"Hold ctrl to change 10 levels\">Player level").Append(Constants.Space);
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-left target-level-minus\"></span><span class=\"target-level\"></span>");
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-right target-level-plus\"></span> </span>").Append(Constants.LineEnding);
      // Some enemies exist in paths with different base levels. Allow changing the enemy and player level by clicking on the path information.
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"current-path-unit\">");
      foreach (var path in Paths)
        htmlBuilder.Append("<span class=\"path-button\">").Append(Helper.ToUpper(path)).Append("</span> ");
      htmlBuilder.Append(" </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(AddAttributes(baseIndent + 1));
      // Add information about the scaling mode so users can see what happens "under the hood".
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent + 1)).Append("<span class=\"scaling-unit\" title=\"Internal scaling mode\">Scaling:").Append(Constants.Space).Append(Gw2Helper.ScalingTypeToString(ScalingType)).Append(" </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("</div>").Append(Constants.LineEnding);
      return htmlBuilder;
    }

    /// <summary>
    /// Returns HTML for dynamically scaling attributes. For example power, health, weapon strength and so on.
    /// </summary>
    private StringBuilder AddAttributes(int baseIndent)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<br>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"health-unit\" title=\"Health\"><span class=").Append(Constants.IconClass).Append(" data-src=\"health\">Health</span>:").Append(Constants.Space).Append("<span class=\"health\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"armor-unit\" title=\"Armor\"><span class=").Append(Constants.IconClass).Append(" data-src=\"armor\">Armor</span>:").Append(Constants.Space).Append("<span class=\"armor\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"offense-unit\" title=\"Relative offensive power\"><span class=").Append(Constants.IconClass).Append(" data-src=\"offense\">Offense</span>:").Append(Constants.Space).Append("<span class=\"offense\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"weapon-unit\" title=\"Main weapon strength\"><span class=").Append(Constants.IconClass).Append(" data-src=\"weapon\">Weapon strength</span>:").Append(Constants.Space).Append("<span class=\"weapon\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"power-unit\" title=\"Power\"><span class=").Append(Constants.IconClass).Append(" data-src=\"power\">Power</span>:").Append(Constants.Space).Append("<span class=\"power\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"precision-unit\" title=\"Critical chance\"><span class=").Append(Constants.IconClass).Append(" data-src=\"precision\">Critical chance</span>:").Append(Constants.Space).Append("<span class=\"precision\"></span>% </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"ferocity-unit\" title=\"Critical damage\"><span class=").Append(Constants.IconClass).Append(" data-src=\"ferocity\">Critical damage</span>:").Append(Constants.Space).Append("<span class=\"ferocity\"></span>%  </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"condition-unit\" title=\"Condition damage\"><span class=").Append(Constants.IconClass).Append(" data-src=\"condition\">Condition damage</span>:").Append(Constants.Space).Append("<span class=\"condition\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"healing-unit\" title=\"Healing power\"><span class=").Append(Constants.IconClass).Append(" data-src=\"healing_power\">Healing power</span>:").Append(Constants.Space).Append("<span class=\"healing-power\"></span> </span>").Append(Constants.LineEnding);
      if (Attributes.Size > 0)
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<span class=\"size-unit\" title=\"Relative size\"><span class=").Append(Constants.IconClass).Append(" data-src=\"size\">Size</span>:").Append(Constants.Space).Append(Math.Round(Attributes.Size * 100)).Append("%</span>").Append(Constants.LineEnding);
      return htmlBuilder;
    }

    /// <summary>
    /// Returns HTML representation for attacks.
    /// </summary>
    private StringBuilder AddAttacks(int baseIndent)
    {
      var htmlBuilder = new StringBuilder();
      // First gather the information because the table structure depends on the content.
      var attackHTMLs = new List<string>();
      var mediaHTMLs = new List<string>();
      foreach (var attack in Attacks)
      {
        attackHTMLs.Add(attack.ToHTML(this, 2));
        mediaHTMLs.Add(attack.MediaToHTML(2));
      }
      for (var i = 0; i < attackHTMLs.Count; i++)
      {
        var j = i;
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<tr><td class=\"enemy-info\">").Append(Constants.LineEnding);
        htmlBuilder.Append(attackHTMLs[i]);
        // Merge attacks without media to the previous attack. This saves space and makes sense especially for attack chains.
        while (i + 1 < mediaHTMLs.Count && mediaHTMLs[i + 1].Equals(""))
        {
          i++;
          htmlBuilder.Append(attackHTMLs[i]);
        }
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("</td>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<td class=\"enemy-media\">").Append(Constants.LineEnding);
        htmlBuilder.Append(mediaHTMLs[j]);
        htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("</td></tr>").Append(Constants.LineEnding);
      }
      return htmlBuilder;
    }
  }
}
