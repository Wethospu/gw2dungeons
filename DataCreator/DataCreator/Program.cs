using DataCreator.Encounters;
using DataCreator.Enemies;
using DataCreator.Shared;
using DataCreator.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace DataCreator
{
  /***********************************************************************************************
   * Program / 2014-08-01 / Wethospu                                                             *
   *                                                                                             *
   * Main program. Loads settings and handles console.                                           *
   *                                                                                             *
   ***********************************************************************************************/

  static class Program
  {
    static void Main()
    {
      Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
      Console.OutputEncoding = Constants.Encoding;
      Console.WriteLine(Directory.GetCurrentDirectory());
      Directory.SetCurrentDirectory("..");
      while (true)
      {
        Console.Clear();
        Helper.WarningCount = 0;
        if (Directory.Exists(Constants.DataOutput))
        {
          // Gather old files to remove them. / 2015-09-25 / Wethospu
          var fileList = new List<string>();
          if (Directory.Exists(Constants.DataOutput + Constants.DataEnemyResult))
            fileList.AddRange(Directory.GetFiles(Constants.DataOutput + Constants.DataEnemyResult, "*", SearchOption.AllDirectories));
          if (Directory.Exists(Constants.DataOutput + Constants.DataEncounterResult))
            fileList.AddRange(Directory.GetFiles(Constants.DataOutput + Constants.DataEncounterResult, "*", SearchOption.AllDirectories));
          if (Directory.Exists(Constants.DataOutput + Constants.DataMediaResult))
            fileList.AddRange(Directory.GetFiles(Constants.DataOutput + Constants.DataMediaResult, "*", SearchOption.AllDirectories));
          if (Directory.Exists(Constants.DataOutput))
            fileList.AddRange(Directory.GetFiles(Constants.DataOutput, "*", SearchOption.TopDirectoryOnly));
          Console.WriteLine("Removing " + fileList.Count + " generated data files.");
          Console.WriteLine("\n");
          try
          {
            foreach (var file in fileList)
              File.Delete(file);
            // Also delete folders. / 2015-09-25 / Wethospu
            if (Directory.Exists(Constants.DataOutput + Constants.DataEnemyResult))
              Directory.Delete(Constants.DataOutput + Constants.DataEnemyResult, true);
            if(Directory.Exists(Constants.DataOutput + Constants.DataEncounterResult))
              Directory.Delete(Constants.DataOutput + Constants.DataEncounterResult, true);
            if (Directory.Exists(Constants.DataOutput + Constants.DataMediaResult))
              Directory.Delete(Constants.DataOutput + Constants.DataMediaResult, true);
          }
          catch
          {
            Console.WriteLine("Couldn't remove every output file. Some files may not be updated.");
            Console.WriteLine("");
          }

        }
        if (!Build())
          break;
        var builder = new StringBuilder();
        builder.Append("Generating completed");
        if (Helper.WarningCount > 0)
        {
          builder.Append(Helper.WarningCountMessage());
        }
        builder.Append(".");
        SaveSettings();
        Console.WriteLine(builder.ToString());
        Console.WriteLine("Press any key to exit or Esc to restart.");
        var info = Console.ReadKey();
        if (info.Key == ConsoleKey.Escape)
          continue;
        break;
      }
    }

    /***********************************************************************************************
     * LoadSettings / 2014-08-01 / Wethospu                                                        *
     *                                                                                             *
     * Loads available tactics, special conversions and media sizes.                               *
     *                                                                                             *
     ***********************************************************************************************/

    static void LoadSettings()
    {
      try
      {
        Console.WriteLine("Reading available tactics.");
        var lines = File.ReadAllLines(@"AvailableTactics.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0)
          Helper.ShowWarningMessage("Tactic file is empty!");
        Constants.AvailableTactics.Clear();
        foreach (var str in lines)
        {
          // Empty line or comment: continue
          if (str == "" || str[0] == '#')
            continue;
          Constants.AvailableTactics.Add(str.ToLower());
        }
      }
      catch (FileNotFoundException)
      {
        Helper.ShowWarningMessage("File 'AvailableTactics.txt' not found!");
      }
      try
      {
        Console.WriteLine("Reading available tips.");
        var lines = File.ReadAllLines(@"AvailableTips.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0)
          Helper.ShowWarningMessage("Tip file is empty!");
        Constants.AvailableTips.Clear();
        foreach (var str in lines)
        {
          // Empty line or comment: continue
          if (str == "" || str[0] == '#')
            continue;
          Constants.AvailableTips.Add(str.ToLower());
        }
      }
      catch (FileNotFoundException)
      {
        Helper.ShowWarningMessage("File 'AvailableTips.txt' not found!");
      }
      try
      {
        Console.WriteLine("Reading special character conversions.");
        var lines = File.ReadAllLines(@"SpecialCharacters.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0)
          Helper.ShowWarningMessage("Conversion file is empty!");
        Constants.CharacterConversions.Clear();
        Constants.CharacterSimplifications.Clear();
        foreach (var str in lines)
        {
          // Empty line or comment: continue
          if (str == "" || str[0] == '#')
            continue;
          var split = str.Split('=');
          if (split.Length != 3)
          {
            Helper.ShowWarningMessage("Line " + str + " is wrong. Use syntax 'character'='html value'='simplified'.");
            continue;
          }
          Constants.CharacterConversions.Add(split[0], split[1]);
          Constants.CharacterSimplifications.Add(split[0], split[2]);
        }
      }
      catch (FileNotFoundException)
      {
        Helper.ShowWarningMessage("File 'SpecialCharacters.txt' not found!");
      }
      // Media size file which links url to a width and height.
      // Size is used to resize the overlay to fit the content.
      try
      {
        Console.WriteLine("Reading media sizes.");
        var lines = File.ReadAllLines(@"MediaSizes.txt", Encoding.GetEncoding(1252));
        if (lines.Length == 0 && !Constants.DownloadData)
        {
          Helper.ShowWarningMessage("Media size file is empty. Consider using parameter back.");
        }
        Constants.MediaSizes.Clear();
        foreach (var str in lines)
        {
          // Empty line or comment: continue
          if (str == "" || str[0] == '#')
            continue;
          var split = str.Split('|');
          if (split.Length != 3)
          {
            if (!Constants.DownloadData)
              Helper.ShowWarningMessage("Media size file corrupted. Consider using parameter back.");
            continue;
          }
          int height, width;
          if (!int.TryParse(split[1], out width))
          {
            if (!Constants.DownloadData)
              Helper.ShowWarningMessage("Media size file corrupted. Consider using parameter back.");
            continue;
          }
          if (!int.TryParse(split[2], out height))
          {
            if (!Constants.DownloadData)
              Helper.ShowWarningMessage("Media size file corrupted. Consider using parameter back.");
            continue;
          }
          Constants.MediaSizes.Add(split[0], new int[]{width, height});
        }
      }
      catch (FileNotFoundException)
      {
        Helper.ShowWarningMessage("File 'AvailableTactics.txt' not found!");
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
            // Empty line or comment: continue
            if (str == "" || str[0] == '#')
              continue;
            Constants.ValidatedUrls.Add(str);
          }
        }
        catch (FileNotFoundException)
        {
          Helper.ShowWarningMessage("File 'ValidatedUrls.txt' not found!");
        }
      }
  }

    /***********************************************************************************************
     * SaveSettings / 2015-05-24 / Wethospu                                                        *
     *                                                                                             *
     * Saves media sizes.                                                                          *
     *                                                                                             *
     ***********************************************************************************************/

    static void SaveSettings()
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
          Helper.ShowWarningMessage("File MediaSizes.txt in use.");
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
          Helper.ShowWarningMessage("File ValidatedUrls.txt in use.");
        }
      }
    }

    /***********************************************************************************************
     * Build / 2014-08-01 / Wethospu                                                               *
     *                                                                                             *
     * Generates the content.                                                                      *
     *                                                                                             *
     ***********************************************************************************************/

    static bool Build()
    {
      Console.WriteLine("Press enter to start.");
      Console.WriteLine("Press 1 to validate urls and update ValidatedUrls.txt.");
      Console.WriteLine("Press 2 to download used images/videos and update MediaSizes.txt.");
      Console.WriteLine("Press 3 to do both.");
      Console.WriteLine("Press 4 for optimized release generation.");
      Console.WriteLine("Press esc to quit.");
      var row = Console.CursorTop;
      var key = Console.ReadKey();
      Helper.ClearConsoleLine(row);
      if (key.Key == ConsoleKey.Escape)
        return false;
      Console.WriteLine("");
      Constants.ValidateUrls = key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.D3;
      Constants.DownloadData = key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.D3;
      Constants.Initialize(key.Key == ConsoleKey.D4);
      CheckInternetSettings();
      LoadSettings();
      Dictionary<string, EnemyAttributes> enemyAttributes = new Dictionary<string, EnemyAttributes>();
      if (File.Exists(Constants.DataRaw + "data.json"))
      {
        using (StreamReader r = new StreamReader(Constants.DataRaw + "data.json"))
        {
          string json = r.ReadToEnd();
          enemyAttributes = JsonConvert.DeserializeObject<Dictionary<string, EnemyAttributes>>(json);
        }
      }
      else
        Helper.ShowWarningMessage("File " + Constants.DataRaw + "data.json" + " doesn't exist. No enemy data loaded.");
      
      Constants.UniqueIndexCounter = 0;
      string[] dungeonsToGenerate = { "ac", "cm", "ta", "se", "cof", "hotw", "coe", "arah" };
      string[] fractalsToGenerate = { "Aether", "Aqua", "Cliff", "Furn", "Mai", "Molten", "Snow", "Solid", "Swamp", "Thauma", "Uncat", "Under", "Urban", "Volc" };
      string[] raidsToGenerate = { "vale" };
      // File containing all enemies and encounters for searching.
      // Name|Rank|Category|Dungeon|Path
      // Some special values must also be translated.
      GeneratePage("General");
      var indexFile = new StringBuilder();
      indexFile.Append(Constants.InitialdataHtml);
      indexFile.Append(Constants.InitialDataIndex);
      // Generate enemies with help of datamined information. / 2015-10-05 / Wethospu
      var enemyData = EnemyGenerator.GenerateEnemies(enemyAttributes);
      
      var instanceData = new DataCollector();
      foreach (var dungeon in dungeonsToGenerate)
      {
        var encounterData = GenerateDungeon("dungeon", dungeon, indexFile, instanceData, enemyData);
        if (encounterData != null)
          EncounterGenerator.GenerateFiles(encounterData.Paths, encounterData.Encounters, enemyData, encounterData.Paths);
      }
      // Gather fractals to merge their path info. / 2015-10-29 / Wethospu.
      var fractals = new List<EncounterData>();
      foreach (var fractal in fractalsToGenerate)
      {
        var encounterData = GenerateDungeon("fractal", fractal, indexFile, instanceData, enemyData);
        if (encounterData != null)
          fractals.Add(encounterData);
      }
      foreach (var fractal in fractals)
        EncounterGenerator.GenerateFiles(fractal.Paths, fractal.Encounters, enemyData, instanceData.FractalPaths);
      foreach (var raid in raidsToGenerate)
      {
        var encounterData = GenerateDungeon("raid", raid, indexFile, instanceData, enemyData);
        if (encounterData != null)
          EncounterGenerator.GenerateFiles(encounterData.Paths, encounterData.Encounters, enemyData, encounterData.Paths);
      }


      EnemyGenerator.GenerateFile(enemyData, indexFile, instanceData);
      var fileName = Constants.DataOutput + Constants.DataEnemyResult + "indexfile.htm";
      if (!Directory.Exists(Constants.DataOutput + Constants.DataEnemyResult))
      {
        Helper.ShowWarning("Directory " + Constants.DataOutput + Constants.DataEnemyResult + " doesn't exist.");
        return true;
      }
      File.WriteAllText(fileName, indexFile.ToString());
      OtherGenerator.GenerateOthers(instanceData);
      Console.WriteLine("");
      return true;
    }

    static void CheckInternetSettings()
    {
      if (Constants.ValidateUrls && Constants.DownloadData)
        Console.WriteLine("Checking internet connection for url validating and data download.");
      else if (Constants.ValidateUrls)
        Console.WriteLine("Checking internet connection for url validating.");
      else if (Constants.ValidateUrls)
        Console.WriteLine("Checking internet connection for data download.");
      if (Constants.ValidateUrls || Constants.DownloadData)
      {
        if (Helper.IsValidUrl("http://google.com"))
        {
          Console.WriteLine("Check up succesful. Proceeding, this may take a while.");
        }
        else if (Constants.ValidateUrls && Constants.DownloadData)
        {
          Console.WriteLine("Internet not detected. Disabling url validating and data download.");
          Constants.ValidateUrls = false;
          Constants.DownloadData = false;
        }
        else if (Constants.ValidateUrls)
        {
          Console.WriteLine("Internet not detected. Disabling url validating.");
          Constants.ValidateUrls = false;
        }
        else if (Constants.DownloadData)
        {
          Console.WriteLine("Internet not detected. Disabling data download.");
          Constants.DownloadData = false;
        }
        Console.WriteLine("");
      }
    }

    /***********************************************************************************************
    * GenerateDungeon / 2014-08-01 / Wethospu                                                      *
    *                                                                                              *
    * Generates one dungeon.                                                                       *
    *                                                                                              *
    * enemyAttributes: Datamined enemy attributes and other information.                           *
    *                                                                                              *
    ***********************************************************************************************/

    static EncounterData GenerateDungeon(string type, string file, StringBuilder indexFile, DataCollector dungeonData, List<Enemy> enemyData)
    {
      Console.WriteLine(Helper.ToUpper(type) + " " + file);
      LinkGenerator.CurrentDungeon = file;
      // Read and generate data. / 2015-08-09 / Wethospu
      var location = Constants.DataGuidesRaw;
      if (type.Equals("fractal"))
        location = Constants.DataFractalsRaw;
      if (type.Equals("dungeon"))
        location = Constants.DataDungeonsRaw;
      if (type.Equals("raid"))
        location = Constants.DataRaidsRaw;
      var encounterData = EncounterGenerator.GeneratePaths(location, file, enemyData);
      if (encounterData == null)
        return null; 
      if (encounterData.Paths == null)
        return encounterData;
      if (type.Equals("dungeon"))
        dungeonData.AddDungeon(file, encounterData.Paths);
      if (type.Equals("fractal"))
        dungeonData.AddFractal(encounterData.Paths);
      if (type.Equals("raid"))
        dungeonData.AddRaid(file, encounterData.Paths);
      return encounterData;
    }

    /***********************************************************************************************
     * GeneratePage / 2014-08-01 / Wethospu                                                        *
     *                                                                                             *
     * Generates one page (only dungeon data).                                                     *
     *                                                                                             *
     ***********************************************************************************************/

    static void GeneratePage(string dungeon)
    {
      Console.WriteLine("Guide " + dungeon);
      LinkGenerator.CurrentDungeon = dungeon;
      var encounterData = EncounterGenerator.GeneratePaths(Constants.DataGuidesRaw, dungeon, null);
      if (encounterData == null)
        return;
      EncounterGenerator.GenerateFiles(encounterData.Paths, encounterData.Encounters, null, encounterData.Paths);
      if (encounterData.Paths == null)
        return;
    }
  }
}
