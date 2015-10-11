using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;

namespace DataCreator.Enemies
{

  /***********************************************************************************************
   * EnemyGenerator / 2014-08-01 / Wethospu                                                      *
   *                                                                                             *
   * Generates enemies. Checks for syntax / content errors.                                      *
   *                                                                                             *
   ***********************************************************************************************/

  public static class EnemyGenerator
  {
    // Variables to keep track of the internal state.
    private static Enemy _currentEnemy;
    private static Attack _currentAttack;
    private static Effect _currentEffect;

    /***********************************************************************************************
    * GenerateEnemies / 2014-08-01 / Wethospu                                                      *
    *                                                                                              *
    * Generates enemies for one dungeon.                                                           *
    *                                                                                              *
    * Returns list of generated enemies.                                                           *
    * enemyAttributes: Datamined enemy attributes and other info.                                  *
    *                                                                                              *
    ***********************************************************************************************/

    public static List<Enemy> GenerateEnemies(Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      var enemyData = new List<Enemy>();
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
          Helper.ShowWarningMessage("File " + file + " doesn't exist!");
          return null;
        }
        Helper.CurrentFile = file;
        for (var row = 0; row < lines.Length; row++)
        {
          Helper.InitializeWarningSystem(row + 1, lines[row]);
          HandleLine(lines[row], enemyData, enemyAttributes);
        }
      }
      // Add the last enemy.
      if (_currentEffect != null && _currentAttack != null)
        _currentAttack.Effects.Add(_currentEffect);
      if (_currentAttack != null && _currentEnemy != null)
        _currentEnemy.Attacks.Add(_currentAttack);
      if (_currentEnemy != null)
        enemyData.Add(_currentEnemy);
      // Reset internal state.
      Helper.InitializeWarningSystem(-1, "");
      _currentEnemy = null;
      _currentAttack = null;
      _currentEffect = null;
      // Sort for consistency (also allows see enemies without proper ids). / 2015-10-05 / Wethospu
      enemyData.Sort();
      // Set up internal indexes. / 2015-10-05 / Wethospu
      for (var i = 0; i < enemyData.Count; i++)
        enemyData[i].Index = i;
      return enemyData;
    }

    /***********************************************************************************************
     * HandleLine / 2014-08-01 / Wethospu                                                          *
     *                                                                                             *
     * Processes one line of base data.                                                            *
     *                                                                                             *
     * line: Line to process.                                                                      *
     * enemies: Output. List of processed enemies.                                                 *
     *                                                                                             *
     ***********************************************************************************************/

    // 0 = main loop, 1 = attack loop, 2 = effect loop.
    private static int _mode;
    private static void HandleLine(string line, List<Enemy> enemies, Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      // Empty line or comment: continue
      if (line == "" || line[0] == '#')
        return;
      if (string.IsNullOrWhiteSpace(line))
      {
        Helper.ShowWarning("Line contains only whitespace (ignored). Please remove!");
        return;
      }
      if (line[0] == ' ')
        Helper.ShowWarning("Extra space detected. Please remove!");

      //// Split row to tag and data.
      var tagIndex = line.IndexOf(Constants.TagSeparator);
      // Note: TagSeparator won't be found if there is pure content. But it may also be an error. / 2015-08-09 / Wethospu
      var tag = "";
      if (tagIndex >= 0)
        tag = line.Substring(0, tagIndex).ToLower();
      // If tag has a space then it's actually pure content. / 2015-08-09 / Wethospu
      if (tag.Contains(" "))
      {
        tag = "";
        tagIndex = -1;
      }
      var data = "";
      if (tagIndex < line.Length)
        data = line.Substring(tagIndex + 1);
      //// Tag and data separated.

      // Analyze the tag and handle the data.
      var returnValue = 0;
      if (_mode == 0)
        returnValue = EnemyLoop(tag, data, enemies, enemyAttributes);
      else if (_mode == 1)
        returnValue = AttackLoop(tag, data);
      else if (_mode == 2)
        returnValue = EffectLoop(tag, data);
      _mode += returnValue;
      // Moving between loop modes only happen when detecting stuff from other loops.
      if (returnValue != 0)
      {
        // Line has to be rechecked on the new loop.
        HandleLine(line, enemies, enemyAttributes);
      }
    }

    /***********************************************************************************************
     * EnemyLoop / 2014-08-01 / Wethospu                                                           *
     *                                                                                             *
     * Main process loop for enemies.                                                              *
     *                                                                                             *
     * tag: Tag of the line.                                                                       *
     * data: Data of the line.                                                                     *
     * enemies: Output. List of processed enemies.                                                 *
     *                                                                                             *
     ***********************************************************************************************/

    private static int EnemyLoop(string tag, string data, List<Enemy> enemies, Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      if (!tag.Equals("name") && !tag.Equals("id") && _currentEnemy != null & _currentEnemy.IsNameCopied)
        Helper.ShowWarning("ID or name not explicitly set for a copied enemy.");
      if (tag.Equals("copy"))
      {
        if (_currentEnemy != null)
          enemies.Add(_currentEnemy);
        var found = FindEnemy(data, enemies);
        if (found != null)
        {
          _currentEnemy = Helper.CloneJson(found);
          _currentEnemy.IsNameCopied = true;
          _currentEnemy.AreAnimationsCopied = true;
        }
        else
          Helper.ShowWarning("Copying failed. Enemy not found!");
      }
      else if (tag.Equals("name"))
      {
        if (data.Length > 0)
        {
          if (_currentEnemy != null && !_currentEnemy.IsNameCopied)
          {
            enemies.Add(_currentEnemy);
            if (_currentEnemy.Paths.Count == 0)
              Helper.ShowWarning("Path not set for enemy " + _currentEnemy.Name);
          }
          if (data.Contains('_'))
            Helper.ShowWarning("Enemy name " + data + "  containts '_'. Replace them with ' '!");
          // For copies only set the name. / 2015-10-05 / Wethospu
          if (_currentEnemy != null && _currentEnemy.IsNameCopied)
            _currentEnemy.Name = data;
          else
            _currentEnemy = new Enemy(data);
          _currentEnemy.IsNameCopied = false;
          _currentAttack = null;
          _currentEffect = null;
        }
        else
          Helper.ShowWarning("Missing info. Use \"name='name'\"!");
      }
      else if (tag.Equals("id"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (data.Length > 0)
        {
          _currentEnemy.IsNameCopied = false;
          var ids = data.Split('|');
          // Enemies can have multiple genders if there are model variations. / 2015 - 09 - 28 / Wethospu
          // Each model has a different id so store old ones to get all added. / 2015-09-28 / Wethospu
          var oldGenders = "";
          foreach (var id in ids)
          {
            _currentEnemy.InternalIds.Add(Helper.ParseI(id));
            if (enemyAttributes.ContainsKey(id))
            {
              _currentEnemy.Attributes = enemyAttributes[id];
              if (oldGenders.Length > 0)
              {
                var genders = oldGenders.Split('|');
                // If the sex is already there it can be ignored. / 2015-09-28 / Wethospu
                if (genders.Contains(_currentEnemy.Attributes.Gender))
                  _currentEnemy.Attributes.Gender = oldGenders;
                else
                  _currentEnemy.Attributes.Gender = oldGenders + "|" + _currentEnemy.Attributes.Gender;
              }
              _currentEnemy.Rank = _currentEnemy.Attributes.GetRank();
              oldGenders = _currentEnemy.Attributes.Gender;
            }
            else
              Helper.ShowWarning("Id " + data + " not found in enemy attributes.");
          }
        }
        else
          Helper.ShowWarning("Missing info. Use \"id='id'\"!");
      }
      else if (tag.Equals("path"))
      {
        if (data.Length == 0)
          Helper.ShowWarning("Missing info. Use \"path='path1'|'path2'|'pathN'\"!");
        if (data.Contains(" "))
        {
          Helper.ShowWarning("' ' found. Use syntax \"path='path1'|'path2'|'pathN'\"");
          data = data.Replace(' ', '|');
        }
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else
          _currentEnemy.Paths = new List<string>(data.ToLower().Split('|'));
      }
      else if (tag.Equals("category"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (data.Length > 0)
        {
          _currentEnemy.Rank = data.ToLower();
          if (!LinkGenerator.EnemyCategories.Contains(_currentEnemy.Rank))
            Helper.ShowWarning("Category " + _currentEnemy.Rank + " not recognized. Check syntax for correct categories.");
        }
        else
          Helper.ShowWarning("Missing info. Use \"category='category'\"!");
      }
      else if (tag.Equals("alt"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (data.Length > 0)
        {
          if (data.Contains('_'))
            Helper.ShowWarning("Alt names " + data + "  containts '_'. Replace them with ' '!");
          var altNames = data.Split('|');
          _currentEnemy.AltNames.Clear();
          foreach (var altName in altNames)
            _currentEnemy.AddAlt(altName);
        }
        else
          Helper.ShowWarning("Missing info. Use \"alt='alt1'|'alt2'|'altN'\"!");
      }
      else if (tag.Equals("image"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (data.Length > 0)
        {
          if (_currentEnemy.AreAnimationsCopied)
          {
            _currentEnemy.Medias.Clear();
            _currentEnemy.AreAnimationsCopied = false;
          }
          _currentEnemy.Medias.Add(new Media(data));
        }
        else
          Helper.ShowWarning("Missing info. Use \"image='imagelink'\"!");
      }
      else if (tag.Equals("level"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (data.Length > 0)
        {
          _currentEnemy.Level = Helper.ParseI(data);
        }
        else
          Helper.ShowWarning("Missing info. Use \"level='amount'\"");
      }
      else if (tag.Equals("scaling"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (data.Length > 0)
        {
          var scalingSplit = data.Split('|');
          _currentEnemy.ScalingType = scalingSplit[0];
          if (scalingSplit.Length > 1)
          {
            int result;
            if (int.TryParse(scalingSplit[1], out result))
              _currentEnemy.ScalingFractal = result;
            else
              Helper.ShowWarning("Fractal scale " + scalingSplit[1] + " is not an integer!");
            if (scalingSplit.Length > 2)
            {
              if (int.TryParse(scalingSplit[2], out result))
                _currentEnemy.ScalingLevel = result;
              else
                Helper.ShowWarning("Enemy level " + scalingSplit[2] + " is not an integer!");

            }
          }
        }

        else
          Helper.ShowWarning("Missing info. Use \"scaling='type'|'fractal scale'|'enemy level'\"!");
      }
      else if (tag.Equals("attack"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (_currentEnemy.Rank.Length == 0)
          Helper.ShowWarningMessage("Category not set for enemy " + _currentEnemy.Name + ". Please fix!");
        return 1;
      }
      else if (tag.Equals("tactic"))
      {
        // Set validity to over max so custom tactics never get overridden. / 2015-08-09 / Wethospu
        _currentEnemy.TacticValidity = 2.0;
        if (data.Length > 0)
          _currentEnemy.Tactics.AddTactics(data);
        else
          Helper.ShowWarning("Missing info. Use \"tactic='tactic1'|'tactic2'|'tacticN'\".");
      }
      else if (tag.Equals("health"))
      {
        if (data.Length > 0)
        {
          _currentEnemy.Attributes.Multipliers.HealthMultiplier = Helper.ParseD(data);
          // If vitality is not set, initialize it with something sensible so the page can calculate something. / 2015-09-10 / Wethospu
          if (_currentEnemy.Attributes.Multipliers.Vitality < 0.1)
            _currentEnemy.Attributes.Multipliers.Vitality = 1;
          if (Helper.ParseD(data) > 1000)
            Helper.ShowWarning("Health values should be multipliers. Calculate the multiplier.");
        }
        else
          Helper.ShowWarning("Missing info. Use \"health='amount'.");
      }
      else if (tag.Equals("toughness"))
      {
        if (data.Length > 0)
        {
          _currentEnemy.Attributes.Multipliers.Toughness = Helper.ParseD(data);
          if (Helper.ParseD(data) > 100)
            Helper.ShowWarning("Toughness values should be multipliers. Calculate the multiplier.");
        }
        else
          Helper.ShowWarning("Missing info. Use \"toughness='amount'.");
      }
      else if (tag.Equals("armor"))
      {
        Helper.ShowWarning("Armor values shouldn't be used. Calculate the toughness multiplier.");
      }
      else if (tag.Equals("condition"))
      {
        if (data.Length > 0)
        {
          _currentEnemy.Attributes.Multipliers.ConditionDamage = Helper.ParseD(data);
          if (Helper.ParseD(data) > 100)
            Helper.ShowWarning("Condition damage values should be multipliers. Calculate the multiplier.");
        }
        else
          Helper.ShowWarning("Missing info. Use \"condition='amount'.");
      }
      else if (tag.Equals("race"))
      {
        if (data.Length > 0)
        {
          _currentEnemy.Attributes.Family.Name = data;
        }
        else
          Helper.ShowWarning("Missing info. Use \"race='value'.");
      }
      else if (tag.Equals("tag"))
      {
        if (data.Length > 0)
        {
          var split = data.Split('|');
          foreach (var str in split)
            _currentEnemy.Tags.Add(str.ToLower());
        }
        else
          Helper.ShowWarning("Missing info. Use \"tag='tactic1'|'tactic2'|'tacticN'\"!");
      }
      // Normal content.
      else if (tag.Equals(""))
      {
        // Preprocess the line to avoid doing same stuff 25+ times.
        _currentEnemy.Tactics.AddLine(LinkGenerator.CreatePageLinks(LinkGenerator.CheckLinkSyntax(data)));
      }
      else if (tag.Equals("type") || tag.Equals("effect") || tag.Equals("cooldown") || tag.Equals("additional") || tag.Equals("animation"))
        Helper.ShowWarning("Missing attack name (\"attack='name'\")!");
      else
        Helper.ShowWarning("Unrecognized tag: " + tag);
      return 0;
    }


    /***********************************************************************************************
     * AttackLoop / 2014-08-01 / Wethospu                                                          *
     *                                                                                             *
     * Sub process loop for enemy attacks.                                                         *
     *                                                                                             *
     * tag: Tag of the line.                                                                       *
     * data: Data of the line.                                                                     *
     *                                                                                             *
     ***********************************************************************************************/

    static int AttackLoop(string tag, string data)
    {
      // Add old attack and start a new one.
      if (tag.Equals("attack"))
      {
        if (data.Length == 0)
          Helper.ShowWarning("Missing info. Use \"attack='name'\"!");

        if (_currentAttack != null)
          _currentEnemy.Attacks.Add(_currentAttack);
        _currentAttack = new Attack(LinkGenerator.CreatePageLinks(LinkGenerator.CheckLinkSyntax(data)));
        _currentEffect = null;
      }
      // Tags from main loop. Save attack and exit this loop.
      else if (tag.Equals("name") || tag.Equals("copy") || tag.Equals("potion"))
      {
        if (_currentAttack != null)
          _currentEnemy.Attacks.Add(_currentAttack);
        _currentAttack = null;
        _currentEffect = null;
        return -1;
      }
      else if (tag.Equals("id"))
      {
        if (data.Length > 0)
        {
          _currentAttack.LoadAttributes(Helper.ParseI(data), _currentEnemy.Attributes);
        }
        else
          Helper.ShowWarning("Missing info. Use \"id=number\".");
      }
      // Tags from effect loop. Exit immediately.
      else if (tag.Equals("effect"))
      {
        return 1;
      }
      else if (tag.Equals("cooldown"))
      {
        if (data.Length > 0)
          _currentAttack.Cooldown = Helper.ParseD(data);
        else
          Helper.ShowWarning("Missing info. Use \"cooldown='number'\".");
      }
      else if (tag.Equals("additional"))
      {
        // Treat additional as an effect. / 2015-09-22 / Wethospu
        if (data.Length == 0)
          Helper.ShowWarning("Missing info. Use \"additional='text'\".");
        if (_currentEffect != null)
          _currentAttack.Effects.Add(_currentEffect);
        var lower = data.ToLower();
        // Check for interesting tags. / 2015-09-22 / Wethospu
        if (lower.Contains("can't be blocked") || lower.Contains("can't block"))
          _currentEnemy.Tags.Add("can't block");
        if (lower.Contains("can't be evaded") || lower.Contains("can't evade"))
          _currentEnemy.Tags.Add("can't evade");
        _currentEffect = new Effect(LinkGenerator.CreatePageLinks(LinkGenerator.CheckLinkSyntax(data)));
      }
      else if (tag.Equals("animation"))
      {
        if (data.Length > 0)
        {
          if (data.Contains(':') && !data.Contains('='))
            Helper.ShowWarning("Potentially use of wrong syntax. Use \"animation='pre cast'|'time'|'after cast'\" !");
          _currentAttack.Animation = data;
        }
        else
          Helper.ShowWarning("Missing info. Use \"animation='pre cast'|'time'|'after cast'\" !");
      }
      else if (tag.Equals("image"))
      {
        if (data.Length > 0)
        {
          _currentAttack.Medias.Add(new Media(data));
        }
        else
          Helper.ShowWarning("Missing info. Use \"image='imagelink'\"!");
      }
      else if (tag.Equals("subeffect"))
        Helper.ShowWarning("Missing attack effect (\"effect='type'\")!");
      else if (tag.Equals(""))
        Helper.ShowWarning("Something wrong with line " + data + ".");
      else
        Helper.ShowWarning("Unrecognized tag: " + tag);

      return 0;
    }


    /***********************************************************************************************
     * EffectLoop / 2014-08-01 / Wethospu                                                          *
     *                                                                                             *
     * Sub process loop for attack effects.                                                        *
     *                                                                                             *
     * tag: Tag of the line.                                                                       *
     * data: Data of the line.                                                                     *
     *                                                                                             *
     ***********************************************************************************************/

    private static int EffectLoop(string tag, string data)
    {
      // Add old effect and start a new one.
      if (tag.Equals("effect"))
      {
        if (data.Length == 0)
          Helper.ShowWarning("Missing info. Use \"effect='type'\"!");
        if (_currentEffect != null)
          _currentAttack.Effects.Add(_currentEffect);
        var type = data.ToLower();
        foreach (var enemyTag in Constants.AttackTypeTags)
        {
          if (type.Contains(enemyTag))
            _currentEnemy.Tags.Add(enemyTag);
        }
        _currentEffect = new Effect(LinkGenerator.CreatePageLinks(LinkGenerator.CheckLinkSyntax(data)));
      }
      // Tag from attack loop. Save effect and exit this loop.
      else if (tag.Equals("attack"))
      {
        if (_currentEffect != null)
          _currentAttack.Effects.Add(_currentEffect);
        _currentEffect = null;
        return -1;
      }
      // Tag from main loop. Save both effect and attack and then exit this loop.
      else if (tag.Equals("name") || tag.Equals("potion") || tag.Equals("copy"))
      {
        if (_currentEffect != null)
        {
          _currentAttack.Effects.Add(_currentEffect);
          _currentEnemy.Attacks.Add(_currentAttack);
        }
        _currentEffect = null;
        _currentAttack = null;
        return -2;
      }
      else if (tag.Equals("count"))
      {
        if (data.Length > 0)
        {
          if (data.Equals("?"))
            _currentEffect.HitCount = -1;
          else
            _currentEffect.HitCount = Helper.ParseI(data);
          if (_currentEffect.HitCount == 0)
            Helper.ShowWarning("Hit count can't be zero.");
        }
         
        else
          Helper.ShowWarning("Missing info. Use \"count='number'\"");
      }
      else if (tag.Equals("length"))
      {
        if (data.Length > 0)
          _currentEffect.HitLength = Helper.ParseD(data);
        else
          Helper.ShowWarning("Missing info. Use \"length='number'\"");
      }
      else if (tag.Equals("frequency"))
      {
        if (data.Length > 0)
          _currentEffect.HitFrequency = Helper.ParseD(data);
        else
          Helper.ShowWarning("Missing info. Use \"frequency='number'\"");
      }
      // Add subeffects to the effect.
      else if (tag.Equals("subeffect"))
      {
        if (data.Length > 0)
        {
          data = LinkGenerator.CheckLinkSyntax(data);
          _currentEffect.SubEffects.Add(data);
        }
        else
          Helper.ShowWarning("Missing info. Use TODO!");
      }
      // Error handling for wrongly placed tags.
      else if (tag.Equals("additional") || tag.Equals("cooldown") || tag.Equals("animation"))
        Helper.ShowWarning("Wrong position for tag " + tag + ". Move above any type-tags.");
      else if (tag.Equals(""))
        Helper.ShowWarning("Something wrong with line " + data + ".");
      else
        Helper.ShowWarning("Unrecognized tag: " + tag);
      return 0;
    }

    /***********************************************************************************************
     * FindEnemy / 2014-08-01 / Wethospu                                                           *
     *                                                                                             *
     * Finds and returns a previously introduced enemy which matches search data.                  *
     *                                                                                             *
     * data: Search string.                                                                        *
     * enemies: List of enemies to search from.                                                    *
     *                                                                                             *
     ***********************************************************************************************/

    private static Enemy FindEnemy(string data, List<Enemy> enemies)
    {
      if (data.Length == 0)
      {
        Helper.ShowWarning("Missing info. Use syntax \"copy" + Constants.TagSeparator + "'name'|'category'|'path1':'path2':'pathN'\"");
        return null;
      }
      // Check whether copy by id is used. / 2015-10-02 / Wethospu
      try
      {
        var id = int.Parse(data);
        foreach (var enemy in enemies)
        {
          if (enemy.InternalIds.Contains(id))
            return enemy;
        }
        Helper.ShowWarning("No enemy found for copy " + data + ". Change parameters, add the missing enemy, change order of enemies or check the readme.");
      }
      catch { }

      if (data.Contains(Constants.TagSeparator))
        Helper.ShowWarning("'" + Constants.TagSeparator + "' found. Use syntax \"copy" + Constants.TagSeparator + "'name'|'category'|'path1':'path2':'pathN'\"");
      var dataSplit = data.Split('|');
      var name = dataSplit[0];
      var category = "";
      var path = "";
      if (dataSplit.Length > 1)
        category = dataSplit[1];
      if (dataSplit.Length > 2)
      {
        if (dataSplit[2].Contains(' '))
          Helper.ShowWarning("' ' found. Use syntax \"copy" + Constants.TagSeparator + "'name'|'category'|'path1':'path2':'pathN'\"");
        path = dataSplit[2].Replace(':', '|');
      }
      var foundEnemies = Gw2Helper.FindEnemies(enemies, name, category, path);
      if (foundEnemies.Count == 0)
      {
        Helper.ShowWarning("No enemy found for copy. Change parameters, add missing enemy, change order of enemies or check syntax file.");
        return null;
      }
      if (foundEnemies.Count > 1)
        Helper.ShowWarning("Multiple enemies found for copy. Add more parameters or check syntax file.");
      return foundEnemies[0];
    }

    /***********************************************************************************************
     * HandleIndexFile / 2014-08-01 / Wethospu                                                     *
     *                                                                                             *
     * Adds generated enemies to the index file.                                                   *
     *                                                                                             *
     * enemies: Generated enemies.                                                                 *
     * dungeon: Current dungeon.                                                                   *
     * indexFile: Output.                                                                          *
     *                                                                                             *
     ***********************************************************************************************/

    private static void HandleIndexFile(IReadOnlyList<Enemy> enemies, StringBuilder indexFile, DataCollector dungeonData)
    {
      if (indexFile == null)
        return;
      foreach (var enemy in enemies)
      {
        indexFile.Append(enemy.Name).Append("|").Append(Helper.Simplify(enemy.Name)).Append("|").Append(enemy.Rank);
        indexFile.Append("|").Append(enemy.Attributes.Family.GetInternal()).Append("|").Append(string.Join(":", enemy.Paths));
        // Store index so that the enemy can be found faster when searching.
        indexFile.Append("|").Append(enemy.Index);
        // Generate tag string. / 2015-07-18 / Wethospu
        var builder = new StringBuilder();
        foreach (var tag in enemy.Tags)
          builder.Append(dungeonData.ConvertTag(tag));
        indexFile.Append("|").Append(builder.ToString());
        
        indexFile.Append(Constants.LineEnding);
      }
    }

    /***********************************************************************************************
     * GenerateFile / 2015-10-05 / Wethospu                                                        *
     *                                                                                             *
     * Creates html data file from gathered info.                                                  *
     *                                                                                             *
     * enemies: List of enemies.                                                                   *
     * indexFile: Output. Enemies get indexed for faster search.                                   *
     * dungeonData: Output. Collected data from the enemies.                                       *
     *                                                                                             *
     ***********************************************************************************************/

    public static void GenerateFile(List<Enemy> enemies, StringBuilder indexFile, DataCollector dungeonData)
    {
      // Separate enemies (100 enemies per file) to reduce the initial loading time. / 2015-10-08 / Wethospu
      var enemyFile = new StringBuilder();
      enemyFile.Append(Constants.InitialdataHtml).Append(Constants.LineEnding);
      for (var i = 0; i < enemies.Count; i++)
      {
        enemyFile.Append(enemies[i].ToHtml(enemies));
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
            Helper.ShowWarningMessage("File " + fileName + " in use.");
          }
          enemyFile = new StringBuilder();
          enemyFile.Append(Constants.InitialdataHtml).Append(Constants.LineEnding);
        }
      }
      for (var i = 0; i < enemies.Count; i++)
      {
        // Add enemy info to the data collector. / 2015-08-17 / Wethospu
        if (dungeonData != null)
        {
          dungeonData.AddRace(enemies[i].Attributes.Family.GetDisplay());
          dungeonData.AddCategory(enemies[i].Rank);
          foreach (var tag in enemies[i].Tags)
            dungeonData.AddTag(tag);
        }
      }
      HandleIndexFile(enemies, indexFile, dungeonData);
    }
  }
}
