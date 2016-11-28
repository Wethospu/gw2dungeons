using DataCreator.Enemies;
using DataCreator.Shared;
using DataCreator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCreator.Encounters
{
  public class Instability : IComparable
  {
    public string Name;
    public string Image;
    public string Description;
    public List<string> Text;
    private StringBuilder CachedHtml;

    public Instability()
    {
      Name = "";
      Image = "";
      Description = "";
      Text = new List<string>();
      CachedHtml = null;
    }

    public int CompareTo(object obj)
    {
      var toCompare = (Instability)(obj);
      if (toCompare == null)
        return 0;
      return Name.CompareTo(toCompare.Name);
    }

    public void GenerateHtml(List<Enemy> enemyData)
    {
      if (CachedHtml != null)
        return;
      var nameWithoutSpaces = Name.Replace(' ', '_');
      CachedHtml = new StringBuilder();
      string link = "wiki=Mistlock_Instability:_" + nameWithoutSpaces + "|" + nameWithoutSpaces;
      CachedHtml.Append("<p>").Append(LinkGenerator.CreateLinks(link, new List<string>(), enemyData)).Append(": ").Append(LinkGenerator.CreateLinks(Description, new List<string>(), enemyData)).Append("</p>").Append(Constants.ForcedLineEnding);
      foreach (var line in Text)
        CachedHtml.Append("<p>").Append(LinkGenerator.CreateLinks(line, new List<string>(), enemyData)).Append(" </p>").Append(Constants.ForcedLineEnding);
    }

    public StringBuilder GetHtml()
    {
      if (CachedHtml == null)
        GenerateHtml(new List<Enemy>());
      return CachedHtml;
    }
  }
}
