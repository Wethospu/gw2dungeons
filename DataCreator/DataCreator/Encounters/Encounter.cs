using System.Collections.Generic;
using System.Text;
using DataCreator.Utility;
using DataCreator.Shared;
using DataCreator.Enemies;

namespace DataCreator.Encounters
{
  /// <summary>
  /// An object for a single encounter. Contains related tactics and media files.
  /// </summary>
  public class Encounter : BaseType
  {
    /// <summary>
    /// Copies this tactic to fitting enemy tactics. This is needed to share boss tactics between encounters and the boss's page.
    /// </summary>
    public void CopyToEnemyTactics(List<Enemy> enemies)
    {
      var enemiesToUpdate = LinkGenerator.GetEnemiesFromLinks(Name, Paths, enemies);
      var nameWithoutLinks = LinkGenerator.RemoveLinks(Name);
      foreach (var enemy in enemiesToUpdate)
      {
        // Longer the encounter name, less valid its tactics are.
        var tacticValidity = (double)enemy.Name.Length / nameWithoutLinks.Length;
        if (enemy.TacticValidity < tacticValidity)
        {
          enemy.TacticValidity = tacticValidity;
          enemy.Tactics = Helper.CloneJson(Tactics);
        }
      }
    }

    /// <summary>
    /// Returns HTML representation for this encounter.
    /// </summary>
    public string ToHtml(int orderNumber, IEnumerable<Encounter> encounters, int fractalScale, string mapFile)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<table class=\"encounter\"><tr>");
      htmlBuilder.Append(GenerateLeftSide(orderNumber, encounters));
      htmlBuilder.Append(GenerateContent(orderNumber, fractalScale, mapFile));
      htmlBuilder.Append("<td class=\"encounter-right\">").Append(Constants.LineEnding);
      htmlBuilder.Append("</td>");
      htmlBuilder.Append("</tr></table>").Append(Constants.LineEnding).Append("<br/>").Append(Constants.LineEnding);
      return htmlBuilder.ToString();
    }

    /// <summary>
    /// Generates left side for the encounter. Left side is used for media thumbnails but also for the table of contents.
    /// </summary>
    private StringBuilder GenerateLeftSide(int orderNumber, IEnumerable<Encounter> encounters)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<td class=\"encounter-left\">").Append(Constants.LineEnding);
      if (orderNumber == 0)
        htmlBuilder.Append(GenerateTableOfContents(encounters));
      foreach (var media in Medias)
      {
        htmlBuilder.Append("<div>").Append(Constants.LineEnding);
        htmlBuilder.Append(media.GetThumbnailHTML()).Append(Constants.LineEnding);
        htmlBuilder.Append("</div>").Append(Constants.LineEnding);
        htmlBuilder.Append(Constants.LineEnding);
      }
      htmlBuilder.Append("</td>");
      return htmlBuilder;
    }

    /// <summary>
    /// Generates a table of contents from given encounters.
    /// </summary>
    private StringBuilder GenerateTableOfContents(IEnumerable<Encounter> encounters)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<div>").Append(Constants.LineEnding);
      htmlBuilder.Append("<ul class=\"table-of-contents\">").Append(Constants.LineEnding);
      htmlBuilder.Append("<li><h3>Table of contents</h3></li>");
      var tableCounter = 0;
      foreach (var encounter in encounters)
      {
        // Ignpre the dungeon name because it's often long and doesn't add any information or functionality.
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
      return htmlBuilder;
    }

    /// <summary>
    /// Generates HTML for the encounters.
    /// </summary>
    private StringBuilder GenerateContent(int orderNumber, int fractalScale, string mapFile)
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append("<td id=\"").Append(orderNumber).Append("\" class=\"encounter-main\">");
      // Add relevant information to start of the encounter so it can be easily found by the website.
      // TODO: Not sure are these truly needed anymore. 
      htmlBuilder.Append("<div data-name=\"").Append(DataName).Append("\" ");
      htmlBuilder.Append("data-path=\"").Append(Helper.Simplify(string.Join("|", Paths))).Append("\">").Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("<div class=\"in-line\">");
      htmlBuilder.Append(Gw2Helper.AddTab(2)).Append(orderNumber == 0 ? "<h1>" : "<h2>");
      // Because of the index file, links have to be added this late to the encounter name.
      htmlBuilder.Append(Helper.ConvertSpecial(Name));
      htmlBuilder.Append(orderNumber == 0 ? "</h1>" : "</h2>");
      if (mapFile.Length > 0)
        htmlBuilder.Append(Constants.Space).Append(Constants.Space).Append("<a class=\"overlay-link\" href=\"").Append(mapFile).Append("\"><span class=\"glyphicon glyphicon-picture\"></span></a>");
      htmlBuilder.Append(Constants.LineEnding);
      htmlBuilder.Append(Gw2Helper.AddTab(1)).Append("</div>");
      htmlBuilder.Append(Tactics.ToHtml(Index, 1, fractalScale));
      htmlBuilder.Append("</div></td>");
      return htmlBuilder;
    }
  }
}
