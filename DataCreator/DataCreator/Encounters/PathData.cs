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
    public string PathTag = "";
    public string DungeonName = "";
    public string PathNameLong = "";
    public string PathName = "";
    public string PathMap = "";


    public PathData(string str)
    {
      Load(str);
    }

    /***********************************************************************************************
     * Load / 2014-08-01 / Wethospu                                                                *
     *                                                                                             *
     * Loads info from a string.                                                                   *
     *                                                                                             *
     ***********************************************************************************************/

    private void Load(string str)
    {
      var elements = str.Split(Constants.TagSeparator);
      if (elements.Length > 1)
      {
        elements = elements[1].Split(Constants.Delimiter);
        if (elements.Length != 4)
          Helper.ShowWarning("Missing info. Use \"init='pathTag'|'dungeon name'|'long path name'|'short path name'\"!");
        PathTag = elements[0];
        if (elements.Length > 1)
          DungeonName = elements[1];
        if (elements.Length > 2)
          PathNameLong = elements[2];
        if (elements.Length > 3)
          PathName = elements[3];
      }
      else if (str.IndexOf(Constants.TagSeparator) > -1)
      {
        PathTag = str;
      }
      else
        Helper.ShowWarning("Missing info. Use \"init='pathTag'|'dungeon name'|'long path name'|'short path name'\"!");
      CheckPathMap();
    }

    /***********************************************************************************************
     * CheckPathMap / 2015-02-14 / Wethospu                                                        *
     *                                                                                             *
     * Checks whether the path has a map available.                                                *
     *                                                                                             *
     ***********************************************************************************************/

    private void CheckPathMap()
    {
      if (File.Exists(Constants.DataOtherRaw + "media/dungeonimages/" + PathTag.ToLower() + "_map.jpg"))
        PathMap = "media/dungeonimages/" + PathTag.ToLower() + "_map.jpg";
    }
  }
}
