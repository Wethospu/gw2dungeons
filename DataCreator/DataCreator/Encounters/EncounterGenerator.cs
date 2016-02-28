using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataCreator.Enemies;
using DataCreator.Utility;
using DataCreator.Shared;
using System.Linq;

namespace DataCreator.Encounters
{
  /// <summary>
  /// Converts raw encounter data to HTML. Also checks for many syntax or logical errors.
  /// </summary>
  public static class EncounterGenerator
  {
    /// <summary>
    /// Converts raw encounter data to objects.
    /// </summary>
    public static EncounterData ReadInstance(string location, string instance, List<Enemy> enemies)
    {
      string[] lines = Helper.ReadFile(location + instance + ".txt");
      if (lines == null)
        return null;
      var encounterData = new EncounterData();
      for (var row = 0; row < lines.Length; row++)
      {
        ErrorHandler.InitializeWarningSystem(row + 1, lines[row]);
        HandleLine(lines[row], encounterData.Encounters, encounterData.Paths);
      }
      // This code is also used for non-instances which don't have paths.
      // So add a fake path to make everything work correctly.
      // TODO: Figure out why this is necessary and refactor.
      if (encounterData.Paths.Count == 0)
        encounterData.Paths.Add(new PathData(instance));
      // Set unique indexes so HTML tabs get unique IDs.
      for (var i = 0; i < encounterData.Encounters.Count; i++)
        encounterData.Encounters[i].Index = i + Constants.UniqueIndexCounter;
      Constants.UniqueIndexCounter += encounterData.Encounters.Count;
      return encounterData;
    }

    /// <summary>
    /// Processes one line of data.
    /// </summary>
    private static void HandleLine(string line, List<Encounter> encounters, List<PathData> paths)
    {
      Encounter currentEncounter = null;
      if (encounters.Count > 0)
        currentEncounter = encounters[encounters.Count - 1];
      if (line == "" || line[0] == '#')
        return;
      if (string.IsNullOrWhiteSpace(line))
      {
        ErrorHandler.ShowWarning("Line contains only whitespace (ignored). Please remove!");
        return;
      }
      // Check for weird characters but allow "|" to skip next line.
      if (!char.IsLetterOrDigit(line[0]) && line[0] != '|' && line[0] != '~')
      {
        ErrorHandler.ShowWarning("Line starts with a weird character. Please remove!");
        return;
      }
      if (line[0] == ' ')
        ErrorHandler.ShowWarning("Extra space detected. Please remove!");

      //// Split row to tag and data.
      var tagIndex = line.IndexOf(Constants.TagSeparator);
      // No data found. This will be handled later when tag has been analyzed.
      if (tagIndex < 0)
        tagIndex = line.Length - 1;
      var tag = line.Substring(0, tagIndex);
      tag = tag.ToLower();
      var data = "";
      if (tagIndex < line.Length)
        data = line.Substring(tagIndex + 1);
      //// Tag and data separated.

      if (tag.Equals("init"))
        paths.Add(new PathData(line));
      else if (tag.Equals("path"))
        HandlePath(data);
      else if (tag.Equals("name"))
      {
        encounters.Add(new Encounter());
        currentEncounter = encounters[encounters.Count - 1];
        HandleName(data, currentEncounter);
      }
      else if (tag.Equals("image"))
        HandleMedia(data, currentEncounter);
      else if (tag.Equals("tactic"))
        HandleTactic(data, currentEncounter, paths);
      else
        HandleNormalLine(line, currentEncounter, paths); 
    }

    /// <summary>
    /// Sets encounter's name. This is used to initialize the encounter so path information is also set.
    /// </summary>
    private static void HandleName(string data, Encounter encounter)
    {
      if (data.Length > 0)
      {
        data = LinkGenerator.CheckLinkSyntax(data);
        encounter.Name = data;
        encounter.Path = _currentPath;
      }
      else
        ErrorHandler.ShowWarning("Missing info. Use \"name='name'\"!");
    }

    private static string _currentPath = "";
    /// <summary>
    /// Sets the active path. This DOESN'T affect the current encounter.
    /// </summary>
    // TODO: Try to figure out better raw data logic so path info would work same as everything else.
    // TODO: Initial idea was to group up enemies with the same path so the path would have to be given only once.
    // TODO: Perhaps making it a normal value would be more logical.
    private static void HandlePath(string data)
    {
      if (data.Length > 0)
      {
        if (data.Contains(" "))
        {
          ErrorHandler.ShowWarning("' ' found. Use syntax \"path='path1'|'path2'|'pathN'\"");
          data = data.Replace(' ', '|');
        }
        _currentPath = data.ToLower();
      }
      else
        ErrorHandler.ShowWarning("Missing info. Use \"path='path1'|'path2'|'pathN'\"!");
    }

    /// <summary>
    /// Adds media data to a given encounter.
    /// </summary>
    private static void HandleMedia(string data, Encounter encounter)
    {
      if (data.Length > 0)
        encounter.Medias.Add(new Media(data));
      else
        ErrorHandler.ShowWarning("Missing info. Use \"image='image'\"!");
    }

    /// <summary>
    /// Activates matching encounter's tactics. Also creates any missing tactics.
    /// </summary>
    private static void HandleTactic(string data, Encounter encounter, List<PathData> paths)
    {
      if (data.Length > 0)
      {
        var split = new List<string>(data.Split('|'));
        var scales = "";
        // Tactics may have information about their fractal scale.
        var useless = 0;
        var subSplit = split[split.Count - 1].Split('-');
        if (int.TryParse(subSplit[0], out useless) && (subSplit.Length == 1 || int.TryParse(subSplit[1], out useless)))
        {
          scales = split[split.Count - 1];
          split.RemoveAt(split.Count - 1);
        }
        var name = string.Join("|", split);
        encounter.Tactics.AddTactics(name, scales, paths);
      }

      else
        ErrorHandler.ShowWarning("Missing info. Use \"tactic='tactic1'|'tactic2'|'tacticN'\"!");
    }

    /// <summary>
    /// Adds a line to encounter's active tactics.
    /// </summary>
    private static void HandleNormalLine(string data, Encounter encounter, List<PathData> paths)
    {
      // Preprocess the line to avoid doing same stuff 25+ times.
      data = LinkGenerator.CreatePageLinks(LinkGenerator.CheckLinkSyntax(data));
      if (encounter.Tactics.Count == 0)
        encounter.Tactics.AddTactics("normal", "", paths);
      encounter.Tactics.AddLine(data);
    }

    /***********************************************************************************************
     * GenerateFiles / 2014-08-01 / Wethospu                                                       *
     *                                                                                             *
     * Creates html data files from gathered info.                                                 *
     *                                                                                             *
     * paths: Information about generated paths.                                                   *
     * encounters: Generated encounters.                                                           *
     * enemies: List of enemies. Needed for enemy links.                                           *
     * navPaths: Path information for navigation bar.                                              *
     *                                                                                             *
     ***********************************************************************************************/
    public static void GenerateFiles(List<PathData> instance, List<Encounter> encounters, List<Enemy> enemies, List<PathData> allInstances)
    {
      foreach (var path in instance)
      {
        var encounterCounter = 0;
        var encounterFile = new StringBuilder();
        encounterFile.Append("GUIDE:").Append(Helper.ConvertSpecial(path.InstanceName)).Append(Constants.Delimiter).Append(Helper.ConvertSpecial(path.Name)).Append(Constants.ForcedLineEnding);
        encounterFile.Append(GenerateNavigationInfo(path, allInstances));
        encounterFile.Append(Constants.InitialdataHtml);
        var filtered = encounters.Where(encounter => encounter.Path.Contains(path.Tag) && encounter.Tactics.IsAvailableForScale(path.Scale));
        foreach (var encounter in filtered)
        {
          encounterFile.Append(encounter.ToHtml(encounterCounter, filtered, enemies, path.Scale, path.Map));
          encounterCounter++;
        }
        var fileName = Constants.DataOutput + Constants.DataEncounterResult + path.Filename.ToLower() + ".htm";
        var directory = Path.GetDirectoryName(fileName);
        if (directory != null)
          Directory.CreateDirectory(directory);
        try
        {
          File.WriteAllText(fileName, encounterFile.ToString());
        }
        catch (UnauthorizedAccessException)
        {
          ErrorHandler.ShowWarningMessage("File " + fileName + " in use.");
        }
      }
    }

    /// <summary>
    /// Returns HTML for the navigation bar.
    /// </summary>
    private static string GenerateNavigationInfo(PathData currentPath, List<PathData> paths)
    {
      var names = new List<string>();
      var links = new List<string>();
      if (currentPath.Scale == 0)
      {
        // Dungeon and raid instances have their paths separated from each other.
        // So paths have data only for one instance.
        foreach (var path in paths)
        {
          names.Add(path.Name);
          links.Add(path.Filename);
        }
      }
      else
      {
        // All fractal paths are mixed together so they require a different implementation.
        // There are 100 paths so obviously not everything can be included in the small navigation bar.
        var startingScale = currentPath.Scale - Constants.FractalNavPathCount / 2;
        if (startingScale + Constants.FractalNavPathCount >= paths.Count)
          startingScale = paths.Count - Constants.FractalNavPathCount;
        if (startingScale < 1)
          startingScale = 1;
        for (var scale = startingScale; scale < startingScale + Constants.FractalNavPathCount; scale++)
        {
          if (currentPath.Scale == scale || scale >= paths.Count)
            continue;
          names.Add("Scale " + paths[scale].Scale + ": " + paths[scale].Name);
          links.Add(paths[scale].Filename);
        }
      }
      return string.Join("|", names) + Constants.ForcedLineEnding + string.Join("|", links) + Constants.ForcedLineEnding;
    }
  }
}
