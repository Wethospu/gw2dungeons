using DataCreator.Encounters;
using DataCreator.Enemies;
using DataCreator.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCreator.Shared
{
  /// <summary>
  /// Wrapper for a list of tactics. Provides needed functionality.
  /// </summary>
  public class TacticList
  {
    public List<Tactic> Tactics = new List<Tactic>();


    public TacticList()
    {
    }

    /// <summary>
    /// Enables tactics so they can receive text data.
    /// </summary>
    public void AddTactics(string types, string scale, List<PathData> paths)
    {
      if (types.Contains(':'))
        ErrorHandler.ShowWarning("Potentially wrong syntax. Use \"tactic='tactic1'|'tactic2'|'tacticN'\"!");
      // Check validity of the tactic.
      var splitTypes = types.Split('|');
      foreach (var str in splitTypes)
      {
        if (!Constants.AvailableTactics.Contains(str.ToLower()) && !Constants.AvailableTips.Contains(str.ToLower()))
          ErrorHandler.ShowWarning("Tactic or tip " + str + " wasn't found!. Either fix or add to AvailableTactics.txt or AvailableTips.txt!");
        var found = false;
        foreach (var tactic in Tactics)
        {
          if (tactic.Name.Equals(str.ToLower()))
          {
            found = true;
            break;
          }
        }
        if (!found)
        {
          Tactics.Add(new Tactic(str));
          // Add a tactic for every fractal scale because all should get data.
          if (paths != null)
          {
            foreach (var path in paths)
            {
              if (path.FractalScale > 0)
                Tactics.Add(new Tactic(str, path.FractalScale));
            }
          }
        }    
      }
      var minScale = 0;
      var maxScale = 100;
      if (scale.Length > 0)
      {
        var split = scale.Split('-');
        minScale = Helper.ParseI(split[0]);
        maxScale = Helper.ParseI(split[split.Length - 1]);
      }
      // Enable given types (and disable others).
      foreach (var tactic in Tactics)
        tactic.Activate(types, minScale, maxScale);
    }

    /// <summary>
    /// Adds a line to enabled tactics.
    /// </summary>
    public void AddLine(string line)
    {
      bool isAnythingActive = false;
      foreach (var tactic in Tactics)
      {
        if (tactic.IsActive)
        {
          tactic.AddLine(line);
          isAnythingActive = true;
        }
      }
      if (!isAnythingActive)
        ErrorHandler.ShowWarning("No tactic or tip active!");
    }

    /// <summary>
    /// Returns whether there are tactics for a given fractal scale.
    /// </summary>
    public bool IsAvailableForScale(int fractalScale)
    {
      return Tactics.Where(tactic => tactic.FractalScale == fractalScale && tactic.Lines.Count > 0).Any();
    }

    /// <summary>
    /// Replaces enemy and other link tags with html.
    /// </summary>
    public void CreateLinks(List<string> paths, List<Enemy> enemies)
    {
      foreach (var tactic in Tactics)
      {
        for (var i = 0; i < tactic.Lines.Count; i++)
        {
          tactic.Lines[i] = LinkGenerator.CreateLinks(tactic.Lines[i], paths, enemies);
        }
      }
    }

    /// <summary>
    /// Returns HTML representation for available tactics.
    /// </summary>
    public string ToHtml(int orderNumber, int baseIndent, int fractalScale = 0)
    {
      /* Format:
      <div class="tactics">
        <ul class="nav nav-tabs">
          <li role="representation"><a href="#t1normal">Normal</a></li>
        </ul>
        <div class="tab-content">
          <div class="tab-pane id="t1normal">
            <h3>Normal</h3>
            Melee tactic strategy.
          </div>
        </div>
      </div>
       */
      var htmlBuilder = new StringBuilder();
      
      htmlBuilder.Append(GenerateTactics(orderNumber, baseIndent, fractalScale));
      htmlBuilder.Append(GenerateTips(orderNumber, baseIndent, fractalScale));
      return htmlBuilder.ToString();
    }

    private StringBuilder GenerateTactics(int index, int baseIndent, int fractalScale)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<div class=\"tactics\">").Append(Constants.LineEnding);

      var availableTactics = new List<Tactic>(Tactics.Where(tactic => Constants.AvailableTactics.Contains(tactic.Name) && tactic.FractalScale == fractalScale));
      if (availableTactics.Count > 0 && availableTactics[0].Lines.Count == 0)
      {
        throw new System.Exception("GenerateTactics: Encounter has no available tactics. This should be checked earlier in the code.");
      }
      htmlBuilder.Append(GenerateNavigation(index, baseIndent + 1, availableTactics, false));
      htmlBuilder.Append(GenerateStuff(index, baseIndent + 1, availableTactics));
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("</div>").Append(Constants.LineEnding);
      //// End of tactics.
      return htmlBuilder;
    }

    private StringBuilder GenerateTips(int index, int baseIndent, int fractalScale)
    {
      var htmlBuilder = new StringBuilder();
      var availableTips = new List<Tactic>(Tactics.Where(tactic => Constants.AvailableTips.Contains(tactic.Name) && tactic.FractalScale == fractalScale));
      if (availableTips.Count == 0)
        return htmlBuilder;

      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("<div class=\"tips\">").Append(Constants.LineEnding);

      htmlBuilder.Append(GenerateNavigation(index, baseIndent + 1, availableTips, true));
      htmlBuilder.Append(GenerateStuff(index, baseIndent + 1, availableTips));
      htmlBuilder.Append(Gw2Helper.AddTab(baseIndent)).Append("</div>").Append(Constants.LineEnding);
      return htmlBuilder;
    }

    private StringBuilder GenerateNavigation(int index, int indent, List<Tactic> availableTactics, bool useImages)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<ul class=\"nav nav-tabs\">").Append(Constants.LineEnding);
      foreach (var tactic in availableTactics)
      {
       htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<li><a href=\"#t").Append(index).Append(Helper.Simplify(tactic.Name)).Append("\" data-toggle=\"tab\">");
        if (useImages)
          htmlBuilder.Append("<img class=\"professionIcon\" src=\"" + Constants.WebsiteIconLocation + Helper.Simplify(tactic.Name) + ".png\">");
        else
          htmlBuilder.Append(Helper.ToUpperAll(tactic.Name));
       htmlBuilder.Append("</a></li>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("</ul>").Append(Constants.LineEnding);
      return htmlBuilder;
    }

    private StringBuilder GenerateStuff(int index, int indent, List<Tactic> availableTactics)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<div class=\"tab-content\">").Append(Constants.LineEnding);
      foreach (var tactic in availableTactics)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<div class=\"tab-pane\" id=\"t").Append(index).Append(Helper.Simplify(tactic.Name)).Append("\">").Append(Constants.LineEnding);
        // Only check the first line (better performance).
        bool first = true;
        foreach (var line in tactic.Lines)
        {
          var str = line;
          if (first && str.Equals(Constants.InstabilityEncounter))
          {
            foreach (var instability in Constants.Instabilities[tactic.FractalScale - 1])
              htmlBuilder.Append(instability.GetHtml());
            first = false;
            continue;
          }
          first = false;
          if (str.EndsWith(".."))
            ErrorHandler.ShowWarningMessage("Extra dot detected at end of '" + str + "'. Remove it.");
          //if (char.IsLower(str[0]))
          //  ErrorHandler.ShowWarningMessage("Line '" + str + "' starts with a lower character. Fix it.");
          if (!str.EndsWith(".") && !str.EndsWith(":") && !str.EndsWith("!") && !str.EndsWith("\"") && !str.EndsWith("?"))
            ErrorHandler.ShowWarningMessage("No '.', ':', '!', '?' or '\"' at the end of line '" + str + "'. Add it.");
          htmlBuilder.Append(Gw2Helper.AddTab(1));
          htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<p>");
          htmlBuilder.Append(Helper.ConvertSpecial(str));
          htmlBuilder.Append("</p>");
          htmlBuilder.Append(Constants.LineEnding);
        }
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("</div>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("</div>").Append(Constants.LineEnding);
      return htmlBuilder;
    }

    /// <summary>
    /// Overrides the standard List.Count.
    /// </summary>
    public int Count
    {
      get
      {
        return Tactics.Count;
      }    
    }
  }
}
