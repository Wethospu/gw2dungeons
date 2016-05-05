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
        line = line.Replace("PATH_ICON", Constants.WebsiteIconLocation);
        line = line.Replace("PATH_MEDIA", Constants.WebsiteMediaLocation);
        line = line.Replace("PATH_THUMB_BIG", Constants.WebsiteThumbBigLocation);
        line = line.Replace("PATH_THUMB_SMALL", Constants.WebsiteThumbSmallLocation);
        toSave.Append(line).Append(Constants.ForcedLineEnding);
      }
      // Save file.
      File.WriteAllText(target, toSave.ToString());
    }

    /// <summary>
    /// Replaces string ID_DATE with the current date for a given file.
    /// </summary>
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
        // Ignore comments on release mode to reduce file size.
        if (Constants.IsRelease && line.StartsWith("//"))
          continue;
        // Ignore empty lines to reduce file size.
        if (line == "")
          continue;
        ErrorHandler.InitializeWarningSystem(row + 1, line);
        // Ignore whitespace to reduce file size.
        if (string.IsNullOrWhiteSpace(line))
          continue;
        // Apply dictionary.
        line = line.Replace("ID_DATE", DateTime.Today.ToString("yyyy-MM-dd"));
        toSave.Append(line).Append(Constants.ForcedLineEnding);
      }
      // Save file.
      File.WriteAllText(fileName, toSave.ToString());
    }

    /// <summary>
    /// Replaces strings ID_CATEGORIES, ID_PATHS, ID_TAGS and ID_RACES with relevant html.
    /// </summary>
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
        // Ignore comments on release mode to reduce file size.
        if (Constants.IsRelease && line.StartsWith("//"))
          continue;
        // Ignore empty lines to reduce file size.
        if (line == "")
          continue;
        ErrorHandler.InitializeWarningSystem(row + 1, line);
        // Ignore whitespace to reduce file size.
        if (string.IsNullOrWhiteSpace(line))
          continue;
        line = line.Replace("ID_PATHS", dungeonData.GenerateInstanceHtml());
        line = line.Replace("ID_RACES", dungeonData.GenerateRaceHtml());
        line = line.Replace("ID_RANKS", dungeonData.GenerateRankHtml());
        line = line.Replace("ID_TAGS", dungeonData.GenerateTagHtml());
        line = line.Replace("ID_EFFECT_TAGS", dungeonData.GenerateEffectTagHtml());
        line = line.Replace("PATH_ICON", Constants.WebsiteIconLocation);
        line = line.Replace("PATH_MEDIA", Constants.WebsiteMediaLocation);
        line = line.Replace("PATH_THUMB_BIG", Constants.WebsiteThumbBigLocation);
        line = line.Replace("PATH_THUMB_SMALL", Constants.WebsiteThumbSmallLocation);
        toSave.Append(line).Append(Constants.ForcedLineEnding);
      }
      // Save file.
      File.WriteAllText(fileName, toSave.ToString());
    }

    /// <summary>
    /// Copies data files to the output folder while merging some files.
    /// </summary>
    private static void MergeFiles()
    {
      var files = Directory.GetFiles(Constants.DataOtherRaw, "*", SearchOption.AllDirectories);
      var jsBuilder = new StringBuilder();
      var cssBuilder = new StringBuilder();
      // Check which file has been modified last and use it for the merged file.
      var jsLastTime = 0L;
      var cssLastTime = 0L;
      foreach (var file in files)
      {
        // Don't copy media files since it just slows down the process for no reason.
        if (file.Contains(Constants.LocalMediaFolder) || file.Contains(Constants.LocalIconFolder))
          continue;
        var outputFile = Constants.DataOutput + file.Replace(Constants.DataOtherRaw, "");
        var text = File.ReadAllText(file);
        text = text.Replace("PATH_ICON", Constants.WebsiteIconLocation);
        text = text.Replace("PATH_MEDIA", Constants.WebsiteMediaLocation);
        text = text.Replace("PATH_THUMB_BIG", Constants.WebsiteThumbBigLocation);
        text = text.Replace("PATH_THUMB_SMALL", Constants.WebsiteThumbSmallLocation);
        if (Constants.IsRelease && Path.GetExtension(file) == ".js" && !Path.GetFileNameWithoutExtension(file).Equals("html5shiv"))
        {
          jsBuilder.Append(text);
          if (File.GetLastWriteTimeUtc(file).Ticks > jsLastTime)
            jsLastTime = File.GetLastWriteTimeUtc(file).Ticks;
          continue;
        }
        if (Constants.IsRelease && Path.GetExtension(file) == ".css")
        {
          cssBuilder.Append(text);
          if (File.GetLastWriteTimeUtc(file).Ticks > cssLastTime)
            cssLastTime = File.GetLastWriteTimeUtc(file).Ticks;
          continue;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        File.WriteAllText(outputFile, text);
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

    /// <summary>
    /// Replaces ID_JS and ID_CSS with the correct includes. Includes depend on the release mode.
    /// </summary>
    private static void ApplyIncludes(string file)
    {
      var fileName = Constants.DataOutput + file.Replace(Constants.DataOtherRaw, "");
      var dirName = Path.GetDirectoryName(fileName);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      File.WriteAllText(fileName, File.ReadAllText(file, Constants.Encoding).Replace("ID_JS", Constants.JSFiles).Replace("ID_CSS", Constants.CSSFiles));
    }
  }
}
