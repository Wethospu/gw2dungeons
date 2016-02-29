using System;
using System.Collections.Generic;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;
using System.Linq;

namespace DataCreator.Enemies
{

  /***********************************************************************************************
   * Enemy / 2014-08-01 / Wethospu                                                               *
   *                                                                                             *
   * Object for one enemy. Contains its name, type, etc. and list of attacks.                    *
   *                                                                                             *
   ***********************************************************************************************/

  public class Enemy : IComparable
  {
    public string Name = "";
    public int Index = 0;
    public string Rank = "";
    // Alternative names for links and search. Guaranteed to be lower case without special marks.
    public List<string> AltNames { get; private set; }
    public List<string> Paths = new List<string>();
    public EnemyAttributes Attributes = new EnemyAttributes();
    // This tracks how valid current tactics are. It's needed to pick the best tactics when copying them from encounters. / 2015-08-09 / Wethospu
    public double TacticValidity = 0.0;
    public TacticList Tactics = new TacticList();
    public List<Media> Medias = new List<Media>();
    public SortedSet<string> Tags = new SortedSet<string>();
    public SortedSet<int> InternalIds = new SortedSet<int>();
    public bool IsNameCopied = false;
    public bool AreAnimationsCopied = false;
    public int Level = 0;

    public readonly List<Attack> Attacks = new List<Attack>();
    // Scaling variables.
    public string ScalingType = "";
    // Temporary helper variables to get values to zero-point.
    public int ScalingLevel = 80;
    public int ScalingFractal = 1;

    private Enemy()
    {
      AltNames = new List<string>();
    }

    public Enemy(string name)
      : this()
    {
      Name = name;
    }

    /***********************************************************************************************
     * AddAlt / 2014-08-01 / Wethospu                                                              * 
     *                                                                                             * 
     * Adds one alternative name to alternative name list. Converts to correct format.             *
     *                                                                                             *
     * altName: Name to add.                                                                       *
     *                                                                                             *
     ***********************************************************************************************/

    public void AddAlt(string altName)
    {
      // Javascript can't handle special characters.
      // Alt names only used there so can be safely simplified this early.
      AltNames.Add(Helper.Simplify(altName));
    }

    /***********************************************************************************************
     * CompareTo / 2014-08-01 / Wethospu                                                           * 
     *                                                                                             * 
     * Makes enemies comparable. Needed to sort enemy list.                                        *
     *                                                                                             *
     * obj: Object to compare with.                                                                *
     *                                                                                             *
     ***********************************************************************************************/

    public int CompareTo(object obj)
    {
      var toCompare = (Enemy)(obj);
      if (toCompare == null)
      {
        ErrorHandler.ShowWarningMessage("Unrecognized stuff when sorting enemies. Something is seriously wrong!");
        return 0;
      }
      // Compare ids.
      var id1 = InternalIds.Count > 0 ? InternalIds.First() : 0;
      var id2 = toCompare.InternalIds.Count > 0 ? toCompare.InternalIds.First() : 0;
      if (id1 != id2)
        return id1 - id2;
      // Compare name.
      var result = string.Compare(Name, toCompare.Name);
      if (result != 0)
        return result;
      // If same, compare rank.
      result = RankToInt(Rank) - RankToInt(toCompare.Rank);
      if (result != 0)
        return result;
      // If same, compare level.
      result = Level - toCompare.Level;
      if (result == 0)
        // If same, compare path.
        result = string.Compare(string.Join("|", Paths), string.Join("|", toCompare.Paths));
      if (result == 0)
        ErrorHandler.ShowWarningMessage("Couldn't figure out order for enemies " + Name + " and " + toCompare.Name + ". Either program failed or there are duplicates.");
      return result;
    }

    /***********************************************************************************************
     * RankToInt / 2014-08-01 / Wethospu                                                           * 
     *                                                                                             * 
     * Helper function to make category properly comparable.                                       *
     *                                                                                             *
     ***********************************************************************************************/

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
      return 0;
    }

    /***********************************************************************************************
     * ToHtml / 2014-08-01 / Wethospu                                                              * 
     *                                                                                             * 
     * Converts this enemy object to html representration.                                         *
     *                                                                                             *
     * Returns representation.                                                                     *
     * enemies: List of enemies in the path. Needed for enemy links.                               *
     *                                                                                             *
     ***********************************************************************************************/

    public string ToHtml(List<Enemy> enemies)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<div class=\"enemy\"");
      //// Add identifier data (name, rank, path, race, potion and level).
      htmlBuilder.Append(" data-name=\"").Append(Helper.Simplify(Name));
      foreach (var altName in AltNames)
        htmlBuilder.Append(" ").Append(Helper.Simplify(altName));
      htmlBuilder.Append("\"");
      htmlBuilder.Append(" data-rank=\"").Append(Helper.Simplify(Rank)).Append("\"");
      htmlBuilder.Append(" data-path=\"").Append(Helper.Simplify(string.Join("|", Paths))).Append("\"");
      // Only add non-default path info. / 2015-09-39 / Wethospu
      if (Level > 0)
        htmlBuilder.Append(" data-level=\"").Append(Level).Append("\"");
      if (Attributes.Family.GetInternal().Length > 0)
        htmlBuilder.Append(" data-race=\"").Append(Attributes.Family.GetInternal()).Append("\"");
      htmlBuilder.Append(Attributes.ToHtml());
      
      if (ScalingType.Length > 0)
        htmlBuilder.Append(" data-scaling=\"").Append(Gw2Helper.ScalingTypeToMode(ScalingType)).Append("\"");
      htmlBuilder.Append(">").Append(Constants.LineEnding);
      //// Identifiers added.
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("<table>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<tr><td class=\"enemy-info\">").Append(Constants.LineEnding);
      // Add name.
      htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("<div class=\"in-line\">").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"enemy-name\">").Append(Helper.ConvertSpecial(Helper.ToUpperAll(Name.Replace(" ", Constants.Space)))).Append("</span>").Append(Constants.LineEnding);
      // Add details like rank, race, level, health and armor. / 2015-08-09 / Wethospu   
      if (!Rank.Equals(""))
        htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"rank-unit\">").Append(Helper.ToUpper(Helper.ConvertSpecial(Rank)).Replace(" ", Constants.Space)).Append(Constants.Space).Append("</span>").Append(Constants.LineEnding);
      var family = Attributes.Family.GetDisplay();
      if (!family.Equals(""))
        htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"race-unit\">").Append(family).Append(" </span>").Append(Constants.LineEnding);
      // Enemy can have multiple sexes if there are model variations. / 2015-09-28 / Wethospu
      var genders = Attributes.Gender.Split('|');
      for (int i = 0; i < genders.Length; i++)
      {
        if (genders[i].Equals("none") || genders[i].Equals(""))
          continue;
        htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"gender-unit\"><span class=").Append(Constants.IconClass).Append("data-src=\"").Append(genders[i]).Append("\">").Append(Helper.ToUpper(genders[i])).Append("</span></span>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<br>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"level-unit\" title=\"Hold ctrl to change 10 levels\">Level").Append(Constants.Space);
      // Level can be changed dynamically. / 2015-09-27 / Wethospu
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-left level-minus\"></span><span class=\"level\"></span>");
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-right level-plus\"></span> </span>").Append(Constants.LineEnding);
      // Add a dynamic target level for dungeons and a dynamic fractal level for fractals. / 2015-09-39 / Wethospu
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"fractal-level-unit\" title=\"Hold ctrl to change 10 levels\">Fractal level").Append(Constants.Space);
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-left fractal-level-minus\"></span><span class=\"fractal-level\"></span>");
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-right fractal-level-plus\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"target-level-unit\" title=\"Hold ctrl to change 10 levels\">Player level").Append(Constants.Space);
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-left target-level-minus\"></span><span class=\"target-level\"></span>");
      htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-right target-level-plus\"></span> </span>").Append(Constants.LineEnding);
      // Add a link for every path to dynamically change the current path (and base level). / 2015-09-27 / Wethospu
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"current-path-unit\">");
      foreach (var path in Paths)
        htmlBuilder.Append("<span class=\"path-button\">").Append(Helper.ToUpper(path)).Append("</span> ");
      htmlBuilder.Append(" </span>").Append(Constants.LineEnding);

      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<br>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"health-unit\" title=\"Health\"><span class=").Append(Constants.IconClass).Append(" data-src=\"health\">Health</span>:").Append(Constants.Space).Append("<span class=\"health\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"armor-unit\" title=\"Armor\"><span class=").Append(Constants.IconClass).Append(" data-src=\"armor\">Armor</span>:").Append(Constants.Space).Append("<span class=\"armor\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"offense-unit\" title=\"Relative offensive power\"><span class=").Append(Constants.IconClass).Append(" data-src=\"offense\">Offense</span>:").Append(Constants.Space).Append("<span class=\"offense\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"weapon-unit\" title=\"Main weapon strength\"><span class=").Append(Constants.IconClass).Append(" data-src=\"weapon\">Weapon strength</span>:").Append(Constants.Space).Append("<span class=\"weapon\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"power-unit\" title=\"Power\"><span class=").Append(Constants.IconClass).Append(" data-src=\"power\">Power</span>:").Append(Constants.Space).Append("<span class=\"power\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"precision-unit\" title=\"Critical chance\"><span class=").Append(Constants.IconClass).Append(" data-src=\"precision\">Critical chance</span>:").Append(Constants.Space).Append("<span class=\"precision\"></span>% </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"ferocity-unit\" title=\"Critical damage\"><span class=").Append(Constants.IconClass).Append(" data-src=\"ferocity\">Critical damage</span>:").Append(Constants.Space).Append("<span class=\"ferocity\"></span>%  </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"condition-unit\" title=\"Condition damage\"><span class=").Append(Constants.IconClass).Append(" data-src=\"condition\">Condition damage</span>:").Append(Constants.Space).Append("<span class=\"condition\"></span> </span>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"healing-unit\" title=\"Healing power\"><span class=").Append(Constants.IconClass).Append(" data-src=\"healing_power\">Healing power</span>:").Append(Constants.Space).Append("<span class=\"healing-power\"></span> </span>").Append(Constants.LineEnding);
      if (Attributes.Size > 0)
        htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"size-unit\" title=\"Relative size\"><span class=").Append(Constants.IconClass).Append(" data-src=\"size\">Size</span>:").Append(Constants.Space).Append(Math.Round(Attributes.Size * 100)).Append("%</span>").Append(Constants.LineEnding);
      // Add information about scaling mode so people understand when things shouldn't scale. / 2015-10-10 / Wethospu
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"scaling-unit\" title=\"Internal scaling mode\">Scaling:").Append(Constants.Space).Append(Gw2Helper.ScalingTypeToString(ScalingType)).Append(" </span>").Append(Constants.LineEnding);

      htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("</div>").Append(Constants.LineEnding);
      // Add tactics. / 2015-08-09 / Wethospu
      htmlBuilder.Append(Tactics.ToHtml(Index, string.Join("|", Paths), 0, enemies, 3));
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("</td>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<td class=\"enemy-media\">").Append(Constants.LineEnding);
      // Add thumbs.
      foreach (var media in Medias)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(4)).Append(media.GetThumbnailHTML()).Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("<br>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("</td></tr>").Append(Constants.LineEnding);
      //// Add attacks.
      // First gather the information because the table structure depends on the content. / 2015-09-15 / Wethospu
      var attackHTMLs = new List<string>();
      var mediaHTMLs = new List<string>();
      foreach (var attack in Attacks)
      {
        attackHTMLs.Add(attack.ToHTML(string.Join("|", Paths), enemies, this, 2));
        mediaHTMLs.Add(attack.MediaToHTML(2));
      }
      // Generate rows. / 2015-09-15 / Wethospu
      for (var i = 0; i < attackHTMLs.Count; i++)
      {
        var j = i;
        htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<tr><td class=\"enemy-info\">").Append(Constants.LineEnding);
        htmlBuilder.Append(attackHTMLs[i]);
        // Merge attacks without media to the previous attack. This prevents some empty space and makes sense especially for attack chains. / 2015-09-15 / Wethospu
        while (i + 1 < mediaHTMLs.Count && mediaHTMLs[i + 1].Equals(""))
        {
          i++;
          htmlBuilder.Append(attackHTMLs[i]);
        }
        htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("</td>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<td class=\"enemy-media\">").Append(Constants.LineEnding);
        htmlBuilder.Append(mediaHTMLs[j]);
        htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("</td></tr>").Append(Constants.LineEnding);
      }

      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("</table>").Append(Constants.LineEnding);
      htmlBuilder.Append("</div>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }
  }
}
