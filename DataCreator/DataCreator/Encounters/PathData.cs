using System.IO;
using DataCreator.Utility;

namespace DataCreator.Encounters
{

  /***********************************************************************************************
   * PathData / 2014-08-01 / Wethospu                                                            *
   *                                                                                             *
   * Object for one path.                                                                        *
   *                                                                                             *
   ***********************************************************************************************/

  public class PathData
  {
    public string Tag = "";
    public string InstanceName = "";
    public string NameLong = "";
    public string Name = "";
    public string Map = "";
    public string Filename = "";
    public int Scale = 0;


    public PathData(string str, string instance)
    {
      Load(str, instance);
    }

    /***********************************************************************************************
     * Load / 2014-08-01 / Wethospu                                                                *
     *                                                                                             *
     * Loads info from a string.                                                                   *
     *                                                                                             *
     ***********************************************************************************************/

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
          NameLong = elements[2];
        if (elements.Length > 3)
          Name = elements[3];
        Filename = Tag;
        if (elements.Length > 4)
        {
          Scale = Helper.ParseI(elements[4]);
          Filename = "f" + Scale;
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

    /***********************************************************************************************
     * CheckPathMap / 2015-02-14 / Wethospu                                                        *
     *                                                                                             *
     * Checks whether the path has a map available.                                                *
     *                                                                                             *
     ***********************************************************************************************/

    private void CheckPathMap(string instance)
    {
      string fileName = Constants.LocalMediaFolder + instance + "\\" + Tag.ToLower() + "_map.jpg";
      if (File.Exists(Constants.DataOtherRaw + fileName))
        Map = fileName;
    }
  }
}
