
namespace DataCreator.Utility
{
  /// <summary>
  /// An object for a pair of tag and data.
  /// </summary>
  public class TagData
  {
    public string Data;
    public string Tag;

    public TagData(string tag, string data)
    {
      Tag = tag;
      Data = data;
    }

    /// <summary>
    /// Constructs TagData from a line with format "tag separator data".
    /// </summary>
    public static TagData FromLine(string line, char separator)
    {
      var separatorIndex = line.IndexOf(separator);
      if (separatorIndex < 0)
        return new TagData("", line);
      return new TagData(line.Substring(0, separatorIndex), line.Substring(separatorIndex + 1));
    }

    /// <summary>
    /// Constructs TagData from a line with other stuff.
    /// </summary>
    // Very specialized at the moment so not very useful.
    public static TagData FromString(string line, char separator, ref int startIndex, char[] frontSeparators, char[] backSeparators)
    {
      var index = line.IndexOf(separator, startIndex);
      if (index < 0)
        return null;
      // '\' means that the next character should be treated as a pure text.
      while (index > 0 && line[index - 1] == '\\')
      {
        index = line.IndexOf(separator, index + 1);
        if (index < 0)
          return null;
      }
      if (index == line.Length - 1)
      {
        ErrorHandler.ShowWarning("Line has a separator character '" + separator + "' at the end. Remove or add '\' before it.");
        return null;
      }
      var tagIndex = Helper.LastIndexOf(line, frontSeparators, index);
      var tag = line.Substring(tagIndex + 1, index - tagIndex - 1);
      var dataIndex = Helper.FirstIndexOf(line, backSeparators, index);
      if (line[dataIndex - 1] == '!' || line[dataIndex - 1] == ',' || line[dataIndex - 1] == '.' || line[dataIndex - 1] == ':')
        dataIndex--;
      startIndex = dataIndex;
      return new TagData(line.Substring(tagIndex + 1, index - tagIndex - 1), line.Substring(index + 1, dataIndex - index - 1));
    }
  }
}
