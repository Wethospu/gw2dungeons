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
        var index = path.FractalScale - 1;
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
      GenerateFractalSimpleData(instanceData);
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
          entry.Append(Gw2Helper.AddTab(4)).Append("<li><a href=\"./").Append(path.Tag).Append("\">").Append(path.Name).Append("</a>");
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
        if (i%25 == 0)
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
        fractalData.Append("<li><a href=\"./F").Append(path.FractalScale).Append("\" data-scale=\"").Append(path.FractalScale).Append("\"><span class=\"list-icon\">");
        // Fractral information (instablities, agony damage, recommended agony resist).
        fractalData.Append(GenerateInstabilities(path.FractalScale));
        fractalData.Append("<span class=\"list-sub-filler-small\"></span>");
        fractalData.Append("<span class=\"list-sub-agony\"");
        if (path.Tag.ToLower().StartsWith("aqua"))
          fractalData.Append(" data-type=\"1\"");
        if (path.Tag.ToLower().StartsWith("mai") || path.Tag.ToLower().StartsWith("solid") || path.Tag.ToLower().StartsWith("molten"))
          fractalData.Append(" data-type=\"2\"");
        else
          fractalData.Append(" data-type=\"0\"");
        fractalData.Append("></span>").Append(Gw2Helper.GenerateHelpIcon(Constants.WebsiteIconLocation + "agony.png", "Agony damage based on your maximum health"));
        fractalData.Append("<span class=\"list-sub-ar\">").Append(Gw2Helper.RecommendedAgonyResist[i + 1]).Append("</span> ").Append(Gw2Helper.GenerateHelpIcon(Constants.WebsiteIconLocation + "ar.png", "Recommended agony reistance."));
        fractalData.Append("</span>").Append(path.FractalScale).Append(". ").Append(path.Name).Append("<br>").Append(Constants.Space);
        // Fractal reward information (karma, relics and daily fractals).
        fractalData.Append("<span class=\"list-icon\">");
        fractalData.Append("<span class=\"list-sub-fractal-karma\"></span>").Append(Gw2Helper.GenerateHelpIcon("http://wiki.guildwars2.com/images/a/af/Karma.png", "")); ;
        fractalData.Append("<span class=\"list-sub-fractal-relics\"></span>").Append(Gw2Helper.GenerateHelpIcon("https://wiki.guildwars2.com/images/5/57/Fractal_Relic.png", ""));
        fractalData.Append("<span class=\"list-sub-filler\"></span>");
        fractalData.Append("<span class=\"list-sub-daily-recommended-fractal\"></span>");
        fractalData.Append("<span class=\"list-sub-daily-fractal\"></span>");
        fractalData.Append("</span></a></li>");
        if ((i+1)%25 == 0)
          fractalData.Append(Gw2Helper.AddTab(3)).Append("</ul></li>");
      }
      instanceData.Add("ID_FRACTAL", fractalData.ToString());
      return instanceData;
    }

    private List<Instability> instabilities = new List<Instability>()
    {
      new Instability("Boon Fumbler", "When you dodge roll, you lose all your boons.", "https://wiki.guildwars2.com/images/thumb/7/7a/Zephyr%27s_Speed_%28elementalist%29.png/25px-Zephyr%27s_Speed_%28elementalist%29.png"),
      new Instability("No Pain, No Gain", "Enemies recieve random boons when you hit them for critical damage.", "https://wiki.guildwars2.com/images/thumb/2/2c/Arcane_Fury.png/25px-Arcane_Fury.png"),
      new Instability("Last Laugh", "Enemies explode when killed.", "https://wiki.guildwars2.com/images/thumb/8/82/Impact_Savant.png/25px-Impact_Savant.png"),
      new Instability("Sluggish", "The recharge time of skills you use is increased if you have a condition on you.", "https://wiki.guildwars2.com/images/thumb/4/47/Alpha_Training.png/25px-Alpha_Training.png"),
      new Instability("Afflicted", "Enemies apply random conditions.", "https://wiki.guildwars2.com/images/thumb/5/5c/Lotus_Poison.png/25px-Lotus_Poison.png"),
      new Instability("Boon Thieves", "Enemies steal boons when they hit you.", "https://wiki.guildwars2.com/images/thumb/d/d5/Master_of_Misdirection.png/25px-Master_of_Misdirection.png"),
      new Instability("Social Awkwardness", "Nearby allies receive agony.", "https://wiki.guildwars2.com/images/thumb/7/73/Driven_Fortitude.png/25px-Driven_Fortitude.png")
    };

    private StringBuilder GenerateInstabilities(int scale)
    {
      var builder = new StringBuilder();
      if (scale > 30 && scale < 41)
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[0].Icon, instabilities[0].Description));
      else if (scale > 40 && scale < 51)
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[1].Icon, instabilities[1].Description));
      else if (scale > 50 && scale < 61)
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[2].Icon, instabilities[2].Description));
      else if (scale > 60 && scale < 71)
      {
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[3].Icon, instabilities[3].Description));
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[4].Icon, instabilities[4].Description));
      }
      else if (scale > 70 && scale < 81)
      {
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[4].Icon, instabilities[4].Description));
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[0].Icon, instabilities[0].Description));
      }
      else if (scale > 80 && scale < 91)
      {
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[0].Icon, instabilities[0].Description));
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[5].Icon, instabilities[5].Description));
      }
      else if (scale > 90 && scale < 101)
      {
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[5].Icon, instabilities[5].Description));
        builder.Append(Gw2Helper.GenerateHelpIcon(instabilities[6].Icon, instabilities[6].Description));
      }
      else if (scale > 100)
        ErrorHandler.ShowWarningMessage("Scale " + scale + " not supported.");
      return builder;
    }

    /// <summary>
    /// Generates the conversion for fractals in a simplified format.
    /// </summary>
    private Dictionary<string, string> GenerateFractalSimpleData(Dictionary<string, string> instanceData)
    {
      var fractalData = new StringBuilder();
      for (var i = 0; i < FractalPaths.Count; i++)
      {
        // Note: Tabs or linebrakes CAN'T be used to prevent a space between fractal tables.
        if (i % 25 == 0)
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
        fractalData.Append("<li><a href=\"./F").Append(path.FractalScale).Append("\" data-scale=\"").Append(path.FractalScale).Append("\"><span class=\"list-icon\">");
        // Simplified fractal information (agony damage and daily fractals).
        fractalData.Append("<span class=\"list-sub-daily-recommended-fractal\"></span>");
        fractalData.Append("<span class=\"list-sub-daily-fractal\"></span>");
        fractalData.Append("<span class=\"list-sub-agony\"");
        if (path.Tag.ToLower().StartsWith("aqua"))
          fractalData.Append(" data-type=\"1\"");
        if (path.Tag.ToLower().StartsWith("mai") || path.Tag.ToLower().StartsWith("solid") || path.Tag.ToLower().StartsWith("molten"))
          fractalData.Append(" data-type=\"2\"");
        else
          fractalData.Append(" data-type=\"0\"");
        fractalData.Append("></span>").Append(Gw2Helper.GenerateHelpIcon(Constants.WebsiteIconLocation + "agony.png", "Agony damage based on your maximum health"));
        fractalData.Append("</span>").Append(path.FractalScale).Append(". ").Append(path.Name);
        fractalData.Append("</a></li>");
        if ((i + 1) % 25 == 0)
          fractalData.Append(Gw2Helper.AddTab(3)).Append("</ul></li>");
      }
      instanceData.Add("ID_FRACTAL_SIMPLE", fractalData.ToString());
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
          entry.Append(Gw2Helper.AddTab(4)).Append("<li><a href=\"./").Append(path.Tag).Append("\">").Append(path.Name).Append("</a>");
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
