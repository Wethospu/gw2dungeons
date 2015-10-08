using DataCreator.Shared;
using System.IO;
using System.Text;

namespace DataCreator.Utility
{

  /***********************************************************************************************
   * Constants / 2014-08-01 / Wethospu                                                           *
   *                                                                                             *
   * List of constants used in program. Also contains some rarely set values like settings.      *
   *                                                                                             *
   ***********************************************************************************************/

  public class Media
  {
    public string Link;
    public int Width = 0;
    public int Height = 0;

    public Media()
    {

    }

    public Media(Media image)
    {
      if (image.Link != null)
        Link = string.Copy(image.Link);
      Width = image.Width;
      Height = image.Height;
    }

    public Media(string data)
    {
      // Previously data string included width and height. This has been replaced by an automatic system.
      // Check for redundant parameters.
      if (data.Split('|').Length > 1)
        Helper.ShowWarning("Unnecessary parameters. Please remove!");
      var split = data.Split('|');
      Link = split[0];
      // Allow local media links.
      if (Link.Contains("media="))
        Link = Link.Replace("media=", "http://gw2dungeons.net/media/dungeonimages/");
      LinkGenerator.VerifyLink(Link);
      LinkGenerator.BackupAndUpdateSize(Link);
      // Get size from media size file.
      if (Constants.MediaSizes.ContainsKey(Link))
      {
        Width = Constants.MediaSizes[Link][0];
        Height = Constants.MediaSizes[Link][1];
      }
      // Transform gif links to more efficient forms.
      if (Link.Contains("imgur.com"))
        Link = Link.Replace(".gif", ".gifv");
      if (Link.Contains("gfycat.com"))
      {
        Link = Link.Replace(".gif", "");
        Link = Helper.RemoveBetween(Link, "//", "gfycat.com");
      }
    }

    public string ToLink()
    {
      if (Link == null)
        return "";
      var builder = new StringBuilder();
      builder.Append("<a class=\"overlay-link\" href=\"").Append(Link).Append("\"");
      if (Width != 0)
        builder.Append(" data-width=\"").Append(Width).Append("\"");
      if (Height != 0)
        builder.Append(" data-height=\"").Append(Height).Append("\"");
      builder.Append("><span class=\"glyphicon glyphicon-eye-open\"></span></a>");
      return builder.ToString();
    }

    public string ToHtml()
    {
      if (Link == null)
        return "";
      var builder = new StringBuilder();
      builder.Append("<a class=\"overlay-link\" href=\"").Append(Link).Append("\"");
      if (Width != 0)
        builder.Append(" data-width=\"").Append(Width).Append("\"");
      if (Height != 0)
        builder.Append(" data-height=\"").Append(Height).Append("\"");
      builder.Append("><img class=\"thumb-image\" border=\"0\" alt=\"Thumb\" src=\"\" data-name=\"");
      builder.Append(Path.GetFileNameWithoutExtension(Link)).Append(".jpg");
      builder.Append("\"></a>");
      return builder.ToString();
    }

  }
}
