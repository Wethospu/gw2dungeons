using DataCreator.Encounters;
using DataCreator.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataCreator.Shared
{
  // Collects various data from encounters and enemies. / 2015-08-14 / Wethospu
  // This data is used to generate some stuff on the pages. / 2015-08-14 / Wethospu
  public class DataCollector
  {
    private Dictionary<string, List<PathData>> Paths { get; set; }
    private SortedSet<string> Categories { get; set; }
    private string CurrentTag;
    private SortedDictionary<string, string> Tags { get; set; }
    private SortedDictionary<string, string> EffectTags { get; set; }
    private SortedSet<string> Races { get; set; }

    public DataCollector()
    {
      Paths = new Dictionary<string, List<PathData>>();
      Categories = new SortedSet<string>();
      CurrentTag = "0";
      Tags = new SortedDictionary<string, string>();
      EffectTags = new SortedDictionary<string, string>();
      Races = new SortedSet<string>();

      Categories.Add("boss");
      Categories.Add("thrash");
    }

    /***********************************************************************************************
    * Add***** / 2015-08-14 / Wethospu                                                            *
    *                                                                                             *
    * Cleaner interface for adding stuff.                                                         *
    *                                                                                             *
    ***********************************************************************************************/
    public void AddDungeon(string dungeon, List<PathData> paths)
    {
      Paths.Add(dungeon, paths);
    }

    public void AddCategory(string category)
    {
      Categories.Add(category);
    }

    public void AddTag(string tag)
    {
      if (Constants.EffectTags.Contains(tag))
      {
        if (EffectTags.ContainsKey(tag))
          return;
        EffectTags.Add(tag, CurrentTag);
      }
      else
      {
        if (Tags.ContainsKey(tag))
          return;
        Tags.Add(tag, CurrentTag);
      }
      // Generate a new tag short value. / 2015-08-17 / Wethospu
      CurrentTag = char.ToString((char)((int)CurrentTag[0] + 1));
      // Skip problematic characters. / 2015-08-17 / Wethospu
      if ((int)CurrentTag[0] == 60 || (int)CurrentTag[0] == 62)
        CurrentTag = char.ToString((char)((int)CurrentTag[0] + 1));
    }

    public string ConvertTag(string tag)
    {
      if (Tags.ContainsKey(tag))
        return Tags[tag];
      if (EffectTags.ContainsKey(tag))
        return EffectTags[tag];
      Helper.ShowWarningMessage("Internal error. Tag " + tag + " isn't recognized.");
      return "";
    }

    public void AddRace(string race)
    {
      Races.Add(race);
    }

     /***********************************************************************************************
     * GenerateDungeonData / 2014-08-01 / Wethospu                                                 *
     *                                                                                             *
     * Generates "dungeon translation" to generate main page.                                      *
     *                                                                                             *
     * Returns generated translation data.                                                         *
     * pathData: Information for generated paths.                                                  *
     *                                                                                             *
     ***********************************************************************************************/
    
    public Dictionary<string, string> GenerateDungeonData()
    {
      var dungeonData = new Dictionary<string, string>();
      foreach (var dungeon in Paths.Keys)
      {
        // Generate entry for dungeon.
        // Format:
        // <h2>The Ruined City of Arah</h2>
        // <ul>
        //     <li><a href="./'page'">'Long path name'</a><span class="tracker" data-tag="'Path id'"></span></li>
        //     <li><a href="./'page'">'Long path name'</a><span class="tracker" data-tag="'Path id'"></span></li>
        // </ul>
        if (Paths[dungeon] == null || Paths[dungeon].Count == 0)
        {
          Helper.ShowWarningMessage("Dungeon \"" + dungeon + "\" had no path data!");
          continue;
        }
        var entry = new StringBuilder();
        entry.Append(Gw2Helper.AddTab(3)).Append("<h2>").Append(Paths[dungeon][0].DungeonName).Append("</h2>").Append(Constants.LineEnding);
        entry.Append(Gw2Helper.AddTab(3)).Append("<ul class=\"nav nav-stacked\">").Append(Constants.LineEnding);
        foreach (var path in Paths[dungeon])
        {
          entry.Append(Gw2Helper.AddTab(4)).Append("<li><a href=\"./").Append(path.PathTag).Append("\">").Append(path.PathNameLong).Append("</a>");
          // Only add completion status to dungeons. / 2015-07-11 / Wethospu
          if (!dungeon.ToLower().Equals("fotm"))
            entry.Append("<span class=\"tracker\" data-tag=\"").Append(path.PathTag).Append("\"></span>");
          entry.Append("</li>").Append(Constants.LineEnding);
        }
        entry.Append(Gw2Helper.AddTab(3)).Append("</ul>").Append(Constants.LineEnding);
        dungeonData.Add("DUNGEON_" + dungeon.ToUpper(), entry.ToString());
      }
      return dungeonData;
    }


    /***********************************************************************************************
    * GeneratePathHtml / 2015-08-14 / Wethospu                                                    *
    *                                                                                             *
    * Converts paths to usable html.                                                              *
    *                                                                                             *
    ***********************************************************************************************/

    public string GenerateDungeonHtml()
    {
      var builder = new StringBuilder();
      foreach (var dungeon in Paths.Keys)
      {
        var paths = Paths[dungeon];
        if (paths.Count == 0)
          continue;
        // Add only dungeons. / 2015-08-19 / Wethospu
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(dungeon).Append("\">");
        builder.Append(paths[0].DungeonName).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }

    /***********************************************************************************************
   * GenerateCategoryHtml / 2015-08-14 / Wethospu                                                *
   *                                                                                             *
   * Converts categories to usable html.                                                         *
   *                                                                                             *
   ***********************************************************************************************/

    public string GenerateCategoryHtml()
    {
      var builder = new StringBuilder();
      foreach (var category in Categories)
      {
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(category).Append("\">");
        builder.Append(Helper.ToUpper(category)).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }

    /***********************************************************************************************
    * GenerateRaceHtml / 2015-08-14 / Wethospu                                                    *
    *                                                                                             *
    * Converts races to usable html.                                                              *
    *                                                                                             *
    ***********************************************************************************************/

    public string GenerateRaceHtml()
    {
      var builder = new StringBuilder();
      foreach (var race in Races)
      {
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(race).Append("\">");
        builder.Append(Helper.ToUpperAll(race)).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }

    /***********************************************************************************************
    * GenerateTagHtml / 2015-08-14 / Wethospu                                                     *
    *                                                                                             *
    * Converts tags to usable html.                                                               *
    *                                                                                             *
    ***********************************************************************************************/

    public string GenerateTagHtml()
    {
      var builder = new StringBuilder();
      foreach (var tag in Tags.Keys)
      {
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(Tags[tag]).Append("\">");
        builder.Append(Helper.ToUpperAll(tag)).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }

    public string GenerateEffectTagHtml()
    {
      var builder = new StringBuilder();
      foreach (var tag in EffectTags.Keys)
      {
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(EffectTags[tag]).Append("\">");
        builder.Append(Helper.ToUpperAll(tag)).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }
  }
}
