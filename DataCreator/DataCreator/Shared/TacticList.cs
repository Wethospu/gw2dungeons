using DataCreator.Enemies;
using DataCreator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCreator.Shared
{
  public class TacticList
  {
    public List<Tactic> Tactics = new List<Tactic>();


    public TacticList()
    {
    }

    public TacticList(TacticList toCopy)
    {
      foreach (var tactic in toCopy.Tactics)
        Tactics.Add(new Tactic(tactic));
    }

    /***********************************************************************************************
     * AddTactics / 2014-08-01 / Wethospu                                                          *
     *                                                                                             *
     * Enables tactics for this encounter.                                                         *
     *                                                                                             *
     * types: Tactic types to enable.                                                              *
     *                                                                                             *
     ***********************************************************************************************/

    public void AddTactics(string types)
    {
      if (types.Contains(':'))
        Helper.ShowWarning("Potentially wrong syntax. Use \"tactic='tactic1'|'tactic2'|'tacticN'\"!");
      // Check validity of the tactic.
      var splitTypes = types.Split('|');
      foreach (var str in splitTypes)
      {
        if (!Constants.AvailableTactics.Contains(str.ToLower()) && !Constants.AvailableTips.Contains(str.ToLower()))
          Helper.ShowWarning("Tactic or tip " + str + " wasn't found!. Either fix or add to AvailableTactics.txt or AvailableTips.txt!");
        // Check whether that tactic should be added. / 2015-06-28 / JereK
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
          Tactics.Add(new Tactic(str));
      }
      // Enable given types (and disable others).
      foreach (var tactic in Tactics)
        tactic.Activate(types);
    }

    /***********************************************************************************************
     * AddLine / 2015-08-09    / Wethospu                                                          *
     *                                                                                             *
     * Adds a line to active encounters.                                                           *
     *                                                                                             *
     * line: Line to add.                                                                          *
     *                                                                                             *
     ***********************************************************************************************/

    public void AddLine(string line)
    {
      Boolean isAnythingActive = false;
      foreach (var tactic in Tactics)
      {
        if (tactic.AddLine(line))
          isAnythingActive = true;
      }
      if (!isAnythingActive)
        Helper.ShowWarning("No tactic or tip active!");
    }

    /***********************************************************************************************
     * ToHtml / 2014-08-01 / Wethospu                                                              * 
     *                                                                                             * 
     * Converts this encounter object to html representration.                                     *
     *                                                                                             *
     * Returns representation.                                                                     *
     * index: Unique index for the encounter/enemy.                                                *
     * path: Path of the dungeon. Needed to generate enemy links.                                  *
     * indent: Base indent for tactic html.                                                        *
     *                                                                                             *
     ***********************************************************************************************/

    public string ToHtml(int index, string path, List<Enemy> enemies, int indent)
    {
      var htmlBuilder = new StringBuilder();
      /* Example:
      <div class="tactics" id="s1">
         <ul class="nav nav-tabs">
              <li role="representation"><a href="#s1normal">Normal</a></li>
          </ul>
          <div class="tab-content">
          <div class="tab-pane id="s1normal">
              <h3>Normal</h3>
              Normal strategy.
          </div>
          </div>
      </div>
       */
      //// Build tactics. / 2015-06-28 / Wethospu
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<div class=\"tactics\" id=\"s").Append(index).Append("\">").Append(Constants.LineEnding);
      // Count tactics (if only one, don't add the tab)
      var tacticAmount = 0;
      foreach (var tactic in Tactics.Where(tactic => Constants.AvailableTactics.Contains(tactic.Name)))
        tacticAmount++;
      // Add navigation only when more than one tactic. / 2015-07-22 / Wethospu
      if (tacticAmount > 1)
      {
        // Build navigation.
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<ul class=\"nav nav-tabs\">").Append(Constants.LineEnding);
        foreach (var tactic in Tactics.Where(tactic => Constants.AvailableTactics.Contains(tactic.Name)))
          htmlBuilder.Append(Gw2Helper.AddTab(indent + 2)).Append("<li><a href=\"#s").Append(index).Append(Helper.Simplify(tactic.Name)).Append("\" data-toggle=\"tab\">").Append(Helper.ConvertSpecial(Helper.ToUpper(tactic.Name))).Append("</a></li>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("</ul>").Append(Constants.LineEnding);
      }

      // End of navigation.
      htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<div class=\"tab-content\">").Append(Constants.LineEnding);
      foreach (var tactic in Tactics.Where(tactic => Constants.AvailableTactics.Contains(tactic.Name)))
      {
        // Name added. Add text.
        if (tacticAmount > 1)
          htmlBuilder.Append(Gw2Helper.AddTab(indent + 2)).Append("<div class=\"tab-pane\" id=\"s").Append(index).Append(Helper.Simplify(tactic.Name)).Append("\">").Append(Constants.LineEnding);
        foreach (var line in tactic.Lines)
        {
          var str = line;
          str = LinkGenerator.CreateEnemyLinks(str, path, enemies);
          if (str.EndsWith(".."))
            Helper.ShowWarningMessage("Extra dot detected at end of '" + str + "'. Remove it.");
          if (Char.IsLower(str[0]))
            Helper.ShowWarningMessage("Line '" + str + "' starts with a lower character. Fix it.");
          if (!str.EndsWith(".") && !str.EndsWith(":") && !str.EndsWith("!") && !str.EndsWith("\""))
            Helper.ShowWarningMessage("No '.', ':', '!' or '\"' at the end of line '" + str + "'. Add it.");
          htmlBuilder.Append(Gw2Helper.AddTab(1));
          htmlBuilder.Append(Gw2Helper.AddTab(2)).Append("<p>");
          htmlBuilder.Append(Helper.ConvertSpecial(str));
          htmlBuilder.Append("</p>");
          htmlBuilder.Append(Constants.LineEnding);
        }
        if (tacticAmount > 1)
          htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("</div>").Append(Constants.LineEnding);
      }
      htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("</div>").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("</div>").Append(Constants.LineEnding);
      //// End of tactics.
      //// Build tips. / 2015-06-28 / Wethospu
      if (Tactics.Find(tactic => Constants.AvailableTips.Contains(tactic.Name)) != null)
      {
        htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("<div class=\"tips\" id=\"t").Append(index).Append("\">").Append(Constants.LineEnding);
        // Build navigation. / 2015-06-28 / Wethospu
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<ul class=\"nav nav-tabs\">").Append(Constants.LineEnding);
        foreach (var tactic in Tactics.Where(tactic => Constants.AvailableTips.Contains(tactic.Name)))
          htmlBuilder.Append(Gw2Helper.AddTab(indent + 2)).Append("<li><a href=\"#t").Append(index).Append(Helper.Simplify(tactic.Name)).Append("\" data-toggle=\"tab\">").Append("<img class=\"professionIcon\" src=\"media/img/tip_" + Helper.Simplify(tactic.Name) + ".png\">").Append("</a></li>").Append(Constants.LineEnding);
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("</ul>").Append(Constants.LineEnding);

        // End of navigation. / 2015-06-28 / Wethospu
        htmlBuilder.Append(Gw2Helper.AddTab(indent + 1)).Append("<div class=\"tab-content\">").Append(Constants.LineEnding);
        foreach (var tactic in Tactics.Where(tactic => Constants.AvailableTips.Contains(tactic.Name)))
        {
          // Name added. Add text. / 2015-06-28 / Wethospu
          htmlBuilder.Append(Gw2Helper.AddTab(indent + 2)).Append("<div class=\"tab-pane\" id=\"t").Append(index).Append(Helper.Simplify(tactic.Name)).Append("\">").Append(Constants.LineEnding);
          foreach (var line in tactic.Lines)
          {
            var str = line;
            str = LinkGenerator.CreateEnemyLinks(str, path, enemies);
            if (str.EndsWith(".."))
              Helper.ShowWarningMessage("Extra dot detected at end of '" + str + "'. Remove it.");
            if (Char.IsLower(str[0]))
              Helper.ShowWarningMessage("Line '" + str + "' starts with a lower character. Fix it.");
            if (!str.EndsWith(".") && !str.EndsWith(":") && !str.EndsWith("!") && !str.EndsWith("\""))
              Helper.ShowWarningMessage("No '.', ':', '!' or '\"' at the end of line '" + str + "'. Add it.");      
            htmlBuilder.Append(Gw2Helper.AddTab(indent));
            htmlBuilder.Append(Gw2Helper.AddTab(indent + 2)).Append("<p>");
            htmlBuilder.Append(Helper.ConvertSpecial(str));
            htmlBuilder.Append("</p>");
            htmlBuilder.Append(Constants.LineEnding);
          }
          htmlBuilder.Append(Gw2Helper.AddTab(indent + 2)).Append("</div>").Append(Constants.LineEnding);
        }
        htmlBuilder.Append(Gw2Helper.AddTab(indent)).Append("</div>").Append("</div>").Append(Constants.LineEnding);
      }
      return htmlBuilder.ToString();
    }

    // Overrides for standard list stuff.

    public int Count
    {
      get
      {
        return Tactics.Count;
      }    
    }
  }
}
