using DataCreator.Enemies;
using DataCreator.Utility;
using System;
using System.IO;
using System.Text;

namespace DataCreator
{
  /// <summary>
  /// Loads and saves settings/cache files.
  /// </summary>
  public static class Settings
  {
    /// <summary>
    /// Loads settings and pre-defined values.
    /// </summary>
    // Very small at the moment because most settings are hardcoded in Constants.
    // Feel free to expand if needed. Some sort of json might work nicely.
    public static void ReadSettings()
    {
      // Pre-defined tactic names to catch errors.
      try
      {
        Console.WriteLine("Reading available tactics.");
        var lines = File.ReadAllLines(@"AvailableTactics.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0)
          ErrorHandler.ShowWarningMessage("Tactic file is empty!");
        Constants.AvailableTactics.Clear();
        foreach (var str in lines)
        {
          if (str == "" || str[0] == '#')
            continue;
          Constants.AvailableTactics.Add(str.ToLower());
        }
      }
      catch (FileNotFoundException)
      {
        ErrorHandler.ShowWarningMessage("File 'AvailableTactics.txt' not found!");
      }
      // Pre-defined tip names for catching errors.
      try
      {
        Console.WriteLine("Reading available tips.");
        var lines = File.ReadAllLines(@"AvailableTips.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0)
          ErrorHandler.ShowWarningMessage("Tip file is empty!");
        Constants.AvailableTips.Clear();
        foreach (var str in lines)
        {
          if (str == "" || str[0] == '#')
            continue;
          Constants.AvailableTips.Add(str.ToLower());
        }
      }
      catch (FileNotFoundException)
      {
        ErrorHandler.ShowWarningMessage("File 'AvailableTips.txt' not found!");
      }
      // Conversions for special characters to allow using them on html.
      try
      {
        Console.WriteLine("Reading special character conversions.");
        var lines = File.ReadAllLines(@"SpecialCharacters.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0)
          ErrorHandler.ShowWarningMessage("Conversion file is empty!");
        Constants.CharacterConversions.Clear();
        Constants.CharacterSimplifications.Clear();
        foreach (var str in lines)
        {
          if (str == "" || str[0] == '#')
            continue;
          var split = str.Split('=');
          if (split.Length != 3)
          {
            ErrorHandler.ShowWarningMessage("Line " + str + " is wrong. Use syntax 'character'='html value'='simplified'.");
            continue;
          }
          Constants.CharacterConversions.Add(split[0], split[1]);
          Constants.CharacterSimplifications.Add(split[0], split[2]);
        }
      }
      catch (FileNotFoundException)
      {
        ErrorHandler.ShowWarningMessage("File 'SpecialCharacters.txt' not found!");
      }
    }

    // Some functionality is really slow (requires internet connection).
    /// <summary>
    /// Use cache files to skip already done parts.
    /// </summary>
    public static void ReadCacheFiles()
    {
      SubEffect.CreateEffectTypes();
      try
      {
        Console.WriteLine("Reading media sizes.");
        var lines = File.ReadAllLines(@"MediaSizes.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0 && !Constants.DownloadData)
        {
          ErrorHandler.ShowWarningMessage("Media size file is empty. Consider using parameter back.");
        }
        Constants.MediaSizes.Clear();
        foreach (var str in lines)
        {
          if (str == "" || str[0] == '#')
            continue;
          var split = str.Split('|');
          if (split.Length != 3)
          {
            if (!Constants.DownloadData)
              ErrorHandler.ShowWarningMessage("Media size file corrupted. Consider using backuping media files.");
            continue;
          }
          int height, width;
          if (!int.TryParse(split[1], out width))
          {
            if (!Constants.DownloadData)
              ErrorHandler.ShowWarningMessage("Media size file corrupted. Consider using backuping media files.");
            continue;
          }
          if (!int.TryParse(split[2], out height))
          {
            if (!Constants.DownloadData)
              ErrorHandler.ShowWarningMessage("Media size file corrupted. Consider using backuping media files.");
            continue;
          }
          Constants.MediaSizes.Add(split[0], new int[] { width, height });
        }
      }
      catch (FileNotFoundException)
      {
        ErrorHandler.ShowWarningMessage("File 'MediaSizes.txt' not found!");
      }
      if (Constants.ValidateUrls)
      {
        // File with list of verified links.
        try
        {
          Console.WriteLine("Reading known urls.");
          var lines = File.ReadAllLines(@"ValidatedUrls.txt", Encoding.GetEncoding(1252));
          Constants.ValidatedUrls.Clear();
          foreach (var str in lines)
          {
            if (str == "" || str[0] == '#')
              continue;
            Constants.ValidatedUrls.Add(str);
          }
        }
        catch (FileNotFoundException)
        {
          ErrorHandler.ShowWarningMessage("File 'ValidatedUrls.txt' not found!");
        }
      }
    }

    /// <summary>
    /// Store results of slower operations so they can be skipped next time.
    /// </summary>
    public static void SaveCacheFiles()
    {
      if (Constants.DownloadData)
      {
        var toWrite = new StringBuilder();
        foreach (var key in Constants.MediaSizes.Keys)
        {
          toWrite.Append(key).Append("|").Append(Constants.MediaSizes[key][0]).Append("|").Append(Constants.MediaSizes[key][1]).Append('\n');
        }
        try
        {
          File.WriteAllText(@"MediaSizes.txt", toWrite.ToString());
        }
        catch (UnauthorizedAccessException)
        {
          ErrorHandler.ShowWarningMessage("File MediaSizes.txt in use.");
        }
      }
      if (Constants.ValidateUrls)
      {
        var toWrite = new StringBuilder();
        foreach (var key in Constants.ValidatedUrls)
        {
          toWrite.Append(key).Append('\n');
        }
        try
        {
          File.WriteAllText(@"ValidatedUrls.txt", toWrite.ToString());
        }
        catch (UnauthorizedAccessException)
        {
          ErrorHandler.ShowWarningMessage("File ValidatedUrls.txt in use.");
        }
      }
    }
  }
}
