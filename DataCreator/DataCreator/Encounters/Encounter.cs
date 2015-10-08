using System;
using System.Collections.Generic;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;
using DataCreator.Enemies;

namespace DataCreator.Encounters
{

  /***********************************************************************************************
   * Encounter / 2014-08-01 / Wethospu                                                           *
   *                                                                                             *
   * Object for one encounter. Contains names and list of tactics.                               *
   *                                                                                             *
   ***********************************************************************************************/

  public class Encounter
  {
    public string Name = "";
    public string Path = "";
    public int Index = 0;
    public List<Media> Medias = new List<Media>();
    public TacticList Tactics = new TacticList();


    /***********************************************************************************************
    * UpdateEnemyTactics / 2014-08-01 / Wethospu                                                   * 
    *                                                                                              * 
    * Updates enemy specific tactics with the tactics from this encounter.                         *
    *                                                                                              *
    ***********************************************************************************************/

    public void UpdateEnemyTactics(List<Enemy> enemies)
    {
      var enemiesToUpdate = LinkGenerator.GetEnemiesFromLinks(Name, Path, enemies);
      var cleaned = LinkGenerator.RemoveLinks(Name);
      foreach (var enemy in enemiesToUpdate)
      {
        // Longer the encounter name, less valid its tactics are. / 2015-08-09 / Wethospu
        var tacticValidity = (double)enemy.Name.Length / cleaned.Length;
        if (enemy.TacticValidity < tacticValidity)
        {
          enemy.TacticValidity = tacticValidity;
          enemy.Tactics = Helper.CloneJson(Tactics);
        }
      }
    }

    /***********************************************************************************************
     * ToHtml / 2014-08-01 / Wethospu                                                              * 
     *                                                                                             * 
     * Converts this encounter object to html representration.                                     *
     *                                                                                             *
     * Returns representation.                                                                     *
     * currentPath: Name of current path. Needed for enemy links.                                  *
     * first: Whether this is the first encounter. Causes bigger label.                            *
     * enemies: List of enemies in the path. Needed for enemy links.                               *
     *                                                                                             *
     ***********************************************************************************************/

    public string ToHtml(PathData currentPath, bool first, List<Enemy> enemies)
    {
      UpdateEnemyTactics(enemies);
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<table class=\"encounter\"><tr>");
      //// Add thumbs.
      htmlBuilder.Append("<td class=\"encounter-left\">").Append(Constants.LineEnding);
      foreach (var media in Medias)
      {
        htmlBuilder.Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(media.ToHtml()).Append(Constants.LineEnding);
        htmlBuilder.Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Constants.LineEnding);
      }
      htmlBuilder.Append("</td>");
      //// Encounters.
      htmlBuilder.Append("<td class=\"encounter-main\">");
      // Add identifer data to main-div.
      htmlBuilder.Append("<div data-name=\"").Append(Helper.Simplify(Name));
      htmlBuilder.Append("\" ");
      htmlBuilder.Append("data-path=\"").Append(Helper.Simplify(Path)).Append("\">").Append(Constants.LineEnding);
      // Name added (color-codes get replaced with colors).
      //builder.Append("    <p class=\"EncounterName\">");
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("<div class=\"in-line\">");
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append(first ? "<h1>" : "<h2>");
      // Because of index file, links have to be added this late to the encounter name.
      htmlBuilder.Append(Helper.ConvertSpecial(LinkGenerator.CreateEnemyLinks(LinkGenerator.CreatePageLinks(Name), Path, enemies)));
      htmlBuilder.Append(first ? "</h1>" : "</h2>");
      // Add map link if needed.
      if (currentPath.PathMap.Length > 0)
        htmlBuilder.Append(Constants.Space).Append(Constants.Space).Append("<a class=\"overlay-link\" href=\"").Append(currentPath.PathMap).Append("\"><span class=\"glyphicon glyphicon-picture\"></span></a>");
      htmlBuilder.Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("</div>");
      // Add tactics. / 2015-08-09 / Wethospu
      htmlBuilder.Append(Tactics.ToHtml(Index, Path, enemies, 1));
      htmlBuilder.Append("</div></td>");
      // Space for ads. / 2015-08-04 / Wethospu
      htmlBuilder.Append("<td class=\"encounter-right\">").Append(Constants.LineEnding);
      htmlBuilder.Append("</td>");
      htmlBuilder.Append("</tr></table>").Append(Constants.LineEnding).Append("<br/>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }
  }
}
