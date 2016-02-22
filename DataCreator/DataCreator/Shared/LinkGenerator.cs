using System;
using System.Collections.Generic;
using System.Text;
using DataCreator.Enemies;
using System.IO;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using DataCreator.Utility;
using System.Drawing.Drawing2D;

namespace DataCreator.Shared
{

  /***********************************************************************************************
   * LinkGenerator / 2014-08-01 / Wethospu                                                       *
   *                                                                                             *
   * Functions to generate enemy and other links for encounters and enemies.                     *
   *                                                                                             *
   ***********************************************************************************************/

  public class LinkGenerator
  {
    public static string[] EnemyCategories = { "normal", "veteran", "elite", "champion", "legendary", "structure", "trap", "skill", "ally", "bundle" };

    private static HashSet<string> downloadData = new HashSet<string>();

    private static string _currentDungeon = "";
    public static string CurrentDungeon
    {
      get
      {
        if (_currentDungeon.Length == 0)
          ErrorHandler.ShowWarning("Dungeon isn't defined! Something is seriously wrong.");
        return _currentDungeon;
      }
      set
      {
        _currentDungeon = value;
      }
    }


    /***********************************************************************************************
     * CreateEnemyLinks / 2014-07-24 / Wethospu                                                    * 
     *                                                                                             * 
     * Creates enemy links. Verifies that linked enemy exists and fills missing info.              *
     *                                                                                             *
     * Returns processed string.                                                                   *
     * str: String to edit.                                                                        *
     * path: Information about the enemy link origin.                                              *
     * scale: Fractal scale.                                                                       *
     * enemyData: List of enemies.                                                                 *
     *                                                                                             * 
     ***********************************************************************************************/

    public static string CreateEnemyLinks(string str, string path, List<Enemy> enemyData, int scale = 0)
    {
      // Iterator over the string and look for links.
      for (var iterator = 0; iterator < str.Length; iterator++)
      {
        var link = FindNextLink(str, ref iterator);
        if (link == null)
          continue;
        // Only accept enemy links.
        if (!link[0].ToLower().Equals(Constants.LinkEnemy))
          continue;

        var replaceWith = CreateEnemyLink(link[1], path, scale, enemyData);
        var toReplace = link[0] + Constants.LinkChar + link[1];
        // Replace all doesn't work because some links are found partially in other links.
        str = Helper.ReplaceFirst(str, toReplace, replaceWith);
        // Move iterator based on string lengths.
        iterator = iterator - toReplace.Length + replaceWith.Length;
      }
      return str;
    }

    /***********************************************************************************************
     * GetEnemiesFromLinks / 2015-08-09 / Wethospu                                                 * 
     *                                                                                             * 
     * Same implementation as above, but returns found enemies instead of links.                   *
     *                                                                                             *
     * Returns processed string.                                                                   *
     * str: String to edit.                                                                        *
     * path: Needed for enemy links.                                                               *
     * enemyData: List of enemies.                                                                 *
     *                                                                                             * 
     ***********************************************************************************************/

    public static List<Enemy> GetEnemiesFromLinks(string str, string path, List<Enemy> enemyData)
    {
      var enemies = new List<Enemy>();
      // Iterator over the string and look for links.
      for (var iterator = 0; iterator < str.Length; iterator++)
      {
        var link = FindNextLink(str, ref iterator);
        if (link == null)
          continue;
        // Only accept enemy links.
        if (!link[0].ToLower().Equals(Constants.LinkEnemy))
          continue;

        var foundEnemies = GetEnemiesFromLink(link[1], path, enemyData);
        foreach (var enemy in foundEnemies)
          enemies.Add(enemy);
      }
      return enemies;
    }


    /***********************************************************************************************
     * CreateLinks / 2014-07-24 / Wethospu                                                         * 
     *                                                                                             * 
     * Creates page links. Enemy links have to created separately when enemie have been loaded.    *
     *                                                                                             *
     * Returns processed string.                                                                   *
     * str: String to edit.                                                                        *
     *                                                                                             * 
     ***********************************************************************************************/

    public static string CreatePageLinks(string str)
    {
      // Iterator over the string and look for links.
      for (var iterator = 0; iterator < str.Length; iterator++)
      {
        var link = FindNextLink(str, ref iterator);
        if (link == null)
          break;
        // Ignore enemy links (have to be built later).
        if (link[0].ToLower().Equals(Constants.LinkEnemy))
          continue;

        // Check that link type is an actualy link (for example not a html tag).
        var index = Array.IndexOf(Constants.LinkTypes, link[0].ToLower());
        if (index < 0 || index >= Constants.LinkTypes.Length)
          continue;

        var toReplace = link[0] + Constants.LinkChar + link[1];
        var replaceWith = CreatePageLink(link[0], link[1]);
        // Replace all doesn't work because some links are found partially in other links.
        str = Helper.ReplaceFirst(str, toReplace, replaceWith);
        // Move iterator based on string lengths.
        iterator = iterator - toReplace.Length + replaceWith.Length;
      }
      return str;
    }

    /***********************************************************************************************
     * RemoveLinks / 2014-07-24 / Wethospu                                                         * 
     *                                                                                             * 
     * Removes all links and replaces them with shown text.                                        *
     * This is needed for encounter names when they are added to the index file.                   *
     *                                                                                             *
     * Returns processed string.                                                                   *
     * str: String to edit.                                                                        *
     *                                                                                             * 
     ***********************************************************************************************/

    public static string RemoveLinks(string str)
    {
      // Iterator over the string and look for links.
      for (var iterator = 0; iterator < str.Length; iterator++)
      {
        var link = FindNextLink(str, ref iterator);
        if (link == null)
          continue;

        string shownText;
        // Enemy links have different syntax.
        if (link[0].ToLower().Equals(Constants.LinkEnemy))
          shownText = CreateEnemyLinkText(link[1]);
        else if (link[0].ToLower().Equals(Constants.LinkWiki))
          shownText = CreateLinkText(link[1], true, false);
        else
          shownText = CreateLinkText(link[1], false, false);

        var toRemove = link[0] + Constants.LinkChar + link[1];
        str = str.Replace(toRemove, shownText);
        // Move iterator based on string lengths.
        iterator = iterator - toRemove.Length + shownText.Length;
      }
      return str;
    }
    /***********************************************************************************************
     * CheckLinkSyntax / 2014-07-24 / Wethospu                                                     * 
     *                                                                                             * 
     * Checks syntax of links. Prints errors and removes problematic links.                        *
     *                                                                                             *
     * Returns cleaned string.                                                                     *
     * str: String to check.                                                                       *
     *                                                                                             * 
     ***********************************************************************************************/

    public static string CheckLinkSyntax(string str)
    {
      // Iterator over the string and look for links.
      for (var iterator = 0; iterator < str.Length; iterator++)
      {
        var link = FindNextLink(str, ref iterator);
        if (link == null || link[0].Length == 0)
          continue;

        var shownText = "";

        // Check that type exists.
        var index = Array.IndexOf(Constants.LinkTypes, link[0].ToLower());
        if (index < 0 || index >= Constants.LinkTypes.Length)
        {
          ErrorHandler.ShowWarning("Link type " + link[0] + " not recognized. Please fix!");
        }
        else
        {
          // Enemy links have different syntax.
          if (link[0].ToLower().Equals(Constants.LinkEnemy))
            shownText = CreateEnemyLinkText(link[1], true);
          else if (link[0].ToLower().Equals(Constants.LinkWiki))
            shownText = CreateLinkText(link[1], true, true);
          else
            shownText = CreateLinkText(link[1], false, true);
        }

        // No shown text means an error so remove broken link.
        if (shownText.Equals(""))
          str = str.Replace(link[0] + Constants.LinkChar + link[1], shownText);
      }
      return str;
    }


    /***********************************************************************************************
     * CreateEnemyLink / 2014-07-24 / Wethospu                                                     * 
     *                                                                                             * 
     * Creates an enemy link from link data. Finds it from list of enemies to fill missing data.   *
     *                                                                                             *
     * Returns created link.                                                                       *
     * linkData: Data for the link.                                                                *
     * enemies: List of enemies to verify links and filling missing data.                          *
     *                                                                                             * 
     ***********************************************************************************************/

    private static string CreateEnemyLink(string linkData, string path, int scale, List<Enemy> enemies)
    {
      var shownText = CreateEnemyLinkText(linkData);

      var enemiesToLink = new List<Enemy>();
      var enemyLevels = new List<int>();
      // Get linked enemy data.
      var enemySplit = linkData.Split('|');
      foreach (var enemyStr in enemySplit)
      {
        var subSplit = enemyStr.Split(':');
        var name = subSplit[0].ToLower();
        var rank = "";
        var level = 0;
        // Ignore shown texts.
        if (name.Equals("text"))
          continue;
        // Get enemy data.
        if (subSplit.Length > 1)
          rank = subSplit[1].ToLower();
        if (subSplit.Length > 2)
          level = Helper.ParseI(subSplit[2].ToLower());
        enemyLevels.Add(level);
        // Find enemy.
        var foundEnemies = Gw2Helper.FindEnemies(enemies, name, rank, path);
        if (foundEnemies.Count == 0)
        {
          ErrorHandler.ShowWarningMessage("No enemy found for enemy " + name + " with link " + linkData + " and path " + path + ". Change parameters, add missing enemy or check syntax file.");
          continue;
        }
        if (foundEnemies.Count > 1)
          ErrorHandler.ShowWarningMessage("Multiple enemies found for enemy " + name + " with link " + linkData + " and path " + path + ". Add more parameters or check syntax file.");
        // Use data from the first enemy.
        // Note: While this could be used to for example link every elite enemy, it would be extremely inefficient.
        // Instead, new link type should be added and link loading function in javascript should be expanded to allow jokers.
        enemiesToLink.Add(foundEnemies[0]);
      }
      if (enemiesToLink.Count == 0)
        return shownText;
      // Linkable enemies found. Start building the link.
      // Syntax:
      // <span class="'main rank' enemy-button" data-name="'enemy names'" data-rank="'enemy categories" data-level="'enemy levels'" data-path="'path'" data-level"'level'">'shown text'</span>
      var link = new StringBuilder();
      link.Append("<span class=\"").Append(enemiesToLink[0].Rank).Append(" enemy-button\" data-index=\"");
      // Add enemy names.
      for (var index = 0; index < enemiesToLink.Count; index++)
      {
        link.Append(enemiesToLink[index].Index);
        if (index + 1 < enemiesToLink.Count)
          link.Append(':');
      }
      // Add the path to load correct map. / 2015-10-08 / Wethospu
      link.Append("\" data-path=\"").Append(path).Append("\"");
      // Add enemy levels to allow changing enemy level dynamically. / 2015-09-28 / Wethospu
      link.Append(" data-level=\"");
      for (var index = 0; index < enemyLevels.Count; index++)
      {
        link.Append(enemyLevels[index]);
        if (index + 1 < enemiesToLink.Count)
          link.Append(':');
      }
      if (scale > 0)
        link.Append("\" data-scale=\"").Append(scale);
      link.Append("\"> ");
      // Add shown text.
      link.Append(shownText).Append("</span>");
      return link.ToString();
    }

    /***********************************************************************************************
    * GetEnemiesFromLink / 2015-08-09 / Wethospu                                                  * 
    *                                                                                             * 
    * Same as above but returns found enemies instead of creating links.                          *
    *                                                                                             *
    * Returns found enemies.                                                                      *
    * linkData: Data for the link.                                                                *
    * path: Dungeon path. Needed to separate some enemies.                                        *
    * enemies: List of enemies to verify links and filling missing data.                          *
    *                                                                                             * 
    ***********************************************************************************************/

    private static List<Enemy> GetEnemiesFromLink(string linkData, string path, List<Enemy> enemies)
    {
      var enemiesToLink = new List<Enemy>();
      // Get linked enemy data.
      var enemySplit = linkData.Split('|');
      foreach (var enemyStr in enemySplit)
      {
        var subSplit = enemyStr.Split(':');
        var name = subSplit[0].ToLower();
        var rank = "";
        // Ignore shown texts.
        if (name.Equals("text"))
          continue;
        // Get enemy data.
        if (subSplit.Length > 1)
          rank = subSplit[1].ToLower();
        // Find enemy.
        var foundEnemies = Gw2Helper.FindEnemies(enemies, name, rank, path);
        if (foundEnemies.Count == 0)
        {
          ErrorHandler.ShowWarningMessage("No enemy found for enemy " + name + " with link " + linkData + " and path " + path + ". Change parameters, add missing enemy or check syntax file.");
          continue;
        }
        if (foundEnemies.Count > 1)
          ErrorHandler.ShowWarningMessage("Multiple enemies found for enemy " + name + " with link " + linkData + " and path " + path + ". Add more parameters or check syntax file.");
        // Use data from the first enemy.
        // Note: While this could be used to for example link every elite enemy, it would be extremely inefficient.
        // Instead, new link type should be added and link loading function in javascript should be expanded to allow jokers.
        enemiesToLink.Add(foundEnemies[0]);
      }
      return enemiesToLink;
    }


    /***********************************************************************************************
     * FindNextLink / 2014-07-24 / Wethospu                                                        * 
     *                                                                                             * 
     * Finds next enemy rank from the string.                                                      *
     *                                                                                             *
     * Returns found link type and data. Returns null if nothing good was found.                   *
     * str: String to search from.                                                                 *
     * startIndex: In/Out. Index to start search -> end index of found rank.                       *
     *                                                                                             * 
     ***********************************************************************************************/

    private static string[] FindNextLink(string str, ref int startIndex)
    {
      var linkIndex = str.IndexOf(Constants.LinkChar, startIndex);
      if (linkIndex < 0 || linkIndex >= str.Length)
      {
        // Nothing was found.
        startIndex = str.Length;
        return null;
      }
      // Backward search to get link type.
      var typeIndex = 1 + Helper.LastIndexOf(str, new[] { ' ', '(', '[' }, linkIndex);
      var type = str.Substring(typeIndex, linkIndex - typeIndex);
      //// Forward search to get link data.
      // Get end of the sentence (link will end before it).
      var sentenceEnd = str.IndexOf(' ', linkIndex + 1);
      if (sentenceEnd < 0)
        sentenceEnd = str.Length;
      var separator = Helper.LastIndexOf(str, new[] { ':', '|' }, sentenceEnd - 1);
      // Points to the character after the link.
      int dataEndIndex;
      if (separator > 0 && separator < sentenceEnd && separator > linkIndex)
        // With proper separator just search end for shown text.
        dataEndIndex = Helper.FirstIndexOf(str, new[] { '.', ',', ';', ' ', ')', ']' }, separator + 1);
      else
      {
        // With just link, '.', ':', ';' or ',' at end shouldn't be included.
        dataEndIndex = sentenceEnd;
        if (str[dataEndIndex - 1] == '.' || str[dataEndIndex - 1] == ':' || str[dataEndIndex - 1] == ',' || str[dataEndIndex - 1] == ';')
          dataEndIndex--;
      }
      var data = str.Substring(linkIndex + 1, dataEndIndex - linkIndex - 1);

      startIndex = dataEndIndex;
      return new[] { type, data };
    }


    private static string Http { get { return "http://"; } }
    private static string Https { get { return "https://"; } }
    private static string Images { get { return "media/dungeonimages/"; } }
    private static string Youtube { get { return "youtu.be/"; } }
    private static string LinkClass { get { return "overlay-link"; } }
    private static string RecordStart { get { return "<span class=\"record-run\">";  } }
    private static string RecordEnd { get { return "</span>"; } }

    /***********************************************************************************************
     * CreatePageLink / 2014-07-24 / Wethospu                                                      * 
     *                                                                                             * 
     * Creates a page link from link type and data.                                                *
     *                                                                                             *
     * Returns processed string.                                                                   *
     * str: String to edit.                                                                        *
     *                                                                                             * 
     ***********************************************************************************************/

    private static string CreatePageLink(string linkType, string linkData)
    {
      linkType = linkType.ToLower();
      if (linkType.Equals(Constants.LinkEnemy))
      {
        ErrorHandler.ShowWarning("Critical program error.");
        return "";
      }

      // Get shown text for the link.
      var shownText = CreateLinkText(linkData, linkType.Equals(Constants.LinkWiki), false);
      // Get link base URL.
      var url = linkData.Split('|')[0];
      // Build link to a proper one based on link type.
      if (linkType.Equals(Constants.LinkMedia))
        url = Images + url;
      else if (linkType.Equals(Constants.LinkYoutube))
      {
        // Check what url contains. Three cases:
        // 1: Full link (http and youtube tag) -> nothing has to be added.
        // 2: Partial link (only youtube tag) -> add HTTP.
        // 3: Minimal link (no youtube or http) -> add both.
        if (!url.Contains("youtu"))
          url = Http + Youtube + url;
        else if (!url.StartsWith(Http) && !url.StartsWith(Https))
          url = Http + url;
      }
      else if (linkType.Equals(Constants.LinkWiki))
      {
        // Check what url contains. Three cases:
        // 1: Full link (http and wiki tag) -> nothing has to be added.
        // 2: Partial link (only wiki tag) -> add HTTP.
        // 3: Minimal link (no wiki or http) -> add both.
        if (!url.Contains(Constants.Gw2Wiki))
          url = Http + Constants.Gw2Wiki + url;
        else if (!url.StartsWith(Http) && !url.StartsWith(Https))
          url = Http + url;
      }
      else if (linkType.Equals(Constants.LinkLink))
      {
        if (!url.StartsWith(Http) && !url.StartsWith(Https))
          url = Http + url;
      }
      else if (linkType.Equals(Constants.LinkRecord))
      {
        return RecordStart + url + RecordEnd;
      }
      // Url properly created. Test that it exists.
      var fullUrl = url;
      if (linkType.Equals(Constants.LinkLocal) || linkType.Equals(Constants.LinkMedia))
        fullUrl = "http://gw2dungeons.net/" + url;
      VerifyLink(fullUrl);
      BackupAndUpdateSize(fullUrl);
      
      // Generate the actual link.
      var link = new StringBuilder();
      // Mark all non-local links as overlay links to make them appear on the overlay.
      link.Append("<a");
      if (!linkType.Equals(Constants.LinkLocal))
        link.Append(" class=\"").Append(LinkClass).Append("\"");
      url = url.Replace("'", "%27");
      link.Append(" href='").Append(url).Append("'>").Append(shownText.Replace('_', ' ')).Append("</a></span>");
      return link.ToString();
    }



    /***********************************************************************************************
     * BuildEnemyLinkText / 2014-07-24 / Wethospu                                                  * 
     *                                                                                             * 
     * Checks enemies from link data for optional texts. Three cases:                              *
     * 1: No optional text found -> use name of the first enemy.                                   *
     * 2: One optional text found -> use it.                                                       *
     * 3: Multiple optional texts found -> give error.                                             *
     *                                                                                             *
     * Returns shown text.                                                                         *
     * data: Link data.                                                                            *
     * checkErrors: Whether to check and print error messages.                                     *
     *                                                                                             * 
     ***********************************************************************************************/

    private static string CreateEnemyLinkText(string data, bool checkErrors = false)
    {
      var shownText = "";
      var mainSplit = data.Split('|');
      // Check if using optional shown text.
      foreach (var enemy in mainSplit)
      {
        var subSplit = enemy.Split(':');
        if (subSplit[0].ToLower().Equals("text") && subSplit.Length > 1)
        {
          if (checkErrors && !shownText.Equals(""))
            ErrorHandler.ShowWarning("Multiple \"text\" tags on enemy link data. Remove extra ones!");
          shownText = subSplit[1];
        }
      }
      // If no text, use text of first enemy.
      if (shownText.Equals(""))
        shownText = mainSplit[0].Split(':')[0];
      return Helper.ToUpperAll(shownText.Replace('_', ' '));
    }

    /***********************************************************************************************
     * CreateLinkText / 2014-07-24 / Wethospu                                                      * 
     *                                                                                             * 
     * Creates shown link text from link data. Three possible cases:                               *
     * 1: Same text as on the link.                                                                *
     * 2: Optional text specified -> use it instead.                                               *
     * 3: Optional text starts with '(' -> append it to link text.                                 *
     *                                                                                             *
     * Returns shown text.                                                                         *
     * data: Link data.                                                                            *
     * upperCase: Whether to upper case start of each word.                                        *
     * checkErrors: Whether to check and print error messages.                                     *
     *                                                                                             * 
     ***********************************************************************************************/

    private static string CreateLinkText(string data, bool upperCase, bool checkErrors)
    {
      var subSplit = data.Split('|');
      var linkText = subSplit[0];
      if (checkErrors && linkText.Length == 0)
      {
        ErrorHandler.ShowWarning("Link is empty. Please fix!");
        return "";
      }
      if (subSplit.Length > 1)
      {
        if (checkErrors && subSplit[1].Length == 0)
          ErrorHandler.ShowWarning("Extra link text is empty. Please fix!");

        if (subSplit[1][0] == '(')
          linkText += "_" + subSplit[1];
        else
          linkText = subSplit[1];
      }
      if (upperCase)
        linkText = Helper.ToUpperAll(linkText);
      return Helper.ToUpperAll(linkText.Replace("_", Constants.Space));
    }

    /***********************************************************************************************
     * VerifyLink / 2014-07-24 / Wethospu                                                          * 
     *                                                                                             * 
     * Checks that link exists and gives an error message if needed.                               *
     *                                                                                             *
     * Returns nothing. Only checks.                                                               *
     * url: Url to verify.                                                                         *
     *                                                                                             * 
     ***********************************************************************************************/

    public static void VerifyLink(string url)
    {
      if (!Constants.ValidateUrls)
        return;
      // Check whether the url has already been verified.
      if (Constants.ValidatedUrls.Contains(url))
        return;

      var valid = Helper.IsValidUrl(url);
      if (valid)
      {
        Constants.ValidatedUrls.Add(url);
        return;
      }
      ErrorHandler.ShowWarningMessage("Link \"" + url + "\" can't be reached.");
    }

    /***********************************************************************************************
    * BackupAndUpdateSize / 2015-05-24 / Wethospu                                                 * 
    *                                                                                             * 
    * Downloads a backup of given media. Also updates media size file.                            *
    *                                                                                             *
    * url: Url to download.                                                                       *
    *                                                                                             * 
    ***********************************************************************************************/

    public static void BackupAndUpdateSize(string url)
    {
      if (!Constants.DownloadData)
        return;
      // Use the last part of url as the file name.
      var split = url.Split('/');
      var fileName = split[split.Length - 1];
      fileName = fileName.Replace("\\", "").Replace("\"", "");
      fileName = Constants.BackupLocation + fileName;
      // Check whether to download the file.
      if (Path.GetExtension(fileName).Length == 0)
        return;
      // Check whether the file has already been downloaded and checked.
      if (downloadData.Contains(url))
        return;

      downloadData.Add(url);
      // Create directory if needed.
      var dirName = Path.GetDirectoryName(fileName);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      // Download the file if it doesn't exist.
      if (!File.Exists(fileName))
      {
        using (var client = new WebClient())
        {
          var row = Console.CursorTop;
          try
          {

            Console.Write("Downloading " + url);
            client.DownloadFile(url, fileName);
            Helper.ClearConsoleLine(row);
          }
          catch (WebException)
          {
            Helper.ClearConsoleLine(row);
            ErrorHandler.ShowWarningMessage("File \"" + url + "\" can't be downloaded.");
          }
        }
      }
      // If file exists, update its size data.
      if (!File.Exists(fileName))
        return;
      int width = 0;
      int height = 0;
      if (Path.GetExtension(fileName).Equals(".jpg") || Path.GetExtension(fileName).Equals(".png") || Path.GetExtension(fileName).Equals(".bmp"))
      {
        // Built-in system works for standard images.
        Image image = Image.FromFile(fileName);
        width = image.Width;
        height = image.Height;
        GenerateThumbs(fileName, Constants.ThumbWidth, Constants.ThumbHeight);
        GenerateThumbs(fileName, Constants.ThumbWidthSmall, Constants.ThumbHeightSmall);
      }
      if (Path.GetExtension(fileName).Equals(".gif"))
      {
        // Gif has to be checked manually.
        byte[] bytes = new byte[10];
        using (FileStream fs = File.OpenRead(fileName))
        {
          fs.Read(bytes, 0, 10); // type (3 bytes), version (3 bytes), width (2 bytes), height (2 bytes)
        }
        width = bytes[6] | bytes[7] << 8; // byte 6 and 7 contain the width but in network byte order so byte 7 has to be left-shifted 8 places and bit-masked to byte 6
        height = bytes[8] | bytes[9] << 8; // same for height
        GenerateThumbs(fileName, Constants.ThumbWidth, Constants.ThumbHeight);
        GenerateThumbs(fileName, Constants.ThumbWidthSmall, Constants.ThumbHeightSmall);
      }
      if (Path.GetExtension(fileName).Equals(".webm"))
      {
        var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
        var jpgFileName = Directory.GetCurrentDirectory() + "\\" + Path.GetDirectoryName(fileName) + "\\" +  Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        ffMpeg.GetVideoThumbnail(Directory.GetCurrentDirectory() + "\\" + fileName, jpgFileName);
        Image image = Image.FromFile(jpgFileName);
        width = image.Width;
        height = image.Height;
        GenerateThumbs(jpgFileName, Constants.ThumbWidth, Constants.ThumbHeight);
        GenerateThumbs(jpgFileName, Constants.ThumbWidthSmall, Constants.ThumbHeightSmall);
      }
      if (Constants.MediaSizes.ContainsKey(url))
        Constants.MediaSizes[url] = new int[] { width, height };
      else
        Constants.MediaSizes.Add(url, new int[] { width, height });
    }

    /***********************************************************************************************
    * GenerateThumbs / 2015-07-15 / Wethospu                                                      * 
    *                                                                                             * 
    * Generates a smaller thumb image for the given file.                                         *
    *                                                                                             *
    * filename: File to convert.                                                                  *
    *                                                                                             * 
    ***********************************************************************************************/

    public static void GenerateThumbs(string filename, int maxWidth, int maxHeight)
    {
      var OutputFile = Constants.DataOutput + Constants.DataThumbsResult + "_" + maxWidth + "px\\" + Path.GetFileNameWithoutExtension(filename) + ".jpg";
      // Replace common html special characters. / 2015-07-21 / Wethospu
      OutputFile = OutputFile.Replace("%20", " ");
      OutputFile = OutputFile.Replace("%22", "\"");
      OutputFile = OutputFile.Replace("%27", "'");
      OutputFile = OutputFile.Replace("%28", "(");
      OutputFile = OutputFile.Replace("%29", ")");
      // Check if a thumb already exists. / 2015-07-15 / Wethospu
      if (File.Exists(OutputFile))
        return;
      // Create directory if needed.
      var dirName = Path.GetDirectoryName(OutputFile);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      var row = Console.CursorTop;
      Console.Write("Generating a thumb for " + filename);

      Image image;
      try
      {
        image = Image.FromFile(filename);
      }
      catch (OutOfMemoryException)
      {
        ErrorHandler.ShowWarning("File " + filename + " might be corrupted. Remove it and run this again.");
        return;
      }
      ScaleImageSize(ref maxWidth, ref maxHeight, image.Width, image.Height);
      var destRect = new Rectangle(0, 0, maxWidth, maxHeight);
      var destImage = new Bitmap(maxWidth, maxHeight);

      destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

      using (var graphics = Graphics.FromImage(destImage))
      {
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using (var wrapMode = new ImageAttributes())
        {
          wrapMode.SetWrapMode(WrapMode.TileFlipXY);
          graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        }
      }
      // Some copypaste encoder stuff for 90% quality. / 2015-07-15 / Wethospu
      ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
      System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
      EncoderParameters myEncoderParameters = new EncoderParameters(1);
      EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 90L);
      myEncoderParameters.Param[0] = myEncoderParameter;
      destImage.Save(OutputFile, jpgEncoder, myEncoderParameters);

      Helper.ClearConsoleLine(row);
    }


    /***********************************************************************************************
    * ImageCodecInfo / 2015-07-15 / Wethospu                                                      * 
    *                                                                                             * 
    * Helper function to get encoder.                                                             *
    *                                                                                             * 
    ***********************************************************************************************/
    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
      ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
      foreach (ImageCodecInfo codec in codecs)
      {
        if (codec.FormatID == format.Guid)
          return codec;
      }
      return null;
    }

   /***********************************************************************************************
   * ScaleImageSize / 2015-07-15 / Wethospu                                                      * 
   *                                                                                             * 
   * Scales width and height so that they keep the image ratio same.                             *
   *                                                                                             * 
   ***********************************************************************************************/
    private static void ScaleImageSize(ref int width, ref int height, int originalWidth, int originalHeight)
    {
      double widthRatio = (double)width / originalWidth;
      double heightRatio = (double)height / originalHeight;
      if (widthRatio < heightRatio)
        height = (int)(originalHeight * widthRatio);
      else
        width = (int)(originalWidth * heightRatio);
    }
  }
}
