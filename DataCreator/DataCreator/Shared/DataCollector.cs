using DataCreator.Encounters;
using DataCreator.Utility;
using System.Collections.Generic;
using System.Text;

namespace DataCreator.Shared
{
  /// <summary>
  /// Collects various data from encounters and enemies which is used to generate the home page and search filters dynamically.
  /// </summary>
  public class DataCollector
  {
    /// <summary>
    /// Information about dungeon path structure for the home page.
    /// </summary>
    private Dictionary<string, List<PathData>> DungeonPaths { get; set; }
    /// <summary>
    /// Information about fractal path structure for the home page.
    /// </summary>
    public List<PathData> FractalPaths { get; private set; }
    /// <summary>
    /// Information about raid path structure for the home page.
    /// </summary>
    private Dictionary<string, List<PathData>> RaidPaths { get; set; }
    /// <summary>
    /// List of used enemy ranks (legendary, champion, elite and so on).
    /// </summary>
    private SortedSet<string> Ranks { get; set; }
    /// <summary>
    /// Enemy filter tags get shorter tags to reduce url size. This is the next available short tag.
    /// </summary>
    private string NextShortTag;
    /// <summary>
    /// Conversion from a tag to its short tag.
    /// </summary>
    private SortedDictionary<string, string> OtherTagToShortTag { get; set; }
    /// <summary>
    /// Conversion from an effect tag to its short tag. Effects have their own dictionary because they appear separated on the site.
    /// </summary>
    private SortedDictionary<string, string> EffectTagToShortTag { get; set; }
    /// <summary>
    /// List of used enemy races (ghost, undead and so on).
    /// </summary>
    private SortedSet<string> Races { get; set; }

    public DataCollector()
    {
      DungeonPaths = new Dictionary<string, List<PathData>>();
      FractalPaths = new List<PathData>();
      RaidPaths = new Dictionary<string, List<PathData>>();
      Ranks = new SortedSet<string>();
      NextShortTag = "0";
      OtherTagToShortTag = new SortedDictionary<string, string>();
      EffectTagToShortTag = new SortedDictionary<string, string>();
      Races = new SortedSet<string>();
      // Special hardcoded ranks. Boss includes champions and legendaries. Thrash includes elites, veterans and normals.
      Ranks.Add("boss");
      Ranks.Add("thrash");
    }

    /// <summary>
    /// A cleaner interface for adding dungeon paths. Adds paths for one instance.
    /// </summary>
    public void AddDungeon(string dungeon, List<PathData> paths)
    {
      DungeonPaths.Add(dungeon, paths);
    }

    /// <summary>
    /// A cleaner interface for adding fractal paths. Adds paths for one instance.
    /// </summary>
    public void AddFractal(List<PathData> paths)
    {
      // Fractal paths aren't ordered by instance so they can't be added directly.
      foreach (PathData path in paths)
      {
        var index = path.Scale - 1;
        while (FractalPaths.Count <= index)
          FractalPaths.Add(null);
        if (index == -1)
        {
          ErrorHandler.ShowWarningMessage("Trying to add a fractal scale 0. Something is wrong.");
          continue;
        }
        if (FractalPaths[index] != null)
          ErrorHandler.ShowWarningMessage("Fractal F" + (index + 1) + " is already defined. (" + FractalPaths[index].Tag + ", " + path.Tag + ")");
        FractalPaths[index] = path;
      }
    }

    /// <summary>
    /// A cleaner interface for adding raid paths. Adds paths for one instance.
    /// </summary>
    public void AddRaid(string raid, List<PathData> paths)
    {
      RaidPaths.Add(raid, paths);
    }

    /// <summary>
    /// A cleaner interface for adding an enemy rank.
    /// </summary>
    public void AddRank(string rank)
    {
      Ranks.Add(rank);
    }

    /// <summary>
    /// An unified interface for adding tags.
    /// </summary>
    public void AddTag(string tag)
    {
      if (Constants.EffectTags.Contains(tag))
      {
        if (EffectTagToShortTag.ContainsKey(tag))
          return;
        EffectTagToShortTag.Add(tag, NextShortTag);
      }
      else
      {
        if (OtherTagToShortTag.ContainsKey(tag))
          return;
        OtherTagToShortTag.Add(tag, NextShortTag);
      }
      NextShortTag = char.ToString((char)(NextShortTag[0] + 1));
      // Some characters can't be used in HTML or urls so skip them.
      if (NextShortTag[0] == 60 || NextShortTag[0] == 62)
        NextShortTag = char.ToString((char)(NextShortTag[0] + 1));
    }

    /// <summary>
    /// An unified interface for getting a short tag.
    /// </summary>
    public string GetShortTag(string tag)
    {
      if (OtherTagToShortTag.ContainsKey(tag))
        return OtherTagToShortTag[tag];
      if (EffectTagToShortTag.ContainsKey(tag))
        return EffectTagToShortTag[tag];
      ErrorHandler.ShowWarningMessage("Internal error. Tag " + tag + " isn't recognized.");
      return "";
    }

    /// <summary>
    /// A cleaner interface for adding an enemy race.
    /// </summary>
    public void AddRace(string race)
    {
      if (race.Equals(""))
        return;
      Races.Add(race);
    }

    /// <summary>
    /// Generates the conversion needed to create the home page.
    /// </summary>
    public Dictionary<string, string> GenerateInstanceData()
    {
      // General format:
      // <h2>The Ruined City of Arah</h2>
      // <ul>
      //     <li><a href="./'page'">'Long path name'</a><span class="tracker" data-tag="'Path id'"></span></li>
      //     <li><a href="./'page'">'Long path name'</a><span class="tracker" data-tag="'Path id'"></span></li>
      // </ul>
      var instanceData = new Dictionary<string, string>();
      GenerateDungeonData(instanceData);
      GenerateFractalData(instanceData);
      GenerateRaidData(instanceData);
      return instanceData;
    }

    /// <summary>
    /// Generates the conversion for dungeons.
    /// </summary>
    private Dictionary<string, string> GenerateDungeonData(Dictionary<string, string> instanceData)
    {
      foreach (var dungeon in DungeonPaths.Keys)
      {
        if (DungeonPaths[dungeon] == null || DungeonPaths[dungeon].Count == 0)
        {
          ErrorHandler.ShowWarningMessage("Dungeon \"" + dungeon + "\" had no path data!");
          continue;
        }
        var entry = new StringBuilder();
        entry.Append(Gw2Helper.AddTab(3)).Append("<h2>").Append(DungeonPaths[dungeon][0].InstanceName).Append("</h2>").Append(Constants.LineEnding);
        entry.Append(Gw2Helper.AddTab(3)).Append("<ul class=\"nav nav-stacked\">").Append(Constants.LineEnding);
        foreach (var path in DungeonPaths[dungeon])
        {
          entry.Append(Gw2Helper.AddTab(4)).Append("<li><a href=\"./").Append(path.Tag).Append("\">").Append(path.NameLong).Append("</a>");
          entry.Append("</li>").Append(Constants.LineEnding);
        }
        entry.Append(Gw2Helper.AddTab(3)).Append("</ul>").Append(Constants.LineEnding);
        instanceData.Add("ID_" + dungeon.ToUpper(), entry.ToString());
      }
      return instanceData;
    }

    /// <summary>
    /// Generates the conversion for fractals.
    /// </summary>
    private Dictionary<string, string> GenerateFractalData(Dictionary<string, string> instanceData)
    {
      var fractalData = new StringBuilder();
      for (var i = 0; i < FractalPaths.Count; i++)
      {
        // Note: Tabs or linebrakes CAN'T be used to prevent a space between fractal tables.
        if (i%20 == 0)
          fractalData.Append("<li><ul class=\"nav nav-stacked\">").Append(Constants.LineEnding);
        var path = FractalPaths[i];
        // Format:
        // <ul>
        //     <li><a href="./'page'">'Long path name'</a></li>
        //     <li><a href="./'page'">'Long path name'</a></li>
        // </ul>
        if (path == null)
        {
          ErrorHandler.ShowWarningMessage("Fractal F" + (i + 1) + " had no path data!");
          continue;
        }
        fractalData.Append("<li><a href=\"./F").Append(path.Scale).Append("\"><span class=\"list-icon\">");
        fractalData.Append(Gw2Helper.RecommendedAgonyResist[i + 1]).Append(" <span class=").Append(Constants.IconClass).Append(" data-src=\"ar\" title=\"Agony Resistance\">AR</span>");
        fractalData.Append("</span>").Append(path.Scale).Append(". ").Append(path.NameLong).Append("<br>");
        if (i > 18)
        {
          fractalData.Append("<span class=\"list-sub list-sub-fractal\" data-scale=\"").Append(i + 1).Append("\" ");
          if (path.Tag.ToLower().StartsWith("aqua"))
            fractalData.Append(" data-type=\"1\"");
          if (path.Tag.ToLower().StartsWith("mai") || path.Tag.ToLower().StartsWith("solid") || path.Tag.ToLower().StartsWith("molten"))
            fractalData.Append(" data-type=\"2\"");
          else
            fractalData.Append(" data-type=\"0\"");
          fractalData.Append("></span>% <span class=").Append(Constants.IconClass).Append(" data-src=\"agony\" title=\"Agony\">Agony:</span>");
        }
        else
        {
          fractalData.Append(Constants.Space);
        }
        fractalData.Append("</a></li>");
        if ((i+1)%20 == 0)
          fractalData.Append(Gw2Helper.AddTab(3)).Append("</ul></li>");
      }
      instanceData.Add("ID_FRACTAL", fractalData.ToString());
      return instanceData;
    }

    /// <summary>
    /// Generates the conversion for raids.
    /// </summary>
    private Dictionary<string, string> GenerateRaidData(Dictionary<string, string> instanceData)
    {
      var raidData = new StringBuilder();
      foreach (var raid in RaidPaths.Keys)
      {
        if (RaidPaths[raid] == null || RaidPaths[raid].Count == 0)
        {
          ErrorHandler.ShowWarningMessage("Raid \"" + raid + "\" had no path data!");
          continue;
        }
        var entry = new StringBuilder();
        entry.Append(Gw2Helper.AddTab(3)).Append("<h2>").Append(RaidPaths[raid][0].InstanceName).Append("</h2>").Append(Constants.LineEnding);
        entry.Append(Gw2Helper.AddTab(3)).Append("<ul class=\"nav nav-stacked\">").Append(Constants.LineEnding);
        foreach (var path in RaidPaths[raid])
        {
          entry.Append(Gw2Helper.AddTab(4)).Append("<li><a href=\"./").Append(path.Tag).Append("\">").Append(path.NameLong).Append("</a>");
          entry.Append("</li>").Append(Constants.LineEnding);
        }
        entry.Append(Gw2Helper.AddTab(3)).Append("</ul>").Append(Constants.LineEnding);
        instanceData.Add("ID_" + raid.ToUpper(), entry.ToString());
      }
      return instanceData;
    }

    /// <summary>
    /// Generates the search filter for instances.
    /// </summary>
    public string GenerateInstanceHtml()
    {
      var builder = new StringBuilder();
      foreach (var raid in RaidPaths.Keys)
      {
        var paths = RaidPaths[raid];
        if (paths.Count == 0)
          continue;
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"");
        foreach (var path in paths)
          builder.Append(path.Tag.ToLower()).Append("|");
        builder.Remove(builder.Length - 1, 1).Append("\">");
        builder.Append(paths[0].InstanceName).Append("</option>").Append(Constants.LineEnding);
      }
      // Sorting has no practical purpose but looks better.
      var FractalTags = new SortedSet<string>();
      foreach (var fractal in FractalPaths)
        FractalTags.Add(fractal.Tag.ToLower());
      builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"");
      foreach (var tag in FractalTags)
        builder.Append(tag).Append("|");
      builder.Remove(builder.Length - 1, 1).Append("\">");
      builder.Append(FractalPaths[0].InstanceName).Append("</option>").Append(Constants.LineEnding);

      foreach (var dungeon in DungeonPaths.Keys)
      {
        var paths = DungeonPaths[dungeon];
        if (paths.Count == 0)
          continue;
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"");
        foreach (var path in paths)
          builder.Append(path.Tag.ToLower()).Append("|");
        builder.Remove(builder.Length - 1, 1).Append("\">");
        builder.Append(paths[0].InstanceName).Append("</option>").Append(Constants.LineEnding);
      }
      
      return builder.ToString();
    }

    /// <summary>
    /// Generates the search filter for ranks.
    /// </summary>
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

    /// <summary>
    /// Generates the search filter for races.
    /// </summary>
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

    /// <summary>
    /// Generates the search filter for tags.
    /// </summary>
    public string GenerateTagHtml()
    {
      var builder = new StringBuilder();
      foreach (var tag in OtherTagToShortTag.Keys)
      {
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(OtherTagToShortTag[tag]).Append("\">");
        builder.Append(Helper.ToUpperAll(tag)).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }

    /// <summary>
    /// Generates the search filter for effect tags.
    /// </summary>
    public string GenerateEffectTagHtml()
    {
      var builder = new StringBuilder();
      foreach (var tag in EffectTagToShortTag.Keys)
      {
        builder.Append(Gw2Helper.AddTab(3)).Append("<option value=\"").Append(EffectTagToShortTag[tag]).Append("\">");
        builder.Append(Helper.ToUpperAll(tag)).Append("</option>").Append(Constants.LineEnding);
      }
      return builder.ToString();
    }
  }
}
