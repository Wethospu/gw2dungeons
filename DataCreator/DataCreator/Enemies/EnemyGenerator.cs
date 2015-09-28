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
    * dungeon: Enemies will be generated for this dungeon.                                         *
    * enemyAttributes: Datamined enemy attributes and other info.                                  *
    *                                                                                              *
    ***********************************************************************************************/

    public static List<Enemy> GenerateEnemies(string dungeon, Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      var rawDataLocation = Constants.DataEnemyRaw + dungeon + Constants.DataEnemySuffix + ".txt";
      string[] lines;
      if (File.Exists(rawDataLocation))
        lines = File.ReadAllLines(rawDataLocation, Constants.Encoding);
      else
      {
        Helper.ShowWarningMessage("File " + rawDataLocation + " doesn't exist!");
        return null;
      }
      var enemies = new List<Enemy>();
      // Info about connection between enemy type and how common they are in the dungeon.
      // This affects damage calculations because user can choose their potion usage.
      var typeToPotion = new Dictionary<string, string>();
      Helper.CurrentFile = rawDataLocation;
      for (var row = 0; row < lines.Length; row++)
      {
        Helper.InitializeWarningSystem(row + 1, lines[row]);
        HandleLine(lines[row], enemies, typeToPotion, enemyAttributes);
      }
      // Add the last enemy.
      if (_currentEffect != null && _currentAttack != null)
        _currentAttack.Effects.Add(_currentEffect);
      if (_currentAttack != null && _currentEnemy != null)
        _currentEnemy.Attacks.Add(_currentAttack);
      if (_currentEnemy != null)
        enemies.Add(_currentEnemy);
      // Reset internal state.
      Helper.InitializeWarningSystem(-1, "");
      _currentEnemy = null;
      _currentAttack = null;
      _currentEffect = null;
      // Sorting is only done for consistency (to have some order instead of chaos).
      enemies.Sort();
      // Set up unique indexes. These are used for enemy links and enemy tactics (tab system). / 2015-08-11 / Wethospu
      for (var i = 0; i < enemies.Count; i++)
      {
        enemies[i].Index = i + Constants.UniqueIndexCounter;
        enemies[i].FileIndex = i;
      }   
      Constants.UniqueIndexCounter += enemies.Count;

      return enemies;
    }

    /***********************************************************************************************
     * HandleLine / 2014-08-01 / Wethospu                                                          *
     *                                                                                             *
     * Processes one line of base data.                                                            *
     *                                                                                             *
     * line: Line to process.                                                                      *
     * enemies: Output. List of processed enemies.                                                 *
     * typeToPotion: Input/Output. Information about enemy types and potion effectiviness.         *
     *                                                                                             *
     ***********************************************************************************************/

    // 0 = main loop, 1 = attack loop, 2 = effect loop.
    private static int _mode;
    private static void HandleLine(string line, List<Enemy> enemies, Dictionary<string, string> typeToPotion, Dictionary<string, EnemyAttributes> enemyAttributes)
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
        returnValue = EnemyLoop(tag, data, typeToPotion, enemies, enemyAttributes);
      else if (_mode == 1)
        returnValue = AttackLoop(tag, data);
      else if (_mode == 2)
        returnValue = EffectLoop(tag, data);
      _mode += returnValue;
      // Moving between loop modes only happen when detecting stuff from other loops.
      if (returnValue != 0)
      {
        // Line has to be rechecked on the new loop.
        HandleLine(line, enemies, typeToPotion, enemyAttributes);
      }
    }

    /***********************************************************************************************
     * EnemyLoop / 2014-08-01 / Wethospu                                                           *
     *                                                                                             *
     * Main process loop for enemies.                                                              *
     *                                                                                             *
     * tag: Tag of the line.                                                                       *
     * data: Data of the line.                                                                     *
     * typeToPotion: Input/Output. Information about enemy types and potion effectiviness.         *
     * enemies: Output. List of processed enemies.                                                 *
     *                                                                                             *
     ***********************************************************************************************/

    private static int EnemyLoop(string tag, string data, Dictionary<string, string> typeToPotion, List<Enemy> enemies, Dictionary<string, EnemyAttributes> enemyAttributes)
    {
      if (tag.Equals("potion"))
      {
        if (data.Length > 0)
        {
          var dataSplit = data.Split('|');
          if (dataSplit.Length > 1)
          {
            var potionUsage = dataSplit[1].ToLower();
            if (potionUsage.Equals("none") || potionUsage.Equals("main") || potionUsage.Equals("side"))
            {
              if (typeToPotion.ContainsKey(dataSplit[0].ToLower()))
                typeToPotion[dataSplit[0].ToLower()] = potionUsage;
              else
                typeToPotion.Add(dataSplit[0].ToLower(), potionUsage);
            }
            else
              Helper.ShowWarning("Wrong potion usage parameter. Use\"main\", \"side\" or \"none\"!");

          }
          else
            Helper.ShowWarning("Incorrect syntax or missing info. Use \"potion='enemy race'|'potion usage'\"!");

        }
        else
          Helper.ShowWarning("Incorrect syntax or missing info. Use \"potion='enemy race'|'potion usage'\"!");

      }
      else if (tag.Equals("copy"))
      {
        if (_currentEnemy != null)
          enemies.Add(_currentEnemy);
        var found = FindEnemy(data, enemies);
        if (found != null)
        {
          _currentEnemy = new Enemy(found) { Name = "" };
          if (typeToPotion.ContainsKey(_currentEnemy.Attributes.Family.ToLower()))
            _currentEnemy.Potion = typeToPotion[_currentEnemy.Attributes.Family.ToLower()];
        }
        else
          Helper.ShowWarning("Copying failed. Enemy not found!");
      }
      else if (tag.Equals("name"))
      {
        if (data.Length > 0)
        {
          if (_currentEnemy != null && _currentEnemy.Name.Length > 0)
          {
            enemies.Add(_currentEnemy);
            if (_currentEnemy.Path.Length == 0)
              Helper.ShowWarning("Path not set for enemy " + _currentEnemy.Name);
          }
          // Copied enemies exist but have no name.
          if (_currentEnemy != null && _currentEnemy.Name.Length == 0)
            _currentEnemy.Name = data;
          else
          {
            _currentEnemy = new Enemy(data) { Name = data };
            if (data.Contains('_'))
              Helper.ShowWarning("Enemy name " + data + "  containts '_'. Replace them with ' '!");
          }
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
          var ids = data.Split('|');
          foreach (var id in ids)
          {
            if (enemyAttributes.ContainsKey(id))
            {
              // Enemy can have multiple sexes if there are model variations. / 2015-09-28 / Wethospu
              // Store old one to get both added. / 2015-09-28 / Wethospu
              var oldSexes = _currentEnemy.Attributes.Sex;
              _currentEnemy.Attributes = enemyAttributes[id];
              if (oldSexes.Length > 0)
              {
                var sexes = oldSexes.Split('|');
                // If the sex is already there it can be ignored. / 2015-09-28 / Wethospu
                if (sexes.Contains(_currentEnemy.Attributes.Sex))
                  _currentEnemy.Attributes.Sex = oldSexes;
                else
                  _currentEnemy.Attributes.Sex = oldSexes + "|" + _currentEnemy.Attributes.Sex;
              }
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
          _currentEnemy.Path = data.ToLower();
      }
      else if (tag.Equals("category"))
      {
        if (_currentEnemy == null)
          Helper.ShowWarning("Enemy not initialized with name.");
        else if (data.Length > 0)
        {
          _currentEnemy.Category = data.ToLower();
          if (!LinkGenerator.EnemyCategories.Contains(_currentEnemy.Category))
            Helper.ShowWarning("Category " + _currentEnemy.Category + " not recognized. Check syntax for correct categories.");
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
        else if (_currentEnemy.Category.Length == 0)
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
          Helper.ShowWarning("Missing info. Use \"tactic='tactic1'|'tactic2'|'tacticN'\"!");
      }
      else if (tag.Equals("type") || tag.Equals("health") || tag.Equals("armor") || tag.Equals("condition") || tag.Equals("toughness"))
      {
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
      // Tags from effect loop. Exit immediately.
      else if (tag.Equals("effect"))
      {
        return 1;
      }
      else if (tag.Equals("cooldown"))
      {
        if (data.Length > 0)
        {
          if (data.Contains(" s"))
            Helper.ShowWarning("Did you accidentaly include unnecessary 's'? Please remove!");
          _currentAttack.Cooldown = data;
        }
        else
          Helper.ShowWarning("Missing info. Use \"cooldown='number'\" or  \"cooldown='text'\"!");
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
          _currentEffect.HitCount = Helper.ParseI(data);
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
      if (data.Contains(Constants.TagSeparator))
        Helper.ShowWarning("'" + Constants.TagSeparator + "' found. Use syntax \"copy" + Constants.TagSeparator + "'name'|'category'|'path1':'path2':'pathN'\"");
      var dataSplit = data.Split('|');
      var name = dataSplit[0];
      var category = "";
      var path = "";
      var level = 0;
      if (dataSplit.Length > 1)
        category = dataSplit[1];
      if (dataSplit.Length > 2)
      {
        if (dataSplit[2].Contains(' '))
          Helper.ShowWarning("' ' found. Use syntax \"copy" + Constants.TagSeparator + "'name'|'category'|'path1':'path2':'pathN'\"");
        path = dataSplit[2].Replace(':', '|');
      }
      if (dataSplit.Length > 3)
        level = Helper.ParseI(dataSplit[3]);

      var foundEnemies = Gw2Helper.FindEnemies(enemies, name, category, level, path);
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

    private static void HandleIndexFile(IReadOnlyList<Enemy> enemies, string dungeon, StringBuilder indexFile, DataCollector dungeonData)
    {
      if (dungeon == null || indexFile == null)
        return;
      foreach (var enemy in enemies)
      {
        indexFile.Append(enemy.Name).Append("|").Append(Helper.Simplify(enemy.Name)).Append("|").Append(enemy.Category);
        indexFile.Append("|").Append(enemy.Attributes.Family).Append("|").Append(LinkGenerator.CurrentDungeon.ToLower()).Append("|").Append(enemy.Path.Replace('|', ':'));
        // Store index so that the enemy can be found faster when searching.
        indexFile.Append("|").Append(enemy.FileIndex);
        // Generate tag string. / 2015-07-18 / Wethospu
        var builder = new StringBuilder();
        foreach (var tag in enemy.Tags)
          builder.Append(dungeonData.ConvertTag(tag));
        indexFile.Append("|").Append(builder.ToString());
        
        indexFile.Append(Constants.LineEnding);
      }
    }

    /***********************************************************************************************
     * GenerateFiles / 2014-08-01 / Wethospu                                                       *
     *                                                                                             *
     * Creates html data file from gathered info.                                                  *
     *                                                                                             *
     * enemies: List of enemies.                                                                   *
     * dungeon: Current dungeon for correct file name.                                             *
     * indexFile: Output. Enemies get indexed for faster search.                                   *
     * dungeonData: Output. Collected data from the enemies.                                       *
     *                                                                                             *
     ***********************************************************************************************/

    public static void GenerateFiles(List<Enemy> enemies, string dungeon, StringBuilder indexFile, DataCollector dungeonData)
    {
      // Add enemies to the index file and build html.
      var enemyFile = new StringBuilder();
      enemyFile.Append(Constants.InitialdataHtml).Append(Constants.LineEnding);
      foreach (var enemy in enemies)
        enemyFile.Append(enemy.ToHtml(enemies));

      var fileName = Constants.DataOutput + Constants.DataEnemyResult + dungeon.ToLower() + ".htm";
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
      for (var i = 0; i < enemies.Count; i++)
      {
        // Add enemy info to the data collector. / 2015-08-17 / Wethospu
        if (dungeonData != null)
        {
          if (!enemies[i].Attributes.Family.Equals(""))
            dungeonData.AddRace(enemies[i].Attributes.Family);
          dungeonData.AddCategory(enemies[i].Category);
          foreach (var tag in enemies[i].Tags)
            dungeonData.AddTag(tag);
        }
      }
      HandleIndexFile(enemies, dungeon, indexFile, dungeonData);
    }
  }
}
