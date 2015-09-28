using System;
using System.Collections.Generic;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;

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
    // Order number of this enemy in the enemy file. Needed for faster/simpler enemy linking. / 2015-08-11 / Wethospu
    public int FileIndex = 0;
    public string Category = "";
    // Alternative names for links and search. Guaranteed to be lower case without special marks.
    public List<string> AltNames { get; private set; }
    public string Path = "";
    public string Potion = "";
    public EnemyAttributes Attributes = new EnemyAttributes();
    // This tracks how valid current tactics are. It's needed to pick the best tactics when copying them from encounters. / 2015-08-09 / Wethospu
    public double TacticValidity = 0.0;
    public TacticList Tactics = new TacticList();
    public List<Media> Medias = new List<Media>();
    public SortedSet<string> Tags = new SortedSet<string>();

    private int _level = 0;
    public int Level
    {
      get
      {
        // Make sure level is proper when needed (load path specific value).
        if (_level == 0 && !Category.Equals("Structure") && !Category.Equals("Trap") && !Category.Equals("Skill"))
          _level = Gw2Helper.PathToLevel(Path);
        return _level;
      }
      set
      {
        _level = value;
      }
    }
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

    public Enemy(Enemy toCopy)
      : this()
    {
      Category = string.Copy(toCopy.Category);
      Path = string.Copy(toCopy.Path);
      Potion = string.Copy(toCopy.Potion);
      Level = toCopy.Level;
      ScalingType = string.Copy(toCopy.ScalingType);
      ScalingLevel = toCopy.ScalingLevel;
      ScalingFractal = toCopy.ScalingFractal;
      foreach (var media in toCopy.Medias)
        Medias.Add(new Media(media));
      Tactics = new TacticList(toCopy.Tactics);
      foreach (var attack in toCopy.Attacks)
        Attacks.Add(new Attack(attack));
      foreach (var tag in toCopy.Tags)
        Tags.Add(tag);
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

    public int CompareTo(Object obj)
    {
      var toCompare = (Enemy)(obj);
      if (toCompare == null)
      {
        Helper.ShowWarningMessage("Unrecognized stuff when sorting enemies. Something is seriously wrong!");
        return 0;
      }
      // Compare name.
      var result = string.Compare(Name, toCompare.Name);
      if (result == 0)
      {
        // If same, compare category.
        result = CategoryToInt(Category) - CategoryToInt(toCompare.Category);
        if (result == 0)
        {
          // If same, compare level.
          result = Level - toCompare.Level;
          if (result == 0)
            // If same, compare path.
            result = string.Compare(Path, toCompare.Path);
        }
      }
      if (result == 0)
        Helper.ShowWarningMessage("Couldn't figure out order for enemies " + Name + " and " + toCompare.Name + ". Either program failed or there are duplicates.");
      return result;
    }

    /***********************************************************************************************
     * CategoryToInt / 2014-08-01 / Wethospu                                                       * 
     *                                                                                             * 
     * Helper function to make category properly comparable.                                       *
     *                                                                                             *
     ***********************************************************************************************/

    private static int CategoryToInt(string category)
    {
      if (category.ToLower().Equals("normal"))
        return 0;
      if (category.ToLower().Equals("veteran"))
        return 1;
      if (category.ToLower().Equals("elite"))
        return 3;
      if (category.ToLower().Equals("champion"))
        return 4;
      if (category.ToLower().Equals("legendary"))
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
      //// Add identifier data (name, category, path, race, potion and level).
      htmlBuilder.Append(" data-name=\"").Append(Helper.Simplify(Name));
      foreach (var altName in AltNames)
        htmlBuilder.Append(" ").Append(Helper.Simplify(altName));
      htmlBuilder.Append("\"");
      htmlBuilder.Append(" data-category=\"").Append(Helper.Simplify(Category)).Append("\"");
      htmlBuilder.Append(" data-path=\"").Append(Helper.Simplify(Path)).Append("\"");
      htmlBuilder.Append(" data-potion=\"").Append(Helper.Simplify(Potion)).Append("\"");
      htmlBuilder.Append(" data-level=\"").Append(Level).Append("\"");
      htmlBuilder.Append(Attributes.ToHtml());
      
      if (ScalingType.Length > 0)
        htmlBuilder.Append(" data-scaling=\"").Append(Scaling.ScalingTypeToMode(Helper.Simplify(ScalingType))).Append("\"");
      htmlBuilder.Append(">").Append(Constants.LineEnding);
      //// Identifiers added.
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("<table>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<tr><td class=\"enemy-info\">").Append(Constants.LineEnding);
      // Add name.
      htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("<div class=\"in-line\">").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(4)).Append("<span class=\"enemy-name\">").Append(Helper.ConvertSpecial(Helper.ToUpperAll(Name.Replace(" ", Constants.Space)))).Append("</span>");
      // Add details like category, race, level, health and armor at the same line. / 2015-08-09 / Wethospu
      if (!Category.Equals("Structure") && !Category.Equals("Trap") && !Category.Equals("Skill"))
      {
        htmlBuilder.Append("Level").Append(Constants.Space);
        // Level can be changed dynamically. / 2015-09-27 / Wethospu
        htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-left level-minus\"></span><span class=\"level\"></span>");
        htmlBuilder.Append("<span class=\"glyphicon glyphicon-chevron-right level-plus\"></span>").Append(" ");
      }
        
      if (!Category.Equals(""))
        htmlBuilder.Append(Helper.ToUpper(Helper.ConvertSpecial(Category)).Replace(" ", Constants.Space)).Append(" ");
      if (!Attributes.Family.Equals(""))
        htmlBuilder.Append(Helper.ToUpper(Helper.ConvertSpecial(Attributes.Family)).Replace(" ", Constants.Space)).Append(" ");
      // Enemy can have multiple sexes if there are model variations. / 2015-09-28 / Wethospu
      var sexes = Attributes.Sex.Split('|');
      for (int i = 0; i < sexes.Length; i++)
      {
        if (sexes[i].Equals("none"))
          continue;
        htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"").Append(sexes[i]).Append("\">").Append(Helper.ToUpper(sexes[i])).Append("</span>");
      }
      htmlBuilder.Append("<br>").Append(Constants.LineEnding);
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"health\">Health</span>:").Append(Constants.Space).Append("<span class=\"health\"></span>  ");
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"armor\">Armor</span>:").Append(Constants.Space).Append("<span class=\"armor\"></span>  ");
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"power\">Power</span>:").Append(Constants.Space).Append("<span class=\"power\"></span>  ");
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"precision\">Critical chance</span>:").Append(Constants.Space).Append("<span class=\"precision\"></span>%  ");
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"ferocity\">Critical damage</span>:").Append(Constants.Space).Append("<span class=\"ferocity\"></span>%  ");
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"condition\">Condition damage</span>:").Append(Constants.Space).Append("<span class=\"condition\"></span>  ");
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"healing_power\">Healing power</span>:").Append(Constants.Space).Append("<span class=\"healing-power\"></span> ");
      htmlBuilder.Append("<span class=").Append(Constants.IconClass).Append("title=\"size\">Size</span>:").Append(Constants.Space).Append(Math.Round(Attributes.Size * 100)).Append("%");
      htmlBuilder.Append(Constants.LineEnding).Append(Gw2Helper.AddTab(3)).Append("</div>").Append(Constants.LineEnding);
      // Add tactics. / 2015-08-09 / Wethospu
      htmlBuilder.Append(Tactics.ToHtml(Index, Path, enemies, 3));
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("</td>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<td class=\"enemy-media\">").Append(Constants.LineEnding);
      // Add thumbs.
      foreach (var media in Medias)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(3)).Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(4)).Append(media.ToHtml()).Append(Constants.LineEnding);
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
        attackHTMLs.Add(attack.AttackToHTML(Path, enemies, this, 2));
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
