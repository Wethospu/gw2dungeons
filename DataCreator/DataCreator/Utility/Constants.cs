using DataCreator.Encounters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataCreator.Utility
{
  /// <summary>
  /// Contains constants, global values and rarely changed values like settings.
  /// </summary>
  // Constants could be stored in a text file but currently there is no real need for that.
  // Globals should be refactored but too lazy for that.
  public static class Constants
  {
    /// <summary>
    /// Encoding needs to be fixed to ensure numbers get right formatting and so on.
    /// </summary>
    // This doesn't really do anything at the moment because the encoding is set globally during the startup.
    // TODO: Remove this.
    public static Encoding Encoding { get { return Encoding.Default; } }

    /// <summary>
    /// Needed to give every encounter an unique index so their tactics don't get mixed up.
    /// </summary>
    // TODO: Refactor this.
    public static int UniqueIndexCounter { get; set;  }

    // Uses more complicated notation to keep flexibility for future.
    // TODO: Clean this up. Let's stick to the fact that all input is in Raw folder.
    public static string DataEnemyRaw { get { return "Raw\\Enemies\\"; } }
    public static string DataGuidesRaw { get { return "Raw\\Guides\\"; } }
    public static string DataDungeonsRaw { get { return "Raw\\Dungeons\\"; } }
    public static string DataRaidsRaw { get { return "Raw\\Raids\\"; } }
    public static string DataFractalsRaw { get { return "Raw\\Fractals\\"; } }
    public static string DataRaw { get { return "Raw\\"; } }
    public static string DataOtherRaw { get { return "Raw\\Other\\"; } }

    public static string DataOutput { get { return "Output\\"; } }
    public static string DataEnemyResult { get { return "enemies\\"; } }
    public static string DataMediaResult { get { return "media\\"; } }
    public static string DataEncounterResult { get { return "pages\\"; } }
    public static string DataThumbsResult { get { return "thumbs"; } }
    public static string EnemyMediaFolder { get { return "Enemies"; } }

    public static string BackupLocation { get { return "Planning\\Backup\\"; } }

    public static string LocalMediaFolder { get { return "media\\images\\"; } }
    public static string LocalIconFolder { get { return "media\\icons\\"; } }
    public static string WebsiteMediaLocation { get { return "/media/images/"; } }
    public static string WebsiteThumbBigLocation { get { return "/media/thumbs_" + ThumbWidth + "px/"; } }
    public static string WebsiteThumbSmallLocation { get { return "/media/thumbs_" + ThumbWidthSmall + "px/"; } }
    public static string WebsiteIconLocation { get { return "/media/icons/"; } }
    public static string WebsiteURL { get { return "http://gw2dungeons.net/"; } }

    // This site is used to verify internet connection.
    // Google was chosen because of reliability.
    public static string URLToVerifyInternet { get { return "http://google.com"; } }

    // Initial data for all generated files.
    // Mainly used to warn people that the file is generated so they don't make changes to it.
    // Not used in release mode for slightly smaller file sizes.
    public static string InitialdataHtml { get; set; }
    public static string InitialdataPhp { get; set; }
    public static string InitialdataText { get; set; }
    public static string InitialDataIndex { get; set; }
    // Variable characters. Used to reduce file sizes during release mode.
    public static string LineEnding { get; private set; }
    public static string Tab { get; private set; }
    // Special characters to ensure everyone uses the same ones.
    public static string ForcedLineEnding { get { return "\n"; } }
    public static string Space { get { return "&nbsp;"; } }
    public static char Delimiter { get { return '|'; } }
    public static char LinkChar { get {return '='; } }
    public static char TagSeparator { get { return '='; } }

    public static int ThumbWidth { get { return 350; } }
    public static int ThumbHeight { get { return 350; } }
    public static int ThumbWidthSmall { get { return 250; } }
    public static int ThumbHeightSmall { get { return 250; } }

    public static int FractalNavPathCount { get { return 6; } }

    // Language specific constants. These were needed when there were multiple translations.
    // Currently used to normalize the language and to keep some support for translations.
    public static string Gw2Wiki { get { return "wiki.guildwars2.com/wiki/"; } }
    public static string And { get { return "and"; } }
    public static string Stack { get { return "stack"; } }
    public static string For { get { return "for"; } }
    public static string Stacks { get { return "stacks"; } }
    public static string Second { get { return "s"; } }

    /// <summary>
    /// Special characters must be converted to html format so that they show properly.
    /// </summary>
    public static readonly Dictionary<string, string> CharacterConversions = new Dictionary<string, string>();
    /// <summary>
    /// Special characters must be converted to simpler versions so they can be used in stuff like html ids.
    /// </summary>
    public static readonly Dictionary<string, string> CharacterSimplifications = new Dictionary<string, string>();

    // Run-time settings.
    public static bool ValidateUrls { get; set; }
    public static bool DownloadData { get; set; }

    /* Tags for different link types.
         Media: used for internal media. Assumes that they are in "media/dungeonimages/".
         Local: general link for stuff inside of the page.
         Link: general link for stuff outside of the page.
         Youtube: used for youtube. Assumes url of "youtu.be/".
         Wiki: used for gw2wiki. Assumes url of "wiki.guildwars2.com/wiki/".
         Record: used to read records from the record database.
    */
    public static string LinkMedia { get { return "media"; } }
    public static string LinkLocal { get { return "local"; } }
    public static string LinkYoutube { get { return "youtube"; } }
    public static string LinkLink { get { return "link"; } }
    public static string LinkWiki { get { return "wiki"; } }
    public static string LinkEnemy { get { return "enemy"; } }
    public static string LinkRecord { get { return "record"; } }
    public static readonly SortedSet<string> LinkTypes = new SortedSet<string>(){ LinkMedia, LinkLocal, LinkYoutube, LinkLink, LinkWiki, LinkEnemy, LinkRecord };

    /// <summary>
    /// Use a pre-defined tactics to catch typing mistakes.
    /// </summary>
    // Technically any name is possible so just add more if needed.
    public static readonly HashSet<string> AvailableTactics = new HashSet<string>();
    /// <summary>
    /// Use a pre-defined tips to catch typing mistakes.
    /// </summary>
    public static readonly HashSet<string> AvailableTips = new HashSet<string>();
    // The website uses iframes which prevent getting size information directly from the file.
    // This means the overlay can't be scaled according to the shown content.
    // Insert media sizes to the media links during the generation as a workaround.
    // This takes very long because over 1GB of data has to be downloaded to check the sizes.
    // So store the results to prevent doing this again and again.
    public static readonly Dictionary<string, int[]> MediaSizes = new Dictionary<string, int[]>();
    // Typing mistakes on links are very common. Also links may disappear at any moment.
    // Checking all these manually would be cumbersome so it's better to do it automatically.
    // However this takes a while so store the results.
    public static readonly HashSet<string> ValidatedUrls = new HashSet<string>();
    
    // Html classes.
    public static string IconClass { get { return "\"icon\""; } }
    public static string HelpIconClass { get { return "\"icon-help\""; } }

    // Tags which are recognized by this program.
    // These can be used to filter enemies in the website's search.
    public static readonly SortedSet<string> AttackTypeTags = new SortedSet<string>(){ "pbaoe", "melee", "ranged", "projectile", "homing", "bouncing", "aoe", "dash", "leap",  "delayed", "field",
      "summon", "aura", "ticking", "cone", "piercing", "evade", "buff", "trap", "defiant", "resistant" };

    public static readonly SortedSet<string> EffectTags = new SortedSet<string>(){ "alacrity", "condition", "bleeding", "blind", "burning", "chilled", "confusion", "crippled", "fear", "immobilized",
      "poison", "slow", "torment", "vulnerability", "weakness", "boon", "aegis", "fury", "defiance", "might", "protection", "regeneration", "resistance", "retaliation", "stability", "swiftness", "quickness",
      "vigor", "control", "daze", "float", "knockback", "knockdown", "launch", "pull", "displacement", "sink", "stun", "taunt", "agony", "invulnerability", "revealed", "stealth", "buff",
      "damage", "fixed damage", "healing", "percent damage"  };

    public static readonly SortedSet<string> AvailableRanks = new SortedSet<string>(){ "normal", "veteran", "elite", "champion", "legendary", "structure", "trap", "skill", "bundle" };


    // Dynamic include for js and css based on the release mode.
    public static string JSFiles { get; set; }
    public static string CSSFiles { get; set; }

    // Program can generate the pages as optimized release or more informative developer version.
    // Release removes some useless characters and also merges js/css files.
    public static bool IsRelease { get; private set; }

    public static List<SortedSet<Instability>> Instabilities = new List<SortedSet<Instability> >();
    public static string InstabilityEncounter { get { return "INSTABILITIES"; } }

    /// <summary>
    /// Set up values based on relese mode.
    /// </summary>
    public static void Initialize(bool release)
    {
      IsRelease = release;
      if (release)
      {
        Tab = "";
        LineEnding = "";
        JSFiles = "    <script src=\"./media/gw2dungeons.js\"></script>\n";
        CSSFiles = "    <link rel=\"stylesheet\" href=\"./media/gw2dungeons.css\">\n";
        InitialdataHtml = "";
        InitialdataPhp = "";
        InitialdataText = "";
        InitialDataIndex = "";
      }
      else
      {
        Tab = "    ";
        LineEnding = "\n";
        // Add a date to prevent caching on the developer environment.
        var date = DateTime.UtcNow.Ticks;
        JSFiles = Tab + "<script src=\"./media/js/bootstrap-tagsinput.min.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/jquery-sortable-min.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/commentsection.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/jquery.storage.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-main.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-settings.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-calculator.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-sql.js?" + date + "\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gfycat.min.js?" + date + "\"></script>" + LineEnding;

        CSSFiles = Tab + "<link rel=\"stylesheet\" href=\"./media/css/gw2dungeons.css?" + date + "\">" + LineEnding;
        CSSFiles += Tab + "<link rel=\"stylesheet\" href=\"./media/css/bootstrap-tagsinput.css?" + date + "\">" + LineEnding;

        InitialdataHtml = "<!-- Automatically generated file from raw data. ALL YOUR CHANGES WILL BE LOST! -->" + LineEnding;
        InitialdataPhp = "// Automatically generated file from raw data. ALL YOUR CHANGES WILL BE LOST!" + LineEnding;
        InitialdataText = "# Automatically generated file from raw data. ALL YOUR CHANGES WILL BE LOST!" + LineEnding;
        InitialDataIndex = "<!-- display name|search name|rank|race|dungeon|path|index|tags -->" + LineEnding;
      }
    }

  }
}
