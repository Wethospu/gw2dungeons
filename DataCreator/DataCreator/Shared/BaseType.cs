using DataCreator.Encounters;
using DataCreator.Enemies;
using DataCreator.Utility;
using System.Collections.Generic;
using System.Linq;

namespace DataCreator.Shared
{
  /// <summary>
  /// Shared base datatype for enemies and encounters. Used to unify their operations to share functionality.
  /// </summary>
  public class BaseType
  {
    /// <summary>
    /// Name is not guaranteed to remain as a plain text so an additional variable is needed for html data attributes.
    /// </summary>
    public string DataName;
    private string name;
    public string Name
    {
      get { return name; }
      set
      {
        if (value.Length == 0)
        {
          ErrorHandler.ShowWarning("Missing info. Use \"name='name'\"!");
          return;
        }
        //if (value.Contains('_'))
        //  ErrorHandler.ShowWarning("Enemy name " + value + "  containts '_'. Replace them with ' '!");
        name = LinkGenerator.CheckLinkSyntax(value);
        IsNameCopied = false;
      }
    }
    /// <summary>
    /// Tracks whether the name was explicitly set or copied. Usually explicitly setting the name creates a new data.
    /// This allows changing the name of copied data.
    /// </summary>
    public bool IsNameCopied = false;
    /// <summary>
    /// Unique index for this enemy. This is used for more efficient enemy linking (no searching has to be done).
    /// </summary>
    public int Index = 0;
    private List<string> paths = new List<string>();
    /// <summary>
    /// Tags of paths which include this enemy. There is no guarantee that the enemy is actually linked from these paths.
    /// </summary>
    public List<string> Paths
    {
      get { return paths; }
      set
      {
        if (value.Count == 0 || value[0].Length == 0)
          ErrorHandler.ShowWarning("Missing info. Use \"path='path1'|'path2'|'pathN'\"!");
        if (value.Any(val => val.Contains(" ")))
        {
          ErrorHandler.ShowWarning("' ' found. Use syntax \"path='path1'|'path2'|'pathN'\"");
        }
        paths = value;
      }
    }

    /// <summary>
    /// Tactics for fighting this enemy.
    /// </summary>
    public TacticList Tactics = new TacticList();
    /// <summary>
    /// Image or video files showing this encounter or enemy. Commonly only one image is used for enemies.
    /// </summary>
    public List<Media> Medias = new List<Media>();

    public void HandleMedia(string data, string instance)
    {
      if (data.Length > 0)
      {
        if (!data.StartsWith("http://"))
          data = Constants.WebsiteMediaLocation + instance + "/" + data;
        Medias.Add(new Media(data));
      }
      else
        ErrorHandler.ShowWarning("Missing info. Use \"image='image'\"!");
    }

    public void HandleTactic(string data, List<PathData> paths)
    {
      if (data.Length == 0)
      {
        ErrorHandler.ShowWarning("Missing info. Use \"tactic='tactic1'|'tactic2'|'tacticN'\"!");
        return;
      }
      var split = new List<string>(data.Split('|'));
      var scales = "";
      // Tactics may have information about their fractal scale.
      var useless = 0;
      var subSplit = split[split.Count - 1].Split('-');
      if (int.TryParse(subSplit[0], out useless) && (subSplit.Length == 1 || int.TryParse(subSplit[1], out useless)))
      {
        scales = split[split.Count - 1];
        split.RemoveAt(split.Count - 1);
      }
      var name = string.Join("|", split);
      Tactics.AddTactics(name, scales, paths);
    }

    /// <summary>
    /// Replaces enemy and other link tags with html.
    /// </summary>
    public virtual void CreateLinks(List<string> paths, List<Enemy> enemies)
    {
      name = LinkGenerator.CreateLinks(Name, paths, enemies);
      Tactics.CreateLinks(paths, enemies);
    }
  }
}
