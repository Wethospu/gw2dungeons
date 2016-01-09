using System.Collections.Generic;

namespace DataMerger
{
  class Comparer : IComparer<string>
  {
    // Compares two strings based on their numeric value. / 2016-01-09 / Wethospu
    // This is needed to sort json data because ids are numeric strings. / 2016-01-09 / Wethospu
    public int Compare(string id1, string id2)
    {
      int numberX = 0;
      int numberY = 0;
      int.TryParse(id1, out numberX);
      int.TryParse(id2, out numberY);
      return numberX - numberY;
    }
  }
}
