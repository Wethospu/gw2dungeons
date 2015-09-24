using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataCreator.Utility;

namespace DataCreator.Shared
{

  /***********************************************************************************************
   * OtherGenerator / 2014-08-01 / Wethospu                                                       *
   *                                                                                             *
   * Functions to generate other pages like main page, builds, css, js, etc.                     *
   *                                                                                             *
   ***********************************************************************************************/

  static class OtherGenerator
  {
    /***********************************************************************************************
     * GenerateOthers / 2014-08-01 / Wethospu                                                      *
     *                                                                                             *
     * Generates main page and other files.                                                        *
     *                                                                                             *
     * pathData: Information about generated paths.                                                *
     *                                                                                             *
     ***********************************************************************************************/

    public static void GenerateOthers(DataCollector dungeonData)
    {
      Console.WriteLine("Generating pages");
      // Generate main page to dynamically create the dungeon listing.
      ApplyDungeonData(Constants.DataOtherRaw + "pages\\home.htm", dungeonData.GenerateDungeonData());
      // Generate about page to dynamically update date of the last update.
      ApplyDate(Constants.DataOtherRaw + "pages\\about.htm");
      // Generate search page filters based on actual enemies. / 2015-08-14 / Wethospu
      ApplyData(Constants.DataOtherRaw + "pages\\search.htm", dungeonData);
    }

    /***********************************************************************************************
     * ApplyDungeonData / 2015-07-06 / Wethospu                                                    *
     *                                                                                             *
     * Replaces dungeon IDs with metadata from the content pages.                                  *
     * Used to generate the main page "dynamically".                                               *
     *                                                                                             *
     * file: Location of the base file.                                                            *
     * dungeonData: Dictionary for replacing.                                                      *
     *                                                                                             *
     ***********************************************************************************************/

    private static void ApplyDungeonData(string file, Dictionary<string, string> dungeonData)
    {
      var fileName = Constants.DataOutput + file.Replace(Constants.DataOtherRaw, "");
      var dirName = Path.GetDirectoryName(fileName);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      var lines = File.ReadAllLines(file, Constants.Encoding);
      Helper.CurrentFile = file;
      var toSave = new StringBuilder();
      toSave.Append(Constants.InitialdataHtml);
      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        // Ignore empty lines.
        if (line == "")
          continue;
        Helper.InitializeWarningSystem(row + 1, line);
        // Ignore whitespace.
        if (string.IsNullOrWhiteSpace(line))
          continue;
        // Apply dungeon data.
        for (var index = 0; index < line.Length; )
        {
          var startIndex = line.IndexOf("DUNGEON_", index, StringComparison.Ordinal);
          if (startIndex == -1)
            break;
          var endIndex = startIndex;
          // Find proper ending.
          for (; endIndex < line.Length; endIndex++)
          {
            int useless;
            if (Char.IsUpper(line[endIndex]) || line[endIndex] == '_' || int.TryParse("" + line[endIndex], out useless))
              continue;
            break;
          }
          index = endIndex;
          var id = line.Substring(startIndex, endIndex - startIndex);
          if (dungeonData.ContainsKey(id))
          {
            // Use length to position index properly.
            index -= line.Length;
            line = line.Replace(id, dungeonData[id]);
            index += line.Length;
          }
          else
            Helper.ShowWarning("Data for dungeon \"" + id + "\" not found!");
        }
        toSave.Append(line).Append(Constants.LineEnding);
      }
      // Save file.
      File.WriteAllText(fileName, toSave.ToString());
    }

    /***********************************************************************************************
     * ApplyDate / 2015-07-02 / Wethospu                                                           *
     *                                                                                             *
     * Replaces ID_DATE with the current date.                                                     *
     *                                                                                             *
     * file: File to process.                                                                      *
     *                                                                                             *
     ***********************************************************************************************/

    private static void ApplyDate(string file)
    {
      var fileName = Constants.DataOutput + file.Replace(Constants.DataOtherRaw, "");
      var dirName = Path.GetDirectoryName(fileName);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      var lines = File.ReadAllLines(file, Constants.Encoding);
      Helper.CurrentFile = file;
      var toSave = new StringBuilder();
      toSave.Append(Constants.InitialdataHtml);
      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        // Ignore empty lines.
        if (line == "")
          continue;
        Helper.InitializeWarningSystem(row + 1, line);
        // Ignore whitespace.
        if (string.IsNullOrWhiteSpace(line))
          continue;
        // Apply dictionary.
        line = line.Replace("ID_DATE", DateTime.Today.ToString().Split(' ')[0]);
        toSave.Append(line).Append(Constants.LineEnding);
      }
      // Save file.
      File.WriteAllText(fileName, toSave.ToString());
    }

    /***********************************************************************************************
     * ApplyData / 2015-08-14 / Wethospu                                                           *
     *                                                                                             *
     * Replaces ID_CATEGORIES, ID_PATHS, ID_TAGS and ID_RACES with relevant html.                  *
     *                                                                                             *
     ***********************************************************************************************/

    private static void ApplyData(string file, DataCollector dungeonData)
    {
      var fileName = Constants.DataOutput + file.Replace(Constants.DataOtherRaw, "");
      var dirName = Path.GetDirectoryName(fileName);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      var lines = File.ReadAllLines(file, Constants.Encoding);
      Helper.CurrentFile = file;
      var toSave = new StringBuilder();
      toSave.Append(Constants.InitialdataHtml);
      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        // Ignore empty lines.
        if (line == "")
          continue;
        Helper.InitializeWarningSystem(row + 1, line);
        // Ignore whitespace.
        if (string.IsNullOrWhiteSpace(line))
          continue;
        if (line.Contains("ID_PATHS"))
          line = line.Replace("ID_PATHS", dungeonData.GenerateDungeonHtml());
        if (line.Contains("ID_RACES"))
          line = line.Replace("ID_RACES", dungeonData.GenerateRaceHtml());
        if (line.Contains("ID_CATEGORIES"))
          line = line.Replace("ID_CATEGORIES", dungeonData.GenerateCategoryHtml());
        if (line.Contains("ID_TAGS"))
          line = line.Replace("ID_TAGS", dungeonData.GenerateTagHtml());
        if (line.Contains("ID_EFFECT_TAGS"))
          line = line.Replace("ID_EFFECT_TAGS", dungeonData.GenerateEffectTagHtml());
        toSave.Append(line).Append(Constants.LineEnding);
      }
      // Save file.
      File.WriteAllText(fileName, toSave.ToString());
    }
  }
}
