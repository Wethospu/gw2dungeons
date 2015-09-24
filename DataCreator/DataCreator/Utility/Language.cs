using System;
using System.Collections.Generic;
using System.IO;

namespace DataCreator.Utility
{

  /***********************************************************************************************
   * Language / 2014-08-01 / Wethospu                                                            *
   *                                                                                             *
   * Language specific stuff.                                                                    *
   *                                                                                             *
   ***********************************************************************************************/

  public static class Language
  {
    /***********************************************************************************************
     * ReadLanguageData / 2014-08-01 / Wethosp                                                     *
     *                                                                                             *
     * Reads language info.                                                                        *
     *                                                                                             *
     * Returns dictionary of language data.                                                        *
     * language: Language data to read.                                                            *
     *                                                                                             *
     ***********************************************************************************************/

    public static Dictionary<string, string> ReadLanguageData(string language)
    {
      var dataLocation = Constants.DataLanguageRaw + "language";
      if (!language.Equals("en"))
        dataLocation += "_" + language;
      dataLocation += ".txt";
      string[] lines;
      if (File.Exists(dataLocation))
        lines = File.ReadAllLines(dataLocation);
      else
      {
        Helper.ShowWarningMessage("Language file not found in '" + dataLocation + "'!");
        return null;
      }
      if (lines.Length == 0)
      {
        Helper.ShowWarningMessage("Language file was empty! ('" + dataLocation + "')!");
        return null;
      }
      Helper.CurrentFile = dataLocation;
      var languageData = new Dictionary<string, string>();

      for (var row = 0; row < lines.Length; row++)
      {
        var line = lines[row];
        // Empty line or comment: continue
        if (line == "" || line[0] == '#')
          continue;
        Helper.InitializeWarningSystem(row + 1, line);
        if (string.IsNullOrWhiteSpace(line))
        {
          Helper.ShowWarning("Line contains only whitespace (ignored). Please remove extra characters!");
          continue;
        }
        var separator = line.IndexOf(Constants.TagSeparator);
        if (separator < 0 || separator >= line.Length)
        {
          Helper.ShowWarning("Incorrect syntax. Use \"'tag'='data'\"");
          continue;
        }
        var tag = line.Substring(0, separator).Trim();
        var data = line.Substring(separator + 1).Trim();
        if (tag.Length + data.Length + 1 != line.Length)
          Helper.ShowWarning("\"" + line + "\" contains leading or trailing whitespace. Please remove!");

        // Check characters.
        for (var i = 3; i < tag.Length; i++)
        {
          int useless;
          if (!Char.IsUpper(tag[i]) && tag[i] != '_' && !int.TryParse("" + tag[i], out useless))
            Helper.ShowWarning("ID \"" + tag + "\" contains lowercase character '" + tag[i] + "'. Please fix!");
        }
        // Insert data.
        if (languageData.ContainsKey("ID_" + tag))
        {
          Helper.ShowWarning("Duplicate ID \"" + tag + "\". Please rename!");
          languageData["ID_" + tag] = data;
        }
        else
          languageData.Add("ID_" + tag, data);
      }
      // Dictionary generated.
      return languageData;
    }
  }
}
