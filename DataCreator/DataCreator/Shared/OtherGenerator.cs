using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataCreator.Utility;

namespace DataCreator.Shared
{
  /// <summary>
  /// Functions to generate non-instance files.
  /// </summary>
  public static class OtherGenerator
  {
    /// <summary>
    /// Main function to generate everything.
    /// </summary>
    public static void GenerateOthers(DataCollector dungeonData)
    {
      Console.WriteLine("Generating pages");
      MergeFiles();
      // Generate main page to dynamically create the dungeon listing.
      GenerateMainPage(dungeonData.GenerateInstanceData());
      // Generate about page to dynamically update date of the last update.
      ApplyDate(Constants.DataOtherRaw + "pages\\about.htm");
      // Generate search page filters based on actual enemies.
      ApplyData(Constants.DataOtherRaw + "pages\\search.htm", dungeonData);
      // Generate includes based on release mode.
      ApplyIncludes(Constants.DataOtherRaw + "index.php");
    }

    /// <summary>
    /// Generates an instance list for the main page.
    /// </summary>
    private static void GenerateMainPage(Dictionary<string, string> instanceData)
    {
      var source = Constants.DataOtherRaw + "pages\\home.htm";
      var target = Constants.DataOutput + source.Replace(Constants.DataOtherRaw, "");
      var dirName = Path.GetDirectoryName(target);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      var lines = File.ReadAllLines(source, Constants.Encoding);
      ErrorHandler.CurrentFile = source;
      var toSave = new StringBuilder();
      toSave.Append(Constants.InitialdataHtml);
      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        if (Constants.IsRelease)
          line = line.Trim(new char[] { '\t', ' ' });
        // Ignore comments on release mode to reduce size file.
        if (Constants.IsRelease && line.StartsWith("//"))
          continue;
        if (line == "")
          continue;
        ErrorHandler.InitializeWarningSystem(row + 1, line);
        if (string.IsNullOrWhiteSpace(line))
          continue;
        for (var index = 0; index < line.Length; )
        {
          var startIndex = line.IndexOf("ID_", index, StringComparison.Ordinal);
          if (startIndex == -1)
            break;
          var endIndex = startIndex;
          // Find proper ending.
          for (; endIndex < line.Length; endIndex++)
          {
            int useless;
            if (char.IsUpper(line[endIndex]) || line[endIndex] == '_' || int.TryParse("" + line[endIndex], out useless))
              continue;
            break;
          }
          index = endIndex;
          var id = line.Substring(startIndex, endIndex - startIndex);
          if (instanceData.ContainsKey(id))
          {
            // Use length to position index properly.
            index -= line.Length;
            line = line.Replace(id, instanceData[id]);
            index += line.Length;
          }
          else
            ErrorHandler.ShowWarning("Data for dungeon \"" + id + "\" not found!");
        }
        toSave.Append(line).Append(Constants.ForcedLineEnding);
      }
      // Save file.
      File.WriteAllText(target, toSave.ToString());
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
      ErrorHandler.CurrentFile = file;
      var toSave = new StringBuilder();
      toSave.Append(Constants.InitialdataHtml);
      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        if (Constants.IsRelease)
          line = line.Trim(new char[] { '\t', ' ' });
        // Ignore comments on release mode. / 2015-10-10 / Wethospu
        if (Constants.IsRelease && line.StartsWith("//"))
          continue;
        // Ignore empty lines.
        if (line == "")
          continue;
        ErrorHandler.InitializeWarningSystem(row + 1, line);
        // Ignore whitespace.
        if (string.IsNullOrWhiteSpace(line))
          continue;
        // Apply dictionary.
        line = line.Replace("ID_DATE", DateTime.Today.ToString("yyyy-MM-dd"));
        toSave.Append(line).Append(Constants.ForcedLineEnding);
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
      ErrorHandler.CurrentFile = file;
      var toSave = new StringBuilder();
      toSave.Append(Constants.InitialdataHtml);
      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        if (Constants.IsRelease)
          line = line.Trim(new char[] { '\t', ' ' });
        // Ignore comments on release mode. / 2015-10-10 / Wethospu
        if (Constants.IsRelease && line.StartsWith("//"))
          continue;
        // Ignore empty lines.
        if (line == "")
          continue;
        ErrorHandler.InitializeWarningSystem(row + 1, line);
        // Ignore whitespace.
        if (string.IsNullOrWhiteSpace(line))
          continue;
        if (line.Contains("ID_PATHS"))
          line = line.Replace("ID_PATHS", dungeonData.GenerateInstanceHtml());
        if (line.Contains("ID_RACES"))
          line = line.Replace("ID_RACES", dungeonData.GenerateRaceHtml());
        if (line.Contains("ID_RANKS"))
          line = line.Replace("ID_RANKS", dungeonData.GenerateRankHtml());
        if (line.Contains("ID_TAGS"))
          line = line.Replace("ID_TAGS", dungeonData.GenerateTagHtml());
        if (line.Contains("ID_EFFECT_TAGS"))
          line = line.Replace("ID_EFFECT_TAGS", dungeonData.GenerateEffectTagHtml());
        toSave.Append(line).Append(Constants.ForcedLineEnding);
      }
      // Save file.
      File.WriteAllText(fileName, toSave.ToString());
    }

    /***********************************************************************************************
    * MergeFiles / 2015-09-25 / Wethospu                                                           *
    *                                                                                              *
    * Copies data files to the output folder while merging some files together.                    *
    *                                                                                              *
    ***********************************************************************************************/

    private static void MergeFiles()
    {
      var files = Directory.GetFiles(Constants.DataOtherRaw, "*", SearchOption.AllDirectories);
      var jsBuilder = new StringBuilder();
      var cssBuilder = new StringBuilder();
      // Check which file has modified last and use it for the merged file. / 2015-10-19 / Wethospu
      var jsLastTime = 0L;
      var cssLastTime = 0L;
      foreach (var file in files)
      {
        var outputFile = Constants.DataOutput + file.Replace(Constants.DataOtherRaw, "");
        if (Constants.IsRelease && Path.GetExtension(file) == ".js" && !Path.GetFileNameWithoutExtension(file).Equals("html5shiv"))
        {
          jsBuilder.Append(File.ReadAllText(file));
          if (File.GetLastWriteTimeUtc(file).Ticks > jsLastTime)
            jsLastTime = File.GetLastWriteTimeUtc(file).Ticks;
          continue;
        }
        if (Constants.IsRelease && Path.GetExtension(file) == ".css")
        {
          cssBuilder.Append(File.ReadAllText(file));
          if (File.GetLastWriteTimeUtc(file).Ticks > cssLastTime)
            cssLastTime = File.GetLastWriteTimeUtc(file).Ticks;
          continue;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        File.Copy(file, outputFile, true);
      }
      if (Constants.IsRelease)
      {
        /*using (var client = new HttpClient())
        {
          var values = new Dictionary<string, string>
          {
            { "input", jsBuilder.ToString() }
          };
          var jsonString = JsonConvert.SerializeObject(values);
          var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

          var response = await client.PostAsync("http://javascript-minifier.com/raw", content);

          var responseString = await response.Content.ReadAsStringAsync();
          File.WriteAllText(Constants.DataOutput + Constants.DataMediaResult + "gw2dungeons.js", responseString);
        }*/
        File.WriteAllText(Constants.DataOutput + Constants.DataMediaResult + "gw2dungeons.js", jsBuilder.ToString());
        Constants.JSFiles = Constants.JSFiles.Replace(".js", ".js?" + jsLastTime);
        /*using (var client = new HttpClient())
        {
          var values = new Dictionary<string, string>
          {
            { "input", cssBuilder.ToString() }
          };
          var jsonString = JsonConvert.SerializeObject(values);
          var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

          var response = await client.PostAsync("http://cssminifier.com/raw", content);

          var responseString = await response.Content.ReadAsStringAsync();
          File.WriteAllText(Constants.DataOutput + Constants.DataMediaResult + "gw2dungeons.css", responseString);
        }*/
        File.WriteAllText(Constants.DataOutput + Constants.DataMediaResult + "gw2dungeons.css", cssBuilder.ToString());
        Constants.CSSFiles = Constants.CSSFiles.Replace(".css", ".css?" + cssLastTime);
      }
    }

    /***********************************************************************************************
    * ApplyIncludes / 2015-09-25 / Wethospu                                                        *
    *                                                                                              *
    * Replaces ID_JS AND ID_CSS with the correct includes.                                         *
    *                                                                                              *
    * file: File to process.                                                                       *
    *                                                                                              *
    ***********************************************************************************************/

    private static void ApplyIncludes(string file)
    {
      var fileName = Constants.DataOutput + file.Replace(Constants.DataOtherRaw, "");
      var dirName = Path.GetDirectoryName(fileName);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      // Save file.
      File.WriteAllText(fileName, File.ReadAllText(file, Constants.Encoding).Replace("ID_JS", Constants.JSFiles).Replace("ID_CSS", Constants.CSSFiles));
    }
  }
}
