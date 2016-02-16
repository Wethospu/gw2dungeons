using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataCreator.Enemies;
using DataCreator.Utility;
using DataCreator.Shared;

namespace DataCreator.Encounters
{

  /***********************************************************************************************
   * EncounterGenerator / 2014-08-01 / Wethospu                                                  *
   *                                                                                             *
   * Generates encounters. Checks for syntax / content errors.                                   *
   *                                                                                             *
   ***********************************************************************************************/

  public static class EncounterGenerator
  {
    /***********************************************************************************************
     * GeneratePaths / 2014-08-01 / Wethospu                                                       *
     *                                                                                             *
     * Generates paths and their encounters for one dungeon.                                       *
     *                                                                                             *
     * Returns information about generated paths.                                                  *
     * location: Folder of the file.                                                               *
     * dungeon: Paths will be generated for this dungeon.                                          *
     * enemies: List of enemies. Needed for enemy links.                                           *
     *                                                                                             *
     ***********************************************************************************************/

    public static EncounterData GeneratePaths(string location, string dungeon, List<Enemy> enemies)
    {
      var rawDataLocation = location + dungeon + ".txt";
      string[] lines;
      if (File.Exists(rawDataLocation))
        lines = File.ReadAllLines(rawDataLocation, Constants.Encoding);
      else
      {
        Helper.ShowWarningMessage("File " + rawDataLocation + " doesn't exist!");
        return null;
      }
      var encounterData = new EncounterData();
      var currentEncounter = new Encounter();
      Helper.CurrentFile = rawDataLocation;
      for (var row = 0; row < lines.Length; row++)
      {
        Helper.InitializeWarningSystem(row + 1, lines[row]);
        HandleLine(lines[row], ref currentEncounter, encounterData.Encounters, encounterData.Paths);
      }
      if (!currentEncounter.Name.Equals(""))
        encounterData.Encounters.Add(currentEncounter);

      Helper.InitializeWarningSystem(-1, "");

      
      if (encounterData.Paths.Count == 0)
        encounterData.Paths.Add(new PathData(dungeon));
      // Set up unique indexes. These are used for tactics (tab system). / 2015-08-11 / Wethospu
      for (var i = 0; i < encounterData.Encounters.Count; i++)
        encounterData.Encounters[i].Index = i + Constants.UniqueIndexCounter;
      Constants.UniqueIndexCounter += encounterData.Encounters.Count;
      return encounterData;
    }

    /***********************************************************************************************
     * HandleLine / 2014-08-01 / Wethospu                                                          *
     *                                                                                             *
     * Processes one line of base data.                                                            *
     *                                                                                             *
     * line: Line to process.                                                                      *
     * currentEncounter: Input/Output. Data of encounter being processed.                          *
     * encounters: Output. List of processed encounters.                                           *
     * paths: Output. Information about generated paths.                                           *
     *                                                                                             *
     ***********************************************************************************************/

    private static string _currentPath = "";
    private static void HandleLine(string line, ref Encounter currentEncounter, List<Encounter> encounters,  List<PathData> paths)
    {
      // Empty line or comment: continue
      if (line == "" || line[0] == '#')
        return;
      if (string.IsNullOrWhiteSpace(line))
      {
        Helper.ShowWarning("Line contains only whitespace (ignored). Please remove!");
        return;
      }
      // Check for weird characters but allow "|" to skip next line.
      if (!char.IsLetterOrDigit(line[0]) && line[0] != '|' && line[0] != '~')
      {
        Helper.ShowWarning("Line starts with a weird character. Please remove!");
        return;
      }
      if (line[0] == ' ')
        Helper.ShowWarning("Extra space detected. Please remove!");

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
      {
        paths.Add(new PathData(line));
      }
      else if (tag.Equals("path"))
      {
        if (data.Length > 0)
        {
          if (data.Contains(" "))
          {
            Helper.ShowWarning("' ' found. Use syntax \"path='path1'|'path2'|'pathN'\"");
            data = data.Replace(' ', '|');
          }
          _currentPath = data.ToLower();
        }
        else
          Helper.ShowWarning("Missing info. Use \"path='path1'|'path2'|'pathN'\"!");
      }
      else if (tag.Equals("name"))
      {
        if (data.Length > 0)
        {
          data = LinkGenerator.CheckLinkSyntax(data);
          if (!currentEncounter.Name.Equals(""))
          {
            encounters.Add(currentEncounter);
            currentEncounter = new Encounter();
          }
          currentEncounter.Name = data;
          currentEncounter.Path = _currentPath;
        }
        else
          Helper.ShowWarning("Missing info. Use \"name='name'\"!");
      }
      else if (tag.Equals("image"))
      {
        if (data.Length > 0)
        {
          currentEncounter.Medias.Add(new Media(data));
        }
        else
          Helper.ShowWarning("Missing info. Use \"image='image'\"!");
      }
      else if (tag.Equals("tactic"))
      {
        if (data.Length > 0)
        {
          var split = new List<string>(data.Split('|'));
          var scales = "";
          // Check is the last tactic scale information. / 2015-10-29 / Wethospu
          var useless = 0;
          var subSplit = split[split.Count - 1].Split('-');
          if (int.TryParse(subSplit[0], out useless) && (subSplit.Length == 1 || int.TryParse(subSplit[1], out useless)))
          {
            scales = split[split.Count - 1];
            split.RemoveAt(split.Count - 1);
          }
          var name = String.Join("|", split);
          currentEncounter.Tactics.AddTactics(name, scales, paths);
        }
          
        else
          Helper.ShowWarning("Missing info. Use \"tactic='tactic1'|'tactic2'|'tacticN'\"!");
      }
      // Normal content.
      else
      {
        // Preprocess the line to avoid doing same stuff 25+ times.
        line = LinkGenerator.CreatePageLinks(LinkGenerator.CheckLinkSyntax(line));
        if (currentEncounter.Tactics.Count == 0)
          currentEncounter.Tactics.AddTactics("normal", "", paths);
        currentEncounter.Tactics.AddLine(line);
      }
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
    public static void GenerateFiles(List<PathData> paths, List<Encounter> encounters, List<Enemy> enemies, List<PathData> navPaths)
    {
      foreach (var dungeonPath in paths)
      {
        // Generate navigation bar.
        var navigation = GenerateNavigationInfo(dungeonPath, navPaths);
        var counter = 0;
        var encounterFile = new StringBuilder();
        encounterFile.Append("GUIDE:").Append(Helper.ConvertSpecial(dungeonPath.InstanceName)).Append(Constants.Delimiter).Append(Helper.ConvertSpecial(dungeonPath.Name)).Append(Constants.ForcedLineEnding);
        encounterFile.Append(navigation);
        encounterFile.Append(Constants.InitialdataHtml);
        foreach (var encr in encounters)
        {
          if (encr.Path.ToUpper().Contains(dungeonPath.Tag.ToUpper()))
          {
            encounterFile.Append(encr.ToHtml(dungeonPath, encounters, counter, enemies));
            counter++;
          }
        }
        var fileName = Constants.DataOutput + Constants.DataEncounterResult + dungeonPath.Filename.ToLower() + ".htm";
        var dirName = Path.GetDirectoryName(fileName);
        if (dirName != null)
          Directory.CreateDirectory(dirName);
        try
        {
          File.WriteAllText(fileName, encounterFile.ToString());
        }
        catch (UnauthorizedAccessException)
        {
          Helper.ShowWarningMessage("File " + fileName + " in use.");
        }
      }
    }

    /***********************************************************************************************
     * GenerateNavigation / 2014-08-01 / Wethospu                                                  *
     *                                                                                             *
     * Generates html representation for path navigation based on path data.                       *
     *                                                                                             *
     * Returns created html.                                                                       *
     * paths: Information about generated paths.                                                   *
     *                                                                                             *
     ***********************************************************************************************/

    private static string GenerateNavigationInfo(PathData path, List<PathData> paths)
    {
      var names = new List<string>();
      var links = new List<string>();
      if (path.Scale == 0)
      {
        // Dungeon navigation (paths within same dungeon). / 2015-10-19 / Wethospu
        foreach (var dungeonPath in paths)
        {
          names.Add(dungeonPath.Name);
          links.Add(dungeonPath.Filename);
        }
      }
      else
      {
        // Fractal navigation (adjacent paths). / 2015-10-19 / Wethospu
        var start = path.Scale - 3;   
        if (start + 7 >= paths.Count)
          start = paths.Count - 7;
        if (start < 1)
          start = 1;
        for (var i = 0; i < 7; i++)
        {
          if (path.Scale == start + i || start + i >= paths.Count)
            continue;
          names.Add("Scale " + paths[start + i].Scale + ": " + paths[start + i].Name);
          links.Add(paths[start + i].Filename);
        }
      }
      return string.Join("|", names) + Constants.ForcedLineEnding + string.Join("|", links) + Constants.ForcedLineEnding;
    }
  }
}
