using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataCreator.Utility
{

  /***********************************************************************************************
   * Constants / 2014-08-01 / Wethospu                                                           *
   *                                                                                             *
   * List of constants used in program. Also contains some rarely set values like settings.      *
   *                                                                                             *
   ***********************************************************************************************/

  public static class Constants
  {
    public static Encoding Encoding { get { return Encoding.Default; } }

    public static int UniqueIndexCounter { get; set;  }

    // Uses more complicated notation to keep flexibility for future.
    public static string DataEnemyRaw { get { return "Raw\\Enemies\\"; } }
    public static string DataEncounterRaw { get { return "Raw\\Dungeons\\"; } }
    public static string DataRaw { get { return "Raw\\"; } }
    public static string DataOtherRaw { get { return "Raw\\Other\\"; } }
    public static string DataEnemySuffix { get { return "_e"; } }

    public static string DataOutput { get { return "Output\\"; } }
    public static string DataEnemyResult { get { return "enemies\\"; } }
    public static string DataMediaResult { get { return "media\\"; } }
    public static string DataEncounterResult { get { return "pages\\"; } }
    public static string DataThumbsResult { get { return "thumbs"; } }

    public static string BackupLocation { get { return "Planning\\Backup\\"; } }


    public static string InitialdataHtml { get; set; }
    public static string InitialdataPhp { get; set; }
    public static string InitialdataText { get; set; }
    public static string InitialDataIndex { get; set; }
    public static string LineEnding { get; private set; }
    public static string ForcedLineEnding { get { return "\n"; } }
    public static string Space { get { return "&nbsp;"; } }
    public static char Delimiter { get { return '|'; } }
    public static char LinkChar { get {return '='; } }

    public static int ThumbWidth { get { return 350; } }
    public static int ThumbHeight { get { return 350; } }
    public static int ThumbWidthSmall { get { return 250; } }
    public static int ThumbHeightSmall { get { return 250; } }

    public static string Gw2Wiki { get { return "wiki.guildwars2.com/wiki/"; } }
    public static string And { get { return "and"; } }
    public static string Stack { get { return "stack"; } }
    public static string For { get { return "for"; } }
    public static string Stacks { get { return "stacks"; } }
    public static string Second { get { return "s"; } }

    // Special characters must be converted to html format so that they show properly.
    public static readonly Dictionary<string, string> CharacterConversions = new Dictionary<string, string>();
    public static readonly Dictionary<string, string> CharacterSimplifications = new Dictionary<string, string>();

    public static char TagSeparator { get { return '='; } }
    public static string Tab { get; private set; }
    public static bool ValidateUrls { get; set; }

    public static bool DownloadData { get; set; }

    public static string LinkMedia { get { return "media"; } }
    public static string LinkLocal { get { return "local"; } }
    public static string LinkYoutube { get { return "youtube"; } }
    public static string LinkLink { get { return "link"; } }
    public static string LinkWiki { get { return "wiki"; } }
    public static string LinkEnemy { get { return "enemy"; } }
    public static string LinkRecord { get { return "record"; } }
    public static readonly string[] LinkTypes = { LinkMedia, LinkLocal, LinkYoutube, LinkLink, LinkWiki, LinkEnemy, LinkRecord };
    // Image: used for internal images. Assumes that they are in "media/dungeonimages/".
    // Local: general link for stuff inside of the page.
    // Link: general link for stuff outside of the page.
    // Youtube: used for youtube. Assumes url of "youtu.be/".
    // Wiki: used for gw2wiki. Assumes url of "wiki.guildwars2.com/wiki/".
    // Record: used to read records from the record database.

    public static readonly HashSet<string> AvailableTactics = new HashSet<string>();
    public static readonly HashSet<string> AvailableTips = new HashSet<string>();
    // List of media sizes. Allows resizing the layout properly without having to download and/or check file size each time.
    public static readonly Dictionary<string, int[]> MediaSizes = new Dictionary<string, int[]>();
    // List of urls which have already been validated. Makes the validating faster when you don't have to check already validated links.
    public static readonly HashSet<string> ValidatedUrls = new HashSet<string>();
    
    // Html classes.
    public static string IconClass { get { return "\"icon\""; } }

    public static readonly SortedSet<string> AttackTypeTags = new SortedSet<string>(){ "pbaoe", "melee", "ranged", "projectile", "homing", "bouncing", "aoe", "dash", "leap",  "delayed", "field",
      "summon", "aura", "ticking", "cone", "piercing", "evade", "buff", "trap" };

    public static readonly SortedSet<string> EffectTags = new SortedSet<string>(){ "alacrity", "condition", "bleeding", "blind", "burning", "chilled", "confusion", "crippled", "fear", "immobilized",
      "poison", "slow", "torment", "vulnerability", "weakness", "boon", "aegis", "fury", "might", "protection", "regeneration", "resistance", "retaliation", "stability", "swiftness", "quickness",
      "vigor", "control", "daze", "float", "knockback", "knockdown", "launch", "pull", "sink", "stun", "taunt", "agony", "invulnerability", "revealed", "stealth", "buff",
      "damage", "fixed damage", "healing", "percent damage"  };

    public static string JSFiles { get; private set; }
    public static string CSSFiles { get; private set; }

    public static bool IsRelease { get; private set; }

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
        JSFiles = Tab + "<script src=\"./media/js/bootstrap-tagsinput.min.js\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/jquery-sortable-min.js\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/commentsection.js\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/jquery.storage.js\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-main.js\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-settings.js\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-calculator.js\"></script>" + LineEnding;
        JSFiles += Tab + "<script src=\"./media/js/gw2dungeons-sql.js\"></script>" + LineEnding;

        CSSFiles = Tab + "<link rel=\"stylesheet\" href=\"./media/css/gw2dungeons.css\">" + LineEnding;
        CSSFiles += Tab + "<link rel=\"stylesheet\" href=\"./media/css/bootstrap-tagsinput.css\">" + LineEnding;

        InitialdataHtml = "<!-- Automatically generated file from raw data. ALL YOUR CHANGES WILL BE LOST! -->" + LineEnding;
        InitialdataPhp = "// Automatically generated file from raw data. ALL YOUR CHANGES WILL BE LOST!" + LineEnding;
        InitialdataText = "# Automatically generated file from raw data. ALL YOUR CHANGES WILL BE LOST!" + LineEnding;
        InitialDataIndex = "<!-- display name|search name|rank|race|dungeon|path|index|tags -->" + LineEnding;
      }
    }

  }
}
