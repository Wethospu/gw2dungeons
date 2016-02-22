using System.Collections.Generic;
using System.IO;

namespace DataCreator.Utility
{
  /// <summary>
  /// Translation related functions. Not in use at the moment.
  /// </summary>
  public static class Language
  {
    /// <summary>
    /// Reads translation data. Returns a dictionary which converts "ID_*" strings based on the chosen language.
    /// </summary>
    public static Dictionary<string, string> ReadTranslations(string language)
    {
      var dataLocation = Constants.DataRaw + "language";
      // English is the default language so it won't get a folder.
      if (!language.Equals("en"))
        dataLocation += "_" + language;
      dataLocation += ".txt";
      string[] lines;
      if (File.Exists(dataLocation))
        lines = File.ReadAllLines(dataLocation);
      else
      {
        ErrorHandler.ShowWarningMessage("Language file not found in '" + dataLocation + "'!");
        return null;
      }
      if (lines.Length == 0)
      {
        ErrorHandler.ShowWarningMessage("Language file was empty! ('" + dataLocation + "')!");
        return null;
      }
      ErrorHandler.CurrentFile = dataLocation;
      var languageData = new Dictionary<string, string>();

      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        if (line == "" || line[0] == '#')
          continue;
        ErrorHandler.InitializeWarningSystem(row + 1, line);
        if (string.IsNullOrWhiteSpace(line))
        {
          ErrorHandler.ShowWarning("Line contains only whitespace (ignored). Please remove extra characters!");
          continue;
        }
        var separator = line.IndexOf(Constants.TagSeparator);
        if (separator < 0 || separator >= line.Length)
        {
          ErrorHandler.ShowWarning("Incorrect syntax. Use \"'tag'='data'\"");
          continue;
        }
        var tag = line.Substring(0, separator).Trim();
        var data = line.Substring(separator + 1).Trim();
        if (tag.Length + data.Length + 1 != line.Length)
          ErrorHandler.ShowWarning("\"" + line + "\" contains leading or trailing whitespace. Please remove!");
        // Verify tag format.
        for (var i = 3; i < tag.Length; i++)
        {
          int useless;
          if (!char.IsUpper(tag[i]) && tag[i] != '_' && !int.TryParse("" + tag[i], out useless))
            ErrorHandler.ShowWarning("ID \"" + tag + "\" contains lowercase character '" + tag[i] + "'. Please fix!");
        }
        if (languageData.ContainsKey("ID_" + tag))
        {
          ErrorHandler.ShowWarning("Duplicate ID \"" + tag + "\". Please rename!");
          languageData["ID_" + tag] = data;
        }
        else
          languageData.Add("ID_" + tag, data);
      }
      return languageData;
    }
  }
}
