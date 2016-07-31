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
    public static InstanceData ReadInstance(string location, string instance, List<Enemy> enemies)
    {
      string[] lines = Helper.ReadFile(location + instance + ".txt");
      if (lines == null)
        return null;
      var encounterData = new InstanceData();
      for (var row = 0; row < lines.Length; row++)
      {
        ErrorHandler.InitializeWarningSystem(row + 1, lines[row]);
        HandleLine(lines[row], encounterData.Encounters, encounterData.Paths, instance);
      }
      // This code is also used for non-instances which don't have paths.
      // So add a fake path to make everything work correctly.
      // TODO: Figure out why this is necessary and refactor.
      if (encounterData.Paths.Count == 0)
        encounterData.Paths.Add(new PathData(instance, instance));
      // Set unique indexes so HTML tabs get unique IDs.
      for (var i = 0; i < encounterData.Encounters.Count; i++)
        encounterData.Encounters[i].Index = i + Constants.UniqueIndexCounter;
      Constants.UniqueIndexCounter += encounterData.Encounters.Count;
      return encounterData;
    }

    /// <summary>
    /// Processes one line of data.
    /// </summary>
    private static void HandleLine(string line, List<Encounter> encounters, List<PathData> paths, string instance)
    {
      Encounter currentEncounter = null;
      if (encounters.Count > 0)
        currentEncounter = encounters[encounters.Count - 1];
      if (!Helper.CheckLineValidity(line))
        return;
      // Check for weird characters but allow "|" to skip next line.
      if (!char.IsLetterOrDigit(line[0]) && line[0] != '|' && line[0] != '~')
      {
        ErrorHandler.ShowWarning("Line starts with a weird character. Please remove!");
        return;
      }

      var text = new TagData(line, Constants.TagSeparator);
      if (text.Tag.Equals("init"))
        paths.Add(new PathData(line, instance));
      else if (text.Tag.Equals("path"))
        HandlePath(text.Data);
      else if (text.Tag.Equals("name"))
      {
        encounters.Add(new Encounter());
        currentEncounter = encounters[encounters.Count - 1];
        currentEncounter.Name = text.Data;
        currentEncounter.DataName = Helper.Simplify(LinkGenerator.RemoveLinks(text.Data));
        currentEncounter.Paths = new List<string>(_currentPath.Split('|'));
      }
      else
      {
        if (currentEncounter == null)
        {
          ErrorHandler.ShowWarning("Ignoring the line because there is no active encounter");
          return;
        }
        if (text.Tag.Equals("image"))
          currentEncounter.HandleMedia(text.Data, instance);
        else if (text.Tag.Equals("tactic"))
          currentEncounter.HandleTactic(text.Data, paths);
        else
          HandleNormalLine(line, currentEncounter, paths);
      }
    }
    private static string _currentPath = "";
    /// <summary>
    /// Sets the active path. This DOESN'T affect the current encounter.
    /// </summary>
    // TODO: Try to figure out better raw data logic so path info would work same as everything else.
    // TODO: Initial idea was to group up enemies/enemies with the same path so the path would have to be given only once.
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
    /// Adds a line to encounter's active tactics.
    /// </summary>
    private static void HandleNormalLine(string data, Encounter encounter, List<PathData> paths)
    {
      // Preprocess the line to avoid doing same stuff 25+ times.
      data = LinkGenerator.CheckLinkSyntax(data);
      if (encounter.Tactics.Count == 0)
        encounter.Tactics.AddTactics("normal", "", paths);
      encounter.Tactics.AddLine(data);
    }

    /// <summary>
    /// Replaces enemy and other link tags with html.
    /// </summary>
    public static InstanceData CreateLinks(InstanceData instance, List<Enemy> enemies)
    {
      foreach (var encounter in instance.Encounters)
        encounter.CreateLinks(encounter.Paths, enemies);
      return instance;
    }

    /// <summary>
    /// Creates output files. Some final generation still happens here so extra data is needed.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="enemies">Needed to create enemy links.</param>
    /// <param name="relatedPaths">Needed to create the navigation bar.</param>
    public static void CreateFiles(InstanceData instance, List<Enemy> enemies, List<PathData> relatedPaths)
    {
      instance = CreateLinks(instance, enemies);
      foreach (var path in instance.Paths)
      {
        var encounterCounter = 0;
        var encounterFile = new StringBuilder();
        encounterFile.Append("GUIDE").Append(Constants.Delimiter).Append(path.FractalScale).Append(Constants.ForcedLineEnding);
        encounterFile.Append(Helper.ConvertSpecial(path.InstanceName)).Append(Constants.Delimiter).Append(Helper.ConvertSpecial(path.NavigationName)).Append(Constants.ForcedLineEnding);
        encounterFile.Append(GenerateNavigationInfo(path, relatedPaths));
        encounterFile.Append(Constants.InitialdataHtml);
        var pathEncounters = instance.Encounters.Where(encounter => encounter.Paths.Contains(path.Tag.ToLower()) && encounter.Tactics.IsAvailableForScale(path.FractalScale));
        foreach (var encounter in pathEncounters)
        {
          encounter.CopyToEnemyTactics(enemies);
          encounterFile.Append(encounter.ToHtml(encounterCounter, pathEncounters, path.FractalScale, path.Map));
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
    private static string GenerateNavigationInfo(PathData currentPath, List<PathData> relatedPaths)
    {
      var names = new List<string>();
      var links = new List<string>();
      if (currentPath.FractalScale == 0)
      {
        // Dungeon and raid instances have their paths separated from each other.
        // So paths have data only for one instance.
        foreach (var path in relatedPaths)
        {
          if (path == null)
          {
            ErrorHandler.ShowWarningMessage("A path is not defined.");
            continue;
          }
          names.Add(path.NavigationName);
          links.Add(path.Filename);
        }
      }
      else
      {
        // All fractal paths are mixed together so they require a different implementation.
        // There are 100 paths so obviously not everything can be included in the small navigation bar.
        // NOTE: relatedPaths indexing starts from 0 while scales start from 1.
        var startingScale = currentPath.FractalScale - Constants.FractalNavPathCount / 2;
        if (startingScale + Constants.FractalNavPathCount >= relatedPaths.Count)
          startingScale = relatedPaths.Count - Constants.FractalNavPathCount;
        if (startingScale < 1)
          startingScale = 1;
        for (var index = startingScale - 1; index < startingScale + Constants.FractalNavPathCount; index++)
        {
          if (currentPath.FractalScale == index + 1 || index >= relatedPaths.Count)
            continue;
          if (relatedPaths[index] == null)
          {
            ErrorHandler.ShowWarningMessage("Scale " + index + 1 + " is not defined.");
            continue;
          }
          names.Add("Scale " + relatedPaths[index].FractalScale + ": " + relatedPaths[index].NavigationName);
          links.Add(relatedPaths[index].Filename);
        }
      }
      return string.Join("|", names) + Constants.ForcedLineEnding + string.Join("|", links) + Constants.ForcedLineEnding;
    }
  }
}
