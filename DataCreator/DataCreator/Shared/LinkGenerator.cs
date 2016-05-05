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
  /// <summary>
  /// Contains functions to generate internal enemy links and external page links.
  /// </summary>
  class LinkGenerator
  {
    private static string _currentDungeon = "";
    /// <summary>
    /// Helper variable to keep track of current instance. Bad design so try get rid of this.
    /// </summary>
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

    /// <summary>
    /// Creates both internal enemy and external page links to a given string.
    /// </summary>
    /// <param name="str">String to process.</param>
    /// <param name="paths">Path information to find correct the enemies.</param>
    /// <param name="enemyData">Enemy information for enemy links.</param>
    /// <returns>Processed string.</returns>
    public static string CreateLinks(string str, List<string> paths, List<Enemy> enemyData)
    {
      return CreatePageLinks(CreateEnemyLinks(str, paths, enemyData));
    }

    /// <summary>
    /// Creates internal enemy links to a given string. Links use unique index numbers for a good client performance.
    /// </summary>
    /// <param name="str">String to process.</param>
    /// <param name="paths">Path information to find the correct enemies.</param>
    /// <param name="enemyData">Enemy information to make the connection.</param>
    /// <returns>Processed string.</returns>
    private static string CreateEnemyLinks(string str, List<string> paths, List<Enemy> enemyData)
    {
      // Keep track of the progress for a better performance (O(n) vs O(n^2)).
      for (var index = 0; index < str.Length; index++)
      {
        var link = FindNextLink(str, ref index);
        if (link == null || !link[0].Equals(Constants.LinkEnemy))
          continue;

        var replaceWith = CreateEnemyLink(link[1], paths, enemyData);
        var toReplace = link[0] + Constants.LinkChar + link[1];
        // Replace all doesn't work because some links are found partially in other links.
        str = Helper.ReplaceFirst(str, toReplace, replaceWith);
        index = index - toReplace.Length + replaceWith.Length;
      }
      return str;
    }

    /// <summary>
    /// Similar implementation as CreateEnemyLinks but instead returns found enemies from all links.
    /// </summary>
    public static List<Enemy> GetEnemiesFromLinks(string str, List<string> paths, List<Enemy> enemyData)
    {
      var enemies = new List<Enemy>();
      for (var index = 0; index < str.Length; index++)
      {
        var link = FindNextLink(str, ref index);
        if (link == null && !link[0].ToLower().Equals(Constants.LinkEnemy))
          continue;
        var foundEnemies = GetEnemiesFromLink(link[1], paths, enemyData);
        foreach (var enemy in foundEnemies)
          enemies.Add(enemy);
      }
      return enemies;
    }

    /// <summary>
    /// Creates external page links to a given string.
    /// </summary>
    /// <param name="str">String to process.</param>
    /// <returns>Processed string.</returns>
    private static string CreatePageLinks(string str)
    {
      for (var index = 0; index < str.Length; index++)
      {
        var link = FindNextLink(str, ref index);
        if (link == null || link[0].Equals(Constants.LinkEnemy))
          continue;

        // Check that link type is an actual link (not for example a html tag).
        if (!Constants.LinkTypes.Contains(link[0]))
          continue;
        var toReplace = link[0] + Constants.LinkChar + link[1];
        var replaceWith = CreatePageLink(link[0], link[1]);
        // Replace all doesn't work because some links are found partially in other links.
        str = Helper.ReplaceFirst(str, toReplace, replaceWith);
        index = index - toReplace.Length + replaceWith.Length;
      }
      return str;
    }

    /// <summary>
    /// Removes all links from a given string. Used to simplify encounter names.
    /// </summary>
    /// <param name="str">String to process.</param>
    /// <returns>Processed string.</returns>
    public static string RemoveLinks(string str)
    {
      // Iterator over the string and look for links.
      for (var index = 0; index < str.Length; index++)
      {
        var link = FindNextLink(str, ref index);
        if (link == null)
          continue;

        string shownText;
        if (link[0].Equals(Constants.LinkEnemy))
          shownText = CreateEnemyLinkShownText(link[1]);
        else if (link[0].Equals(Constants.LinkWiki))
          shownText = CreateLinkShownText(link[1], true, false);
        else
          shownText = CreateLinkShownText(link[1], false, false);
        var toRemove = link[0] + Constants.LinkChar + link[1];
        str = str.Replace(toRemove, shownText);
        index = index - toRemove.Length + shownText.Length;
      }
      return str;
    }

    /// <summary>
    /// Checks syntax of links and removes problematic links.
    /// </summary>
    public static string CheckLinkSyntax(string str)
    {
      for (var index = 0; index < str.Length; index++)
      {
        var link = FindNextLink(str, ref index);
        if (link == null)
          continue;
        var shownText = "";
        if (!Constants.LinkTypes.Contains(link[0]))
          ErrorHandler.ShowWarning("Link type " + link[0] + " not recognized. Please fix!");
        else
        {
          if (link[0].Equals(Constants.LinkEnemy))
            shownText = CreateEnemyLinkShownText(link[1], true);
          else if (link[0].Equals(Constants.LinkWiki))
            shownText = CreateLinkShownText(link[1], true, true);
          else
            shownText = CreateLinkShownText(link[1], false, true);
        }
        // No shown text means an error so remove the broken link.
        if (shownText.Equals(""))
          str = str.Replace(link[0] + Constants.LinkChar + link[1], shownText);
      }
      return str;
    }

    /// <summary>
    /// Creates an enemy link from a given link value.
    /// </summary>
    private static string CreateEnemyLink(string linkData, List<string> paths, List<Enemy> enemies)
    {
      var shownText = CreateEnemyLinkShownText(linkData);

      var enemiesToLink = new List<Enemy>();
      var enemyLevels = new List<int>();
      var enemySplit = linkData.Split('|');
      foreach (var enemyStr in enemySplit)
      {
        var subSplit = enemyStr.Split(':');
        var name = subSplit[0].ToLower();
        var rank = "";
        var level = 0;
        // Ignore shown text customization because it doesn't contain an enemy.
        if (name.Equals("text"))
          continue;
        // Extract enemy information so it can be searched.
        if (subSplit.Length > 1)
          rank = subSplit[1].ToLower();
        if (subSplit.Length > 2)
          level = Helper.ParseI(subSplit[2].ToLower());
        enemyLevels.Add(level);
        var foundEnemies = Gw2Helper.FindEnemies(enemies, name, rank, paths);
        if (foundEnemies.Count == 0)
        {
          ErrorHandler.ShowWarningMessage("No enemy found for enemy " + name + " with link " + linkData + " and path " + string.Join("|", paths) + ". Change parameters, add missing enemy or check syntax file.");
          continue;
        }
        if (foundEnemies.Count > 1)
          ErrorHandler.ShowWarningMessage("Multiple enemies found for enemy " + name + " with link " + linkData + " and path " + string.Join("|", paths) + ". Add more parameters or check syntax file.");
        // Note: This could be used to link multiple enemies (for example every elite enemy).
        // However it would be extremely inefficient because each enemy would be searched individually.
        enemiesToLink.Add(foundEnemies[0]);
      }
      if (enemiesToLink.Count == 0)
        return shownText;
      // Linkable enemies found. Start building the link.
      // Syntax:
      // <span class="'main rank' enemy-button" data-name="'enemy names'" data-rank="'enemy categories" data-level="'enemy levels'" data-path="'path'" data-level"'level'">'shown text'</span>
      var link = new StringBuilder();
      link.Append("<span class=\"").Append(enemiesToLink[0].Attributes.Rank).Append(" enemy-button\" data-index=\"");
      for (var index = 0; index < enemiesToLink.Count; index++)
      {
        link.Append(enemiesToLink[index].Index);
        if (index + 1 < enemiesToLink.Count)
          link.Append(':');
      }
      // Add extra information to pre-select enemy customization options in the website.
      link.Append("\" data-path=\"").Append(string.Join("|", paths)).Append("\"");
      link.Append(" data-level=\"");
      for (var index = 0; index < enemyLevels.Count; index++)
      {
        link.Append(enemyLevels[index]);
        if (index + 1 < enemiesToLink.Count)
          link.Append(':');
      }
      link.Append("\"> ");
      // Add shown text.
      link.Append(shownText).Append("</span>");
      return link.ToString();
    }
    // Works same as above.
    /// <summary>
    /// Returns enemies found from a given link. This doesn't generate anything.
    /// </summary>
    private static List<Enemy> GetEnemiesFromLink(string linkData, List<string> paths, List<Enemy> enemies)
    {
      var enemiesToLink = new List<Enemy>();
      var enemySplit = linkData.Split('|');
      foreach (var enemyStr in enemySplit)
      {
        var subSplit = enemyStr.Split(':');
        var name = subSplit[0].ToLower();
        var rank = "";
        if (name.Equals("text"))
          continue;
        if (subSplit.Length > 1)
          rank = subSplit[1].ToLower();
        var foundEnemies = Gw2Helper.FindEnemies(enemies, name, rank, paths);
        if (foundEnemies.Count == 0)
        {
          ErrorHandler.ShowWarningMessage("No enemy found for enemy " + name + " with link " + linkData + " and path " + string.Join("|", paths) + ". Change parameters, add missing enemy or check syntax file.");
          continue;
        }
        if (foundEnemies.Count > 1)
          ErrorHandler.ShowWarningMessage("Multiple enemies found for enemy " + name + " with link " + linkData + " and path " + string.Join("|", paths) + ". Add more parameters or check syntax file.");
        enemiesToLink.Add(foundEnemies[0]);
      }
      return enemiesToLink;
    }

    /// <summary>
    /// Returns next link (type and value) from a given string.
    /// </summary>
    /// <param name="str">String to check.</param>
    /// <param name="startIndex">Input: Starting index. Output: End of found link type.</param>
    /// <returns>Array with link type and link value. Null if nothing was found.</returns>
    private static string[] FindNextLink(string str, ref int startIndex)
    {
      var linkIndex = str.IndexOf(Constants.LinkChar, startIndex);
      if (linkIndex < 0 || linkIndex >= str.Length)
      {
        startIndex = str.Length;
        return null;
      }
      // Parse link type (before separator) and link value (after separator) from the string.
      var typeStart = 1 + Helper.LastIndexOf(str, new[] { ' ', '(', '[', '>' }, linkIndex);
      var type = str.Substring(typeStart, linkIndex - typeStart).ToLower();
      // Value is hard to get because some characters ('.', ',', ':', ')') can end the link but also be part of it!
      // Character ' ' is reliable because '_' is used in values.
      var valueEnd = str.IndexOf(' ', linkIndex + 1);
      if (valueEnd < 0)
        valueEnd = str.Length;
      if (str[valueEnd - 1] == '.' || str[valueEnd - 1] == ',' || str[valueEnd - 1] == ':' || str[valueEnd - 1] == ')')
        valueEnd--;
      var value = str.Substring(linkIndex + 1, valueEnd - linkIndex - 1);
      startIndex = valueEnd;
      return new[] { type, value };
    }


    private static string Http { get { return "http://"; } }
    private static string Https { get { return "https://"; } }
    private static string Youtube { get { return "youtu.be/"; } }
    private static string LinkClass { get { return "overlay-link"; } }
    private static string RecordStart { get { return "<span class=\"record-run\">";  } }
    private static string RecordEnd { get { return "</span>"; } }

    /// <summary>
    /// Generates a page link from a given data.
    /// </summary>
    /// <param name="linkType">Type of the link. Used to make the url have a correct format.</param>
    private static string CreatePageLink(string linkType, string linkData)
    {
      linkType = linkType.ToLower();
      if (linkType.Equals(Constants.LinkEnemy))
      {
        ErrorHandler.ShowWarning("Critical program error.");
        return "";
      }

      var shownText = CreateLinkShownText(linkData, linkType.Equals(Constants.LinkWiki), false);
      var url = linkData.Split('|')[0];
      // Verify and build the correct link format based on the link type.
      if (linkType.Equals(Constants.LinkMedia))
        url = Constants.WebsiteMediaLocation + CurrentDungeon + "/" + url;
      else if (linkType.Equals(Constants.LinkYoutube))
      {
        if (!url.Contains("youtu"))
          url = Http + Youtube + url;
        else if (!url.StartsWith(Http) && !url.StartsWith(Https))
          url = Http + url;
      }
      else if (linkType.Equals(Constants.LinkWiki))
      {
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
      else if (linkType.Equals(Constants.LinkLocal))
        url = "http://gw2dungeons.net/" + url;
      // Url properly created. Test that it exists.
      var fullUrl = url;
      if (linkType.Equals(Constants.LinkYoutube))
        fullUrl = "http://www.youtube.com/embed/" + linkData.Split('|')[0];
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

    /// <summary>
    /// Generates shown text for an enemy link from a given data.
    /// </summary>
    /// <param name="data">Link data.</param>
    /// <param name="checkErrors">Whether to print warnings. Used to suppress additional warnings.</param>
    /// <returns>Generated shown text.</returns>
    private static string CreateEnemyLinkShownText(string data, bool checkErrors = false)
    {
      var shownText = "";
      var mainSplit = data.Split('|');
      // Shown text can be customized so check it first.
      foreach (var enemy in mainSplit)
      {
        var subSplit = enemy.Split(':');
        if (subSplit[0].ToLower().Equals("text") && subSplit.Length > 1)
        {
          // Multiple customizations just override each other.
          if (checkErrors && !shownText.Equals(""))
            ErrorHandler.ShowWarning("Multiple \"text\" tags on enemy link data. Remove extra ones!");
          shownText = subSplit[1];
        }
      }
      // By default, use the name of the first enemy.
      if (shownText.Equals(""))
        shownText = mainSplit[0].Split(':')[0];
      return Helper.ToUpperAll(shownText.Replace('_', ' '));
    }

    /// <summary>
    /// Generates the shown text for page links from a given data.
    /// </summary>
    /// <param name="data">Link data.</param>
    /// <param name="upperCase">Whether to upper case the start of each word.</param>
    /// <param name="checkErrors">>Whether to print warnings. Used to suppress additional warnings.</param>
    /// <returns>Generated shown text.</returns>
    private static string CreateLinkShownText(string data, bool upperCase, bool checkErrors)
    {
      var subSplit = data.Split('|');
      var linkText = subSplit[0];
      if (checkErrors && linkText.Length == 0)
      {
        ErrorHandler.ShowWarning("Link is empty. Please fix!");
        return "";
      }
      // Check for a customized text.
      if (subSplit.Length > 1)
      {
        if (checkErrors && subSplit[1].Length == 0)
          ErrorHandler.ShowWarning("Extra link text is empty. Please fix!");
        // "(" is a special case which appends the customization to the link name. Not sure if in use anymore.
        if (subSplit[1][0] == '(')
          linkText += "_" + subSplit[1];
        else
          linkText = subSplit[1];
      }
      if (upperCase)
        linkText = Helper.ToUpperAll(linkText);
      return Helper.ToUpperAll(linkText.Replace("_", Constants.Space));
    }

    /// <summary>
    /// Checks that a given page link exists. Shows a warning if needed.
    /// </summary>
    /// <param name="url"></param>
    public static void VerifyLink(string url)
    {
      if (!Constants.ValidateUrls)
        return;
      // Use a cache because this a very slow operation.
      if (Constants.ValidatedUrls.Contains(url))
        return;
      var valid = Helper.IsValidUrl(url);
      if (valid)
        Constants.ValidatedUrls.Add(url);
      else
        ErrorHandler.ShowWarningMessage("Link \"" + url + "\" can't be reached.");
    }
    /// <summary>
    /// Remember checked files to avoid doing slow operations.
    /// </summary>
    private static HashSet<string> downloadedFileNames = new HashSet<string>();
    /// <summary>
    /// Downloads a back of a given media link. Also updates its size information so it can be loaded properly in the website.
    /// </summary>
    /// <param name="url"></param>
    public static void BackupAndUpdateSize(string url)
    {
      if (!Constants.DownloadData)
        return;
      // Use the last part of url to get a cleaner file name. May cause issues in the future but unlikely.
      var split = url.Split('/');
      var fileName = split[split.Length - 1];
      // Remove illegal characters to avoid problems with filesystems.
      fileName = fileName.Replace("\\", "").Replace("\"", "");
      if (Path.GetExtension(fileName).Equals(".gif"))
      {
        // Separate gifs based on source to get a nicer structure (no real advantage).
        if (url.Contains("gfycat"))
          fileName = Constants.BackupLocation + "gfycat\\" + fileName;
        else if(url.Contains("imgur"))
          fileName = Constants.BackupLocation + "imgur\\" + fileName;
        else
          fileName = Constants.BackupLocation + fileName;
      }
      else
        fileName = Constants.DataOtherRaw + Constants.LocalMediaFolder + CurrentDungeon + "\\" + fileName;
      // Function gets called for all kinds of links. Obviously all media files have a file extension.
      if (Path.GetExtension(fileName).Length == 0)
        return;
      if (downloadedFileNames.Contains(url))
        return;

      downloadedFileNames.Add(url);
      var dirName = Path.GetDirectoryName(fileName);
      if (dirName != null)
        Directory.CreateDirectory(dirName);
      // Don't download already existing files to make this much faster. This means that the backup must be deleted manually if content changes.
      if (!File.Exists(fileName))
      {
        // Use a custom timeout to make this work much faster.
        var timeOut = 10000;
        if (Path.GetExtension(fileName).Equals(".gif"))
          timeOut = 30000;
        using (var client = new WebDownload(timeOut))
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
      if (!File.Exists(fileName))
        return;
      UpdateSizeInformation(fileName, url);
      
    }

    /// <summary>
    /// Updates the size information for a given url from a given file. Needed to show media properly in the website.
    /// </summary>
    /// <param name="fileName">File which is used to get the size information.</param>
    private static void UpdateSizeInformation(string fileName, string url)
    {
      int width = 0;
      int height = 0;
      if (Path.GetExtension(fileName).Equals(".jpg") || Path.GetExtension(fileName).Equals(".png") || Path.GetExtension(fileName).Equals(".bmp"))
      {
        // Built-in system works for standard images.
        Image image = Image.FromFile(fileName);
        width = image.Width;
        height = image.Height;
        GenerateThumbs(fileName, url, Constants.ThumbWidth, Constants.ThumbHeight);
        GenerateThumbs(fileName, url, Constants.ThumbWidthSmall, Constants.ThumbHeightSmall);
      }
      if (Path.GetExtension(fileName).Equals(".gif"))
      {
        // Gif has to be checked manually.
        byte[] bytes = new byte[10];
        using (FileStream fs = File.OpenRead(fileName))
        {
          // Type (3 bytes), version (3 bytes), width (2 bytes), height (2 bytes).
          fs.Read(bytes, 0, 10);
        }
        // Byte 6 and 7 contain the width but in network byte order so byte 7 has to be left-shifted 8 places and bit-masked to byte 6.
        width = bytes[6] | bytes[7] << 8;
        height = bytes[8] | bytes[9] << 8;
        GenerateThumbs(fileName, url, Constants.ThumbWidth, Constants.ThumbHeight);
        GenerateThumbs(fileName, url, Constants.ThumbWidthSmall, Constants.ThumbHeightSmall);
      }
      if (Path.GetExtension(fileName).Equals(".webm"))
      {
        var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
        var jpgFileName = Directory.GetCurrentDirectory() + "\\" + Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        ffMpeg.GetVideoThumbnail(Directory.GetCurrentDirectory() + "\\" + fileName, jpgFileName);
        Image image = Image.FromFile(jpgFileName);
        width = image.Width;
        height = image.Height;
        GenerateThumbs(jpgFileName, url, Constants.ThumbWidth, Constants.ThumbHeight);
        GenerateThumbs(jpgFileName, url, Constants.ThumbWidthSmall, Constants.ThumbHeightSmall);
      }
      if (Constants.MediaSizes.ContainsKey(url))
        Constants.MediaSizes[url] = new int[] { width, height };
      else
        Constants.MediaSizes.Add(url, new int[] { width, height });
    }

    /// <summary>
    /// Creates a jpg-thumbnail for a given file. Needed for preview images in the website.
    /// </summary>
    /// <param name="filename">Filename of the local base file.</param>
    /// <param name="baseUrl">URL of the source location to create subfolders.</param>
    public static void GenerateThumbs(string filename, string baseUrl, int maxWidth, int maxHeight)
    {
      var OutputFile = Constants.DataOutput + Constants.DataThumbsResult + "_" + maxWidth + "px\\";
      var folder = "";
      // Split thumbnails based on source to get a nicer structure and to reduce files per folder.
      if (baseUrl.Contains("wiki"))
        folder = "wiki\\";
      else if (baseUrl.Contains("gfycat"))
        folder = "gfycat\\";
      OutputFile += folder + Path.GetFileNameWithoutExtension(filename) + ".jpg";
      // Replace common html special characters to get cleaner file names.
      OutputFile = OutputFile.Replace("%20", " ");
      OutputFile = OutputFile.Replace("%27", "'");
      OutputFile = OutputFile.Replace("%28", "(");
      OutputFile = OutputFile.Replace("%29", ")");
      if (File.Exists(OutputFile))
        return;
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
      // Some copypaste encoder stuff for a 90% quality.
      ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
      System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
      EncoderParameters myEncoderParameters = new EncoderParameters(1);
      EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 90L);
      myEncoderParameters.Param[0] = myEncoderParameter;
      destImage.Save(OutputFile, jpgEncoder, myEncoderParameters);

      Helper.ClearConsoleLine(row);
    }

    /// <summary>
    /// Helper function to get a proper encoder for thumbnail generation.
    /// </summary>
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

    /// <summary>
    /// Helper function to scale width and height so that the aspect ratio is maintained.
    /// </summary>
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
