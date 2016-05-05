using System.IO;
using DataCreator.Utility;

namespace DataCreator.Encounters
{
  /// <summary>
  /// An object for one path in an instance.
  /// </summary>
  public class PathData
  {
    /// <summary>
    /// Short version for the path name. Used to connect things like enemies to this path.
    /// </summary>
    public string Tag = "";
    /// <summary>
    /// Name of the base instance.
    /// </summary>
    public string InstanceName = "";
    /// <summary>
    /// Name for this path. Used for the main page path list and browser title.
    /// </summary>
    public string Name = "";
    /// <summary>
    /// Name for the path in the navigation bar.
    /// </summary>
    // TODO: Figure out is this really needed or could tag be used instead (so many different names).
    public string NavigationName = "";
    /// <summary>
    /// File name for the map image file.
    /// </summary>
    public string Map = "";
    /// <summary>
    /// File name for the generated path. Commonly same as the tag except for fractals which have their scale.
    /// </summary>
    public string Filename = "";
    /// <summary>
    /// Scale of the fractal.
    /// </summary>
    public int FractalScale = 0;


    public PathData(string str, string instance)
    {
      Load(str, instance);
    }

    private void Load(string str, string instance)
    {
      var elements = str.Split(Constants.TagSeparator);
      if (elements.Length > 1)
      {
        elements = elements[1].Split(Constants.Delimiter);
        if (elements.Length != 4 && elements.Length != 5)
          ErrorHandler.ShowWarning("Missing info. Use \"init='pathTag'|'dungeon name'|'long path name'|'short path name'|'scale'\".");
        Tag = elements[0];
        if (elements.Length > 1)
          InstanceName = elements[1];
        if (elements.Length > 2)
          Name = elements[2];
        if (elements.Length > 3)
          NavigationName = elements[3];
        Filename = Tag;
        if (elements.Length > 4)
        {
          FractalScale = Helper.ParseI(elements[4]);
          Filename = "f" + FractalScale;
        }

      }
      else if (str.IndexOf(Constants.TagSeparator) > -1)
      {
        Tag = str;
        Filename = Tag;
      }
      else
        ErrorHandler.ShowWarning("Missing info. Use \"init='pathTag'|'dungeon name'|'long path name'|'short path name'|'scale'\".");
      CheckPathMap(instance);
    }

    /// <summary>
    /// Sets the map file name if a map is available.
    /// </summary>
    // TODO: Remove hardcoding.
    private void CheckPathMap(string instance)
    {
      string fileName = Constants.LocalMediaFolder + instance + "\\" + Tag.ToLower() + "_map.jpg";
      if (File.Exists(Constants.DataOtherRaw + fileName))
        Map = fileName;
    }
  }
}
