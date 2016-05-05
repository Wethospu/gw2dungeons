using DataCreator.Encounters;
using DataCreator.Enemies;
using DataCreator.Shared;
using DataCreator.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataCreator
{

  enum InstanceType { dungeon, fractal, raid }

  /// <summary>
  /// Contains top level functions. Functionality should be split when the size grows too big.
  /// </summary>
  public static class Program
  {
    public static void Main()
    {
      Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
      Console.OutputEncoding = Constants.Encoding;
      Directory.SetCurrentDirectory("..");
      while (true)
      {
        Console.Clear();
        ErrorHandler.WarningCounter = 0;
        RemoveOldFiles();
        if (!AskUserInput())
          break;
        CheckInternetSettings();
        Settings.ReadSettings();
        Settings.ReadCacheFiles();
        Build();
        var builder = new StringBuilder();
        builder.Append("Generating completed");
        if (ErrorHandler.WarningCounter > 0)
          builder.Append(ErrorHandler.WarningCounterMessage());
        builder.Append(".");
        Settings.SaveCacheFiles();
        Console.WriteLine(builder.ToString());
        Console.WriteLine("Press any key to exit or Esc to restart.");
        var info = Console.ReadKey();
        if (info.Key == ConsoleKey.Escape)
          continue;
        break;
      }
    }

    /// <summary>
    /// Remove all generated files for a clean starting situation.
    /// </summary>
    // If a file can't be removed there is a risk that it won't get updated either.
    private static void RemoveOldFiles()
    {
      if (Directory.Exists(Constants.DataOutput))
      {
        // Gather files first to get their count and to delete them with less code.
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
        try
        {
          foreach (var file in fileList)
            File.Delete(file);
          if (Directory.Exists(Constants.DataOutput + Constants.DataEnemyResult))
            Directory.Delete(Constants.DataOutput + Constants.DataEnemyResult, true);
          if (Directory.Exists(Constants.DataOutput + Constants.DataEncounterResult))
            Directory.Delete(Constants.DataOutput + Constants.DataEncounterResult, true);
          if (Directory.Exists(Constants.DataOutput + Constants.DataMediaResult))
            Directory.Delete(Constants.DataOutput + Constants.DataMediaResult, true);
        }
        catch
        {
          Console.WriteLine("Couldn't remove every output file. Some files may not be updated.");
        }
        Console.WriteLine("\n");
      }
    }

    // The program has different generation modes and some slow operations.
    // So user needs to have control over what he wants to do.
    // Returns false if user wants to close the program.
    private static bool AskUserInput()
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
      return true;
    }

    // Some operations require an active internet connection.
    // Without internet there is no point trying to do them.
    /// <summary>
    /// Verify internet by attempting to reach a common internet site (google.com).
    /// </summary>
    public static void CheckInternetSettings()
    {
      if (Constants.ValidateUrls && Constants.DownloadData)
        Console.WriteLine("Checking internet connection for url validating and data download.");
      else if (Constants.ValidateUrls)
        Console.WriteLine("Checking internet connection for url validating.");
      else if (Constants.ValidateUrls)
        Console.WriteLine("Checking internet connection for data download.");
      if (Constants.ValidateUrls || Constants.DownloadData)
      {
        if (Helper.IsValidUrl(Constants.URLToVerifyInternet))
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

    /// <summary>
    /// Main generating function. Generates all data.
    /// </summary>
    private static void Build()
    {
      var enemyData = ReadEnemyDataminedData();
      Constants.UniqueIndexCounter = 0;
      GeneratePage("General");
      LinkGenerator.CurrentDungeon = Constants.EnemyMediaFolder;
      var enemies = EnemyGenerator.GenerateEnemies(enemyData);
      
      var instanceData = new DataCollector();
      foreach (var dungeon in Directory.EnumerateFiles(Constants.DataDungeonsRaw).Select(Path.GetFileNameWithoutExtension))
      {
        var encounterData = GenerateInstance(InstanceType.dungeon, dungeon, instanceData, enemies);
        if (encounterData != null)
          EncounterGenerator.CreateFiles(encounterData, enemies, encounterData.Paths);
      }
      // Gather fractals to merge their path information.
      var fractals = new List<InstanceData>();
      foreach (var fractal in Directory.EnumerateFiles(Constants.DataFractalsRaw).Select(Path.GetFileNameWithoutExtension))
      {
        var encounterData = GenerateInstance(InstanceType.fractal, fractal, instanceData, enemies);
        if (encounterData != null)
          fractals.Add(encounterData);
      }
      foreach (var fractal in fractals)
        EncounterGenerator.CreateFiles(fractal, enemies, instanceData.FractalPaths);
      foreach (var raid in Directory.EnumerateFiles(Constants.DataRaidsRaw).Select(Path.GetFileNameWithoutExtension))
      {
        var encounterData = GenerateInstance(InstanceType.raid, raid, instanceData, enemies);
        if (encounterData != null)
          EncounterGenerator.CreateFiles(encounterData, enemies, encounterData.Paths);
      }
      EnemyGenerator.CreateFiles(enemies, instanceData);
      GenerateSearchIndex(instanceData, enemies);
      OtherGenerator.GenerateOthers(instanceData);
      Console.WriteLine("");
    }

    /// <summary>
    /// Game client has information about the enemies. It's used to generate accurate damage, health and armor values.
    /// </summary>
    // The data is generated by a third party (Atoi).
    private static Dictionary<string, EnemyAttributes> ReadEnemyDataminedData()
    {
      Dictionary<string, EnemyAttributes> enemyData = new Dictionary<string, EnemyAttributes>();
      if (File.Exists(Constants.DataRaw + "data.json"))
      {
        using (StreamReader r = new StreamReader(Constants.DataRaw + "data.json"))
        {
          string json = r.ReadToEnd();
          enemyData = JsonConvert.DeserializeObject<Dictionary<string, EnemyAttributes>>(json);
        }
      }
      else
        ErrorHandler.ShowWarningMessage("File " + Constants.DataRaw + "data.json" + " doesn't exist. No enemy data loaded.");
      return enemyData;
    }

    /// <summary>
    /// Generates a single instance.
    /// </summary>
    private static InstanceData GenerateInstance(InstanceType type, string file, DataCollector dungeonData, List<Enemy> enemyData)
    {
      Console.WriteLine("File " + file);
      LinkGenerator.CurrentDungeon = file;
      var location = Constants.DataGuidesRaw;
      if (type == InstanceType.fractal)
        location = Constants.DataFractalsRaw;
      if (type == InstanceType.dungeon)
        location = Constants.DataDungeonsRaw;
      if (type == InstanceType.raid)
        location = Constants.DataRaidsRaw;
      var encounterData = EncounterGenerator.ReadInstance(location, file, enemyData);
      if (encounterData == null)
        return null; 
      if (encounterData.Paths == null)
        return encounterData;
      if (type == InstanceType.dungeon)
        dungeonData.AddDungeon(file, encounterData.Paths);
      if (type == InstanceType.fractal)
        dungeonData.AddFractal(encounterData.Paths);
      if (type == InstanceType.raid)
        dungeonData.AddRaid(file, encounterData.Paths);
      return encounterData;
    }

    /// <summary>
    /// Generates a non-instance page.
    /// </summary>
    // Currently used for general guide.
    // TODO: Could this be merged with GenerateInstance?
    private static void GeneratePage(string dungeon)
    {
      Console.WriteLine("Guide " + dungeon);
      LinkGenerator.CurrentDungeon = dungeon;
      var encounterData = EncounterGenerator.ReadInstance(Constants.DataGuidesRaw, dungeon, null);
      if (encounterData == null)
        return;
      EncounterGenerator.CreateFiles(encounterData, null, encounterData.Paths);
      if (encounterData.Paths == null)
        return;
    }

    // With over thousand enemies the website's search performance is an issue.
    /// <summary>
    /// Make the search faster by using a small indexfile which contains search data for every enemy.
    /// </summary>
    private static void GenerateSearchIndex(DataCollector dungeonData, List<Enemy> enemies)
    {
      var indexFile = new StringBuilder();
      indexFile.Append(Constants.InitialdataHtml);
      indexFile.Append(Constants.InitialDataIndex);
      foreach (var enemy in enemies)
      {
        // TODO: Previous enemy name was used to find the enemy info. Now Index seems to be used so remove the name.
        indexFile.Append(enemy.Name).Append("|").Append(Helper.Simplify(enemy.Name)).Append("|").Append(enemy.Rank);
        indexFile.Append("|").Append(enemy.Attributes.Family.GetInternal()).Append("|").Append(string.Join(":", enemy.Paths));
        indexFile.Append("|").Append(enemy.Index);
        // Compact tags take less space (especially important for search URLs).
        var builder = new StringBuilder();
        foreach (var tag in enemy.Tags)
          builder.Append(dungeonData.GetShortTag(tag));
        indexFile.Append("|").Append(builder.ToString());
        indexFile.Append(Constants.ForcedLineEnding);
      }

      var directory = Constants.DataOutput + Constants.DataEnemyResult;
      if (Directory.Exists(directory))
        File.WriteAllText(directory + "indexfile.html", indexFile.ToString());
      else
        ErrorHandler.ShowWarning("Directory " + directory + " doesn't exist.");
    }
  }
}
