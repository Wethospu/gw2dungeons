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
     * encounters: Needed to create table of contents.                                             *
     * counter: Number of the encounter. Needed for table of content linking.                      *
     * enemies: List of enemies in the path. Needed for enemy links.                               *
     *                                                                                             *
     ***********************************************************************************************/

    public string ToHtml(PathData currentPath, List<Encounter> encounters, int counter, List<Enemy> enemies)
    {
      UpdateEnemyTactics(enemies);
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<table class=\"encounter\"><tr>");
      //// Add thumbs.
      htmlBuilder.Append("<td class=\"encounter-left\">").Append(Constants.LineEnding);
      // Generate table of contents to the first encounter. / 2015-10-11 / Wethospu
      if (counter == 0)
      {
        htmlBuilder.Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append("<ul class=\"table-of-contents\">").Append(Constants.LineEnding);
        htmlBuilder.Append("<li><h3>Table of contents</h3></li>");
        var tableCounter = 0;
        foreach (var encounter in encounters)
        {
          if (!encounter.Path.ToUpper().Contains(currentPath.Tag.ToUpper()))
            continue;
          // Ignpre the dungeon name because it's long and doesn't add anything. / 2015-10-11 / Wethospu
          if (tableCounter > 0)
          {
            htmlBuilder.Append("<li><a href=\"#").Append(tableCounter).Append("\">");
            htmlBuilder.Append(Helper.ConvertSpecial(LinkGenerator.RemoveLinks(encounter.Name))).Append("</a></li>");
          }
          tableCounter++;
        }
        htmlBuilder.Append("</ul>").Append(Constants.LineEnding);
        htmlBuilder.Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Constants.LineEnding);
      }
      foreach (var media in Medias)
      {
        htmlBuilder.Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(media.ToHtml()).Append(Constants.LineEnding);
        htmlBuilder.Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Constants.LineEnding);
      }
      htmlBuilder.Append("</td>");
      //// Encounters.
      htmlBuilder.Append("<td id=\"").Append(counter).Append("\" class=\"encounter-main\">");
      // Add identifer data to main-div.
      htmlBuilder.Append("<div data-name=\"").Append(Helper.Simplify(Name));
      htmlBuilder.Append("\" ");
      htmlBuilder.Append("data-path=\"").Append(Helper.Simplify(Path)).Append("\">").Append(Constants.LineEnding);
      // Name added (color-codes get replaced with colors).
      //builder.Append("    <p class=\"EncounterName\">");
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("<div class=\"in-line\">");
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append(counter == 0 ? "<h1>" : "<h2>");
      // Because of index file, links have to be added this late to the encounter name.
      htmlBuilder.Append(Helper.ConvertSpecial(LinkGenerator.CreateEnemyLinks(LinkGenerator.CreatePageLinks(Name), Path, enemies, currentPath.Scale)));
      htmlBuilder.Append(counter == 0 ? "</h1>" : "</h2>");
      // Add map link if needed.
      if (currentPath.Map.Length > 0)
        htmlBuilder.Append(Constants.Space).Append(Constants.Space).Append("<a class=\"overlay-link\" href=\"").Append(currentPath.Map).Append("\"><span class=\"glyphicon glyphicon-picture\"></span></a>");
      htmlBuilder.Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("</div>");
      // Add tactics. / 2015-08-09 / Wethospu
      var tacticHtml = Tactics.ToHtml(Index, Path, currentPath.Scale, enemies, 1);
      // Check that the encounter is relevant (has something for this scale). / 2015-10-29 / Wethospu
      if (tacticHtml.Length == 0)
        return "";
      htmlBuilder.Append(tacticHtml);
      htmlBuilder.Append("</div></td>");
      // Space for ads. / 2015-08-04 / Wethospu
      htmlBuilder.Append("<td class=\"encounter-right\">").Append(Constants.LineEnding);
      htmlBuilder.Append("</td>");
      htmlBuilder.Append("</tr></table>").Append(Constants.LineEnding).Append("<br/>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }
  }
}
