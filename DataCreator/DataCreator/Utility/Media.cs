using DataCreator.Shared;
using System.IO;
using System.Text;

namespace DataCreator.Utility
{
  /// <summary>
  /// Datatype for a media file (an image or a video).
  /// </summary>
  public class Media
  {
    /// <summary>
    /// Link to the file location. Usually youtube, gw2wiki or imgur links.
    /// </summary>
    public string Link;
    /// <summary>
    /// Size information. The website uses iframes so it can't get this information on its own.
    /// </summary>
    public int Width = 0;
    /// <summary>
    /// Size information. The website uses iframes so it can't get this information on its own.
    /// </summary>
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
      if (data.Length == 0)
        ErrorHandler.ShowWarning("Missing info. Use \"image='image'\"!");
      Link = data;
      LinkGenerator.VerifyLink(Link);
      LinkGenerator.BackupAndUpdateSize(Link);
      if (Constants.MediaSizes.ContainsKey(Link))
      {
        Width = Constants.MediaSizes[Link][0];
        Height = Constants.MediaSizes[Link][1];
      }
      // Gifs have to be in .gif format so that they can be directly downloaded.
      // However there are more efficient formats available depending on the site.
      if (Link.Contains("imgur.com"))
        Link = Link.Replace(".gif", ".gifv");
      if (Link.Contains("gfycat.com"))
      {
        Link = Link.Replace(".gif", "");
        Link = Helper.RemoveBetween(Link, "//", "gfycat.com");
      }
    }

    /// <summary>
    /// Returns the link as HTML. Contains right HTML classes.
    /// </summary>
    public string GetLinkHTML()
    {
      if (Link == null)
        return "";
      var builder = GetBaseLinkHTML();
      builder.Append("<span class=\"glyphicon glyphicon-eye-open\"></span></a>");
      return builder.ToString();
    }

    /// <summary>
    /// Returns the thumbnail as HTML. Contains a link to the media file.
    /// </summary>
    public string GetThumbnailHTML()
    {
      if (Link == null)
        return "";
      var builder = GetBaseLinkHTML();
      builder.Append("<img class=\"thumb-image\" border=\"0\" alt=\"Thumb\" src=\"\" data-name=\"");
      var folder = "";
      if (Link.Contains("wiki"))
        folder = "wiki/";
      else if (Link.Contains("gfycat"))
        folder = "gfycat/";
      builder.Append(folder).Append(Path.GetFileNameWithoutExtension(Link)).Append(".jpg");
      builder.Append("\"></a>");
      return builder.ToString();
    }

    /// <summary>
    /// Returns the link as HTML. Contains size information.
    /// </summary>
    private StringBuilder GetBaseLinkHTML()
    {
      var baseLink = new StringBuilder();
      baseLink.Append("<a class=\"overlay-link\" href=\"").Append(Link).Append("\"");
      if (Width != 0)
        baseLink.Append(" data-width=\"").Append(Width).Append("\"");
      if (Height != 0)
        baseLink.Append(" data-height=\"").Append(Height).Append("\"");
      baseLink.Append(">");
      return baseLink;
    }

  }
}
