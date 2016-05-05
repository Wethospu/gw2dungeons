using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;

namespace DataCreator.Enemies
{
  /// <summary>
  /// Used to load enemy data and generate html output for them.
  /// </summary>
  public static class EnemyGenerator
  {
    /// <summary>
    /// Generates all enenies.
    /// </summary>
    // Some enemies are shared between different instances so this can't be done instance by instance.
    public static List<Enemy> GenerateEnemies(Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      var enemyData = new List<Enemy>();
      if (!Directory.Exists(Constants.DataEnemyRaw))
      {
        ErrorHandler.ShowWarning("Directory " + Constants.DataEnemyRaw + " doesn't exist.");
        return enemyData;
      }
      var enemyFiles = Directory.GetFiles(Constants.DataEnemyRaw);
      foreach (var file in enemyFiles)
      {
        if (Path.GetExtension(file) != ".txt")
          continue;
        string[] lines;
        if (File.Exists(file))
          lines = File.ReadAllLines(file, Constants.Encoding);
        else
        {
          ErrorHandler.ShowWarningMessage("File " + file + " doesn't exist!");
          return null;
        }
        ErrorHandler.CurrentFile = file;
        for (var row = 0; row < lines.Length; row++)
        {
          ErrorHandler.InitializeWarningSystem(row + 1, lines[row]);
          HandleLine(lines[row], enemyData, enemyAttributes);
        }
      }
      // Reset internal state.
      ErrorHandler.InitializeWarningSystem(-1, "");
      // Sort for consistency (also allows see enemies without proper ids).
      enemyData.Sort();
      // Set up internal indexes to allow faster searching in the website.
      for (var i = 0; i < enemyData.Count; i++)
        enemyData[i].Index = i;
      return enemyData;
    }

    /// <summary>
    /// Enemy data file has 3 different "levels". 0 = main loop, 1 = attack loop, 2 = effect loop.
    /// </summary>
    private static int _mode;
    /// <summary>
    /// Processes one line of data.
    /// </summary>
    /// <param name="enemies">Current list of added enemies.</param>
    /// <param name="enemyAttributes">Datamined enemy information-</param>
    private static void HandleLine(string line, List<Enemy> enemies, Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      if (!Helper.CheckLineValidity(line))
        return;

      var tagData = new TagData(line, Constants.TagSeparator);
      var returnValue = 0;
      if (_mode == 0)
        returnValue = EnemyLoop(tagData, enemies, enemyAttributes);
      else if (_mode == 1)
        returnValue = AttackLoop(tagData, enemies);
      else if (_mode == 2)
        returnValue = EffectLoop(tagData, enemies);
      _mode += returnValue;
      // Moving between loop modes only happen when detecting stuff from other loops.
      if (returnValue != 0)
      {
        // Line has to be rechecked on the new loop.
        HandleLine(line, enemies, enemyAttributes);
      }
    }

    /// <summary>
    /// Main loop for loading general enemy information.
    /// </summary>
    private static int EnemyLoop(TagData line, List<Enemy> enemies, Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      Enemy currentEnemy = null;
      if (enemies.Count > 0)
        currentEnemy = enemies.Last();
      if (!line.Tag.Equals("name") && !line.Tag.Equals("id") && currentEnemy != null & currentEnemy.IsNameCopied)
        ErrorHandler.ShowWarning("ID or name not explicitly set for a copied enemy.");
      if (line.Tag.Equals("copy"))
      {
        VerifyEnemy(currentEnemy);
        var found = FindEnemy(line.Data, enemies);
        enemies.Add(HandleCopy(found));
        currentEnemy = enemies.Last();
      }
      else if (line.Tag.Equals("name"))
      {
        if (currentEnemy == null || !currentEnemy.IsNameCopied)
        {
          VerifyEnemy(currentEnemy);
          enemies.Add(new Enemy());
          currentEnemy = enemies.Last();
        }
        currentEnemy.Name = line.Data;
      }
      else
      {
        if (currentEnemy == null)
        {
          ErrorHandler.ShowWarning("Ignoring the line because there is no active enemy");
          return 0;
        }
        if (line.Tag.Equals("id"))
          HandleId(line.Data, currentEnemy, enemyAttributes);
        else if (line.Tag.Equals("path"))
          currentEnemy.Paths = new List<string>(line.Data.ToLower().Split('|'));
        else if (line.Tag.Equals("rank"))
          currentEnemy.Rank = line.Data.ToLower();
        else if (line.Tag.Equals("ally"))
          currentEnemy.Allied = true;
        else if (line.Tag.Equals("alt"))
          HandleAlternativeNames(line.Data, currentEnemy);
        else if (line.Tag.Equals("image"))
          HandleMedia(line.Data, currentEnemy);
        else if (line.Tag.Equals("level"))
          HandleLevel(line.Data, currentEnemy);
        else if (line.Tag.Equals("scaling"))
          HandleScaling(line.Data, currentEnemy);
        else if (line.Tag.Equals("attack"))
          return 1;
        else if (line.Tag.Equals("tactic"))
          HandleTactic(line.Data, currentEnemy);
        else if (line.Tag.Equals("health"))
          HandleHealth(line.Data, currentEnemy);
        else if (line.Tag.Equals("toughness"))
          HandleToughness(line.Data, currentEnemy);
        else if (line.Tag.Equals("condition"))
          HandleCondition(line.Data, currentEnemy);
        else if (line.Tag.Equals("race"))
          currentEnemy.Attributes.Family.SetName(line.Data);
        else if (line.Tag.Equals("tag"))
          HandleTag(line.Data, currentEnemy);
        // Normal content.
        else if (line.Tag.Equals(""))
        {
          currentEnemy.Tactics.AddLine(LinkGenerator.CheckLinkSyntax(line.Data));
        }
        else if (line.Tag.Equals("type") || line.Tag.Equals("effect") || line.Tag.Equals("cooldown") || line.Tag.Equals("additional") || line.Tag.Equals("animation"))
          ErrorHandler.ShowWarning("Missing attack name (\"attack='name'\")!");
        else
          ErrorHandler.ShowWarning("Unrecognized tag: " + line.Tag);
      }
      return 0;
    }

    private static void VerifyEnemy(Enemy currentEnemy)
    {
      if (currentEnemy == null)
        return;
      if (currentEnemy.Paths.Count == 0)
        ErrorHandler.ShowWarning("Path not set for previous enemy " + currentEnemy.Name + ".");
      if (currentEnemy.Rank.Length == 0)
        ErrorHandler.ShowWarning("Rank not set for previous enemy " + currentEnemy.Name + ".");
    }

    /// <summary>
    /// Copies an enemy to a given enemy.
    /// </summary>
    private static Enemy HandleCopy(Enemy foundEnemy)
    {
      if (foundEnemy == null)
      {
        ErrorHandler.ShowWarning("Copying failed. Enemy not found!");
        return null;
      }
      var currentEnemy = Helper.CloneJson(foundEnemy);
      currentEnemy.IsNameCopied = true;
      currentEnemy.AreAnimationsCopied = true;
      return currentEnemy;
    }

    /// <summary>
    /// Sets datamined id to a given enemy. Automatically loads datamined data.
    /// </summary>
    private static void HandleId(string data, Enemy currentEnemy, Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      if (data.Length == 0)
      {
        ErrorHandler.ShowWarning("Missing info. Use \"id='id'\"!");
        return;
      }
      currentEnemy.IsNameCopied = false;
      var ids = data.Split('|');
      // Enemies can have multiple genders if there are model variations.
      // Each model has a different id so store old ones to get all added.
      var oldGenders = "";
      foreach (var id in ids)
      {
        currentEnemy.InternalIds.Add(Helper.ParseI(id));
        if (enemyAttributes.ContainsKey(id))
        {
          // Different enemies may share attributes (for exampled allied enemies).
          currentEnemy.Attributes = enemyAttributes[id];
          if (oldGenders.Length > 0)
          {
            var genders = oldGenders.Split('|');
            if (genders.Contains(currentEnemy.Attributes.Gender))
              currentEnemy.Attributes.Gender = oldGenders;
            else
              currentEnemy.Attributes.Gender = oldGenders + "|" + currentEnemy.Attributes.Gender;
          }
        }
        else
          ErrorHandler.ShowWarning("Id " + data + " not found in enemy attributes.");
      }
    }

    /// <summary>
    /// Adds alternative names to a given enemy. These are used for the site search and enemy linking.
    /// </summary>
    private static void HandleAlternativeNames(string data, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"alt='alt1'|'alt2'|'altN'\"!");
      if (data.Contains('_'))
        ErrorHandler.ShowWarning("Alt names " + data + "  containts '_'. Replace them with ' '!");
      var altNames = data.Split('|');
      currentEnemy.AltNames.Clear();
      foreach (var altName in altNames)
        currentEnemy.AddAlternativeName(altName);
    }

    /// <summary>
    /// Adds a media file to a given enemy.
    /// </summary>
    private static void HandleMedia(string data, Enemy currentEnemy)
    {
      if (currentEnemy.AreAnimationsCopied)
      {
        currentEnemy.Medias.Clear();
        currentEnemy.AreAnimationsCopied = false;
      }
      currentEnemy.HandleMedia(data, Constants.EnemyMediaFolder);
    }

    /// <summary>
    /// Sets a custom level for a given enemy. By default, this is calculated from the path base level.
    /// </summary>
    private static void HandleLevel(string data, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"level='amount'\"");
      currentEnemy.Level = Helper.ParseI(data);
    }

    /// <summary>
    /// Sets fractal scaling type for a given enemy. See GW2Helper.ScalingTypeToString for accepted values.
    /// </summary>
    private static void HandleScaling(string data, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"scaling='type'!");
      currentEnemy.ScalingType = data;
    }

    /// <summary>
    /// Similar to encounter tactics. Activates tactics for a given enemy. Activated tactics are able to receive content lines.
    /// </summary>
    private static void HandleTactic(string data, Enemy currentEnemy)
    {
      // Set validity to over max so custom tactics never get overridden by encounter tactics.
      currentEnemy.TacticValidity = 2.0;
      currentEnemy.HandleTactic(data, null);
    }

    /// <summary>
    /// Adds custom search tags to a given enemy. Not really in use because most tags are handled automatically.
    /// </summary>
    private static void HandleTag(string data, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"tag='tag1'|'tag2'|'tag3'\"!");
      var split = data.Split('|');
      foreach (var str in split)
        currentEnemy.Tags.Add(str.ToLower());
    }

    /// <summary>
    /// Most health values are acquired automatically from datamined data. This is used for special cases.
    /// </summary>
    private static void HandleHealth(string data, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"health='amount'.");
      currentEnemy.Attributes.Multipliers.HealthMultiplier = Helper.ParseD(data);
      // If vitality is not set, initialize it with something sensible so the page can calculate something.
      if (currentEnemy.Attributes.Multipliers.Vitality < 0.0001)
        currentEnemy.Attributes.Multipliers.Vitality = 1;
      if (currentEnemy.Attributes.Multipliers.HealthMultiplier > 1000)
        ErrorHandler.ShowWarning("Health values should be multipliers. Calculate the multiplier.");
    }

    /// <summary>
    /// Most toughness values are acquired automatically from datamined data. This is used for special cases.
    /// </summary>
    private static void HandleToughness(string data, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"toughness='amount'.");
      currentEnemy.Attributes.Multipliers.Toughness = Helper.ParseD(data);
      if (currentEnemy.Attributes.Multipliers.Toughness > 100)
        ErrorHandler.ShowWarning("Toughness values should be multipliers. Calculate the multiplier.");
    }

    /// <summary>
    /// Most condition damage values are acquired automatically from datamined data. This is used for special cases.
    /// </summary>
    private static void HandleCondition(string data, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"condition='amount'.");
      currentEnemy.Attributes.Multipliers.ConditionDamage = Helper.ParseD(data);
      if (currentEnemy.Attributes.Multipliers.ConditionDamage > 100)
        ErrorHandler.ShowWarning("Condition damage values should be multipliers. Calculate the multiplier.");
    }

    /// <summary>
    /// Sub-loop for loading enemy attack information.
    /// </summary>
    public static int AttackLoop(TagData line, List<Enemy> enemies)
    {
      Enemy currentEnemy = null;
      if (enemies.Count > 0)
        currentEnemy = enemies.Last();
      Attack currentAttack = null;
      if (currentEnemy != null && currentEnemy.Attacks.Count > 0)
        currentAttack = currentEnemy.Attacks.Last();

      // Add old attack and start a new one.
      if (line.Tag.Equals("attack"))
      {
        if (line.Data.Length == 0)
          ErrorHandler.ShowWarning("Missing info. Use \"attack='name'\"!");
        currentEnemy.Attacks.Add(new Attack(LinkGenerator.CheckLinkSyntax(line.Data)));
        currentAttack = currentEnemy.Attacks.Last();
      }
      // Tags from main loop. Save attack and exit this loop.
      else if (line.Tag.Equals("name") || line.Tag.Equals("copy") || line.Tag.Equals("potion"))
        return -1;
      else if (line.Tag.Equals("id"))
        HandleAttackId(line.Data, currentAttack, currentEnemy);
      // Tags from effect loop. Exit immediately.
      else if (line.Tag.Equals("effect"))
      {
        return 1;
      }
      else if (line.Tag.Equals("cooldown"))
        HandleCooldown(line.Data, currentAttack);
      else if (line.Tag.Equals("additional"))
        HandleAdditional(line.Data, currentAttack, currentEnemy);
      else if (line.Tag.Equals("animation"))
        HandleAnimation(line.Data, currentAttack);
      else if (line.Tag.Equals("image"))
        HandleAttackMedia(line.Data, currentAttack);
      else if (line.Tag.Equals("subeffect"))
        ErrorHandler.ShowWarning("Missing attack effect (\"effect='type'\")!");
      else if (line.Tag.Equals(""))
        ErrorHandler.ShowWarning("Something wrong with line " + line.Data + ".");
      else
        ErrorHandler.ShowWarning("Unrecognized tag: " + line.Tag);

      return 0;
    }

    private static void HandleAttackId(string data, Attack currentAttack, Enemy currentEnemy)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"id=number\".");
      currentAttack.LoadAttributes(Helper.ParseI(data), currentEnemy.Attributes);
    }

    private static void HandleCooldown(string data, Attack currentAttack)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"cooldown='number'\".");
      currentAttack.Cooldown = Helper.ParseD(data);
    }

    private static void HandleAdditional(string data, Attack currentAttack, Enemy currentEnemy)
    {
      // Treat additional information as an effect for a simpler UI.
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"additional='text'\".");
      var lower = data.ToLower();
      if (lower.Contains("can't be blocked") || lower.Contains("can't block"))
        currentEnemy.Tags.Add("can't block");
      if (lower.Contains("can't be evaded") || lower.Contains("can't evade"))
        currentEnemy.Tags.Add("can't evade");
      currentAttack.Effects.Add(new Effect(LinkGenerator.CheckLinkSyntax(data)));
    }

    private static void HandleAnimation(string data, Attack currentAttack)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"animation='pre cast'|'time'|'after cast'\" !");
      if (data.Contains(':') && !data.Contains('='))
        ErrorHandler.ShowWarning("Potentially use of wrong syntax. Use \"animation='pre cast'|'time'|'after cast'\" !");
      currentAttack.Animation = data;
    }

    private static void HandleAttackMedia(string data, Attack currentAttack)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"image='imagelink'\"!");
      currentAttack.Medias.Add(new Media(data));
    }


    /// <summary>
    /// Sub-sub-loop for loading enemy attack effect information.
    /// </summary>
    private static int EffectLoop(TagData line, List<Enemy> enemies)
    {
      Enemy currentEnemy = null;
      if (enemies.Count > 0)
        currentEnemy = enemies.Last();
      Attack currentAttack = null;
      if (currentEnemy != null && currentEnemy.Attacks.Count > 0)
        currentAttack = currentEnemy.Attacks.Last();
      Effect currentEffect = null;
      if (currentAttack != null && currentAttack.Effects.Count > 0)
        currentEffect = currentAttack.Effects.Last();
      // Add old effect and start a new one.
      if (line.Tag.Equals(""))
        ErrorHandler.ShowWarning("No tag for line " + line.Data + ".");
      else if (line.Tag.Equals("effect"))
      {
        if (line.Data.Length == 0)
          ErrorHandler.ShowWarning("Missing info. Use \"effect='type'\"!");
        if (currentAttack == null)
        {
          ErrorHandler.ShowWarning("Ignoring line. No active attack.");
        }
        currentAttack.Effects.Add(new Effect(LinkGenerator.CheckLinkSyntax(line.Data)));
        var type = line.Data.ToLower();
        foreach (var enemyTag in Constants.AttackTypeTags)
        {
          if (type.Contains(enemyTag))
            currentEnemy.Tags.Add(enemyTag);
        }
        currentEffect = currentAttack.Effects.Last();
      }
      // Tag from attack loop. Save effect and exit this loop.
      else if (line.Tag.Equals("attack"))
      {
        return -1;
      }
      // Tag from main loop. Save both effect and attack and then exit this loop.
      else if (line.Tag.Equals("name") || line.Tag.Equals("potion") || line.Tag.Equals("copy"))
      {
        return -2;
      }
      // Error handling for wrongly placed tags.
      else if (line.Tag.Equals("additional") || line.Tag.Equals("cooldown") || line.Tag.Equals("animation"))
        ErrorHandler.ShowWarning("Wrong position for tag " + line.Tag + ". Move above any type-tags.");
      else if (currentEffect == null)
      {
        ErrorHandler.ShowWarning("Ignoring line. No active effect.");
        return 0;
      }
      else if (line.Tag.Equals("count"))
        HandleCount(line.Data, currentEffect);
      else if (line.Tag.Equals("length"))
        HandleLength(line.Data, currentEffect);
      else if (line.Tag.Equals("frequency"))
        HandleFrequency(line.Data, currentEffect);
      else if (line.Tag.Equals("subeffect"))
        HandleSubeffect(line.Data, currentEffect);
      else
        ErrorHandler.ShowWarning("Unrecognized tag: " + line.Tag);
      return 0;
    }

    private static void HandleCount(string data, Effect currentEffect)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"count='number'\"");
      if (data.Equals("?"))
        currentEffect.HitCount = -1;
      else
        currentEffect.HitCount = Helper.ParseI(data);
      if (currentEffect.HitCount == 0)
        ErrorHandler.ShowWarning("Hit count can't be zero.");
    }

    private static void HandleLength(string data, Effect currentEffect)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"length='number'\"");
      currentEffect.HitLength = Helper.ParseD(data);
    }

    private static void HandleFrequency(string data, Effect currentEffect)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"frequency='number'\"");
      currentEffect.HitFrequency = Helper.ParseD(data);
    }

    private static void HandleSubeffect(string data, Effect currentEffect)
    {
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use TODO!");
      currentEffect.SubEffects.Add(LinkGenerator.CheckLinkSyntax(data));
    }

    /// <summary>
    /// Finds an enemy based on a given data. Used to find the base enemy for copying.
    /// </summary>
    private static Enemy FindEnemy(string data, List<Enemy> enemies)
    {
      if (data.Length == 0)
      {
        ErrorHandler.ShowWarning("Missing info. Use syntax \"copy" + Constants.TagSeparator + "'name'|'rank'|'path1':'path2':'pathN'\"");
        return null;
      }
      try
      {
        // Try first "copy by id" because it's faster to check.
        var id = int.Parse(data);
        foreach (var enemy in enemies)
        {
          if (enemy.InternalIds.Contains(id))
            return enemy;
        }
        ErrorHandler.ShowWarning("No enemy found for copy " + data + ". Change parameters, add the missing enemy, change order of enemies or check the readme.");
      }
      catch { }

      if (data.Contains(Constants.TagSeparator))
        ErrorHandler.ShowWarning("'" + Constants.TagSeparator + "' found. Use syntax \"copy" + Constants.TagSeparator + "'name'|'rank'|'path1':'path2':'pathN'\"");
      var dataSplit = data.Split('|');
      var name = dataSplit[0];
      var rank = "";
      var paths = new List<string>();
      if (dataSplit.Length > 1)
        rank = dataSplit[1];
      if (dataSplit.Length > 2)
      {
        if (dataSplit[2].Contains(' '))
          ErrorHandler.ShowWarning("' ' found. Use syntax \"copy" + Constants.TagSeparator + "'name'|'rank'|'path1':'path2':'pathN'\"");
        paths = new List<string>(dataSplit[2].Split(':'));
      }
      var foundEnemies = Gw2Helper.FindEnemies(enemies, name, rank, paths);
      if (foundEnemies.Count == 0)
      {
        ErrorHandler.ShowWarning("No enemy found for copy. Change parameters, add missing enemy, change order of enemies or check syntax file.");
        return null;
      }
      if (foundEnemies.Count > 1)
        ErrorHandler.ShowWarning("Multiple enemies found for copy. Add more parameters or check syntax file.");
      return foundEnemies[0];
    }

    public static List<Enemy> CreateLinks(List<Enemy> enemies)
    {
      foreach (var enemy in enemies)
        enemy.CreateLinks(enemy.Paths, enemies);
      return enemies;
    }



    /// <summary>
    /// Generates html files for the enemies.
    /// </summary>
    public static void CreateFiles(List<Enemy> enemies, DataCollector dungeonData)
    {
      enemies = CreateLinks(enemies);
      // Separate enemies (100 enemies per file) to reduce the initial loading time on the website.
      var enemyFile = new StringBuilder();
      enemyFile.Append(Constants.InitialdataHtml).Append(Constants.LineEnding);
      for (var i = 0; i < enemies.Count; i++)
      {
        enemyFile.Append(enemies[i].ToHtml());
        if ((i + 1) % 100 == 0 || i == enemies.Count - 1)
        {
          var fileName = Constants.DataOutput + Constants.DataEnemyResult + "enemies" + (i/100) + ".htm";
          var dirName = Path.GetDirectoryName(fileName);
          if (dirName != null)
            Directory.CreateDirectory(dirName);
          try
          {
            File.WriteAllText(fileName, enemyFile.ToString());
          }
          catch (UnauthorizedAccessException)
          {
            ErrorHandler.ShowWarningMessage("File " + fileName + " in use.");
          }
          enemyFile = new StringBuilder();
          enemyFile.Append(Constants.InitialdataHtml).Append(Constants.LineEnding);
        }
      }
      for (var i = 0; i < enemies.Count; i++)
      {
        // Add enemy info to the data collector for an advanced search capablities.
        if (dungeonData != null)
        {
          dungeonData.AddRace(enemies[i].Attributes.Family.GetDisplay());
          dungeonData.AddRank(enemies[i].Rank);
          foreach (var tag in enemies[i].Tags)
            dungeonData.AddTag(tag);
        }
      }
    }
  }
}
