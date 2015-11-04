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
    private Dictionary<string, List<PathData>> DungeonPaths { get; set; }
    public List<PathData> FractalPaths { get; private set; }
    private SortedSet<string> Ranks { get; set; }
    private string CurrentTag;
    private SortedDictionary<string, string> Tags { get; set; }
    private SortedDictionary<string, string> EffectTags { get; set; }
    private SortedSet<string> Races { get; set; }

    public DataCollector()
    {
      DungeonPaths = new Dictionary<string, List<PathData>>();
      FractalPaths = new List<PathData>();
      Ranks = new SortedSet<string>();
      CurrentTag = "0";
      Tags = new SortedDictionary<string, string>();
      EffectTags = new SortedDictionary<string, string>();
      Races = new SortedSet<string>();

      Ranks.Add("boss");
      Ranks.Add("thrash");
    }

    /***********************************************************************************************
    * Add***** / 2015-08-14 / Wethospu                                                            *
    *                                                                                             *
    * Cleaner interface for adding stuff.                                                         *
    *                                                                                             *
    ***********************************************************************************************/
    public void AddDungeon(string dungeon, List<PathData> paths)
    {
      DungeonPaths.Add(dungeon, paths);
    }

    public void AddFractal(List<PathData> paths)
    {
      foreach (PathData path in paths)
      {
        var index = path.Scale - 1;
        while (FractalPaths.Count <= index)
          FractalPaths.Add(null);
        if (FractalPaths[index] != null)
          Helper.ShowWarningMessage("Fractal F" + (index + 1) + " is already defined. (" + FractalPaths[index].Tag + ", " + path.Tag + ")");
        FractalPaths[index] = path;
      }
    }

    public void AddRank(string rank)
    {
      Ranks.Add(rank);
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
      if (race.Equals(""))
        return;
      Races.Add(race);
    }

     /***********************************************************************************************
     * GenerateDungeonData / 2014-08-01 / Wethospu                                                 *
     *                                                                                             *
     * Generates "dungeon translation" to generate main page.                                      *
     *                                                                                             *
     * Returns generated translation data.                                                         *
     *                                                                                             *
     ***********************************************************************************************/
    
    public Dictionary<string, string> GenerateDungeonData()
    {
      var dungeonData = new Dictionary<string, string>();
      foreach (var dungeon in DungeonPaths.Keys)
      {
        // Generate entry for dungeon.
        // Format:
        // <h2>The Ruined City of Arah</h2>
        // <ul>
        //     <li><a href="./'page'">'Long path name'</a><span class="tracker" data-tag="'Path id'"></span></li>
        //     <li><a href="./'page'">'Long path name'</a><span class="tracker" data-tag="'Path id'"></span></li>
        // </ul>
        if (DungeonPaths[dungeon] == null || DungeonPaths[dungeon].Count == 0)
        {
          Helper.ShowWarningMessage("Dungeon \"" + dungeon + "\" had no path data!");
          continue;
        }
        var entry = new StringBuilder();
        entry.Append(Gw2Helper.AddTab(3)).Append("<h2>").Append(DungeonPaths[dungeon][0].DungeonName).Append("</h2>").Append(Constants.LineEnding);
        entry.Append(Gw2Helper.AddTab(3)).Append("<ul class=\"nav nav-stacked\">").Append(Constants.LineEnding);
        foreach (var path in DungeonPaths[dungeon])
        {
          entry.Append(Gw2Helper.AddTab(4)).Append("<li><a href=\"./").Append(path.Tag).Append("\">").Append(path.NameLong).Append("</a>");
          //entry.Append("<span class=\"tracker\" data-tag=\"").Append(path.Tag).Append("\"></span>");
          entry.Append("</li>").Append(Constants.LineEnding);
        }
        entry.Append(Gw2Helper.AddTab(3)).Append("</ul>").Append(Constants.LineEnding);
        dungeonData.Add("ID_" + dungeon.ToUpper(), entry.ToString());
      }
      dungeonData.Add("ID_FRACTAL", GenerateFractalData());
      return dungeonData;
    }

    /***********************************************************************************************
     * GenerateFractalData / 2015-10-28 / Wethospu                                                 *
     *                                                                                             *
     * Generates "fractal translation" to generate main page.                                      *
     *                                                                                             *
     * Returns generated translation data.                                                         *
     *                                                                                             *
     ***********************************************************************************************/

    public string GenerateFractalData()
    {
      var fractalData = new StringBuilder();
      for (var i = 0; i < FractalPaths.Count; i++)
      {
        // Note: No tabs or linebrakes used to prevent a space between fractal tables. / 2015-11-14 / Wethospu
        if (i%20 == 0)
          fractalData.Append("<li><ul class=\"nav nav-stacked\">").Append(Constants.LineEnding);
        var path = FractalPaths[i];
        // Generate an entry for a fractal.
        // Format:
        // <ul>
        //     <li><a href="./'page'">'Long path name'</a</li>
        //     <li><a href="./'page'">'Long path name'</a></li>
        // </ul>
        if (path == null)
        {
          Helper.ShowWarningMessage("Fractal F" + (i + 1) + " had no path data!");
          continue;
        }
        fractalData.Append(Gw2Helper.AddTab(4)).Append("<li><a href=\"./F").Append(path.Scale).Append("\"><span class=\"list-icon\">");
        fractalData.Append(Gw2Helper.RecommendedAgonyResist[i + 1]).Append(" <span class=").Append(Constants.IconClass).Append(" data-src=\"ar\" title=\"Agony Resistance\">AR</span>");
        fractalData.Append("</span>").Append(path.Scale).Append(". ").Append(path.NameLong).Append("</a></li>").Append(Constants.LineEnding);

        //"<span class=" + Constants.IconClass + " data-src=\"" + icon.ToLower() + "\" title=\""
        if ((i+1)%20 == 0)
          fractalData.Append(Gw2Helper.AddTab(3)).Append("</ul></li>");
      }
      return fractalData.ToString();
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
      foreach (var dungeon in DungeonPaths.Keys)
      {
        var paths = DungeonPaths[dungeon];
        if (paths.Count == 0)
          continue;
        // Add only dungeons. / 2015-08-19 / Wethospu
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(dungeon).Append("\">");
        builder.Append(paths[0].DungeonName).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }

    /***********************************************************************************************
    * GenerateRankHtml / 2015-08-14 / Wethospu                                                     *
    *                                                                                              *
    * Converts ranks to usable html.                                                               *
    *                                                                                              *
    ***********************************************************************************************/

    public string GenerateRankHtml()
    {
      var builder = new StringBuilder();
      foreach (var rank in Ranks)
      {
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(rank).Append("\">");
        builder.Append(Helper.ToUpper(rank)).Append("</option>").Append(Constants.LineEnding);
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
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(race.ToLower().Replace(Constants.Space, "_")).Append("\">");
        builder.Append(race).Append("</option>").Append(Constants.LineEnding);
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
