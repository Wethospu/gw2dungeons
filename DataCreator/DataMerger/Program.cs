using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace DataMerger
{
  // Merges multiple enemy id json files to a single file. / 2016-09-01 / Wethospu
  // A single file is easier to manage because all info is in the single place. / 2016-09-01 / Wethospu
  class Program
  {
    static void Main(string[] args)
    {
      Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
      // Custom comparer is needed to sort json ids correctly. / 2016-09-01 / Wethospu
      SortedDictionary<string, EnemyData> combined = new SortedDictionary<string, EnemyData>(new Comparer());

      var dataFiles = Directory.GetFiles(".\\");
      foreach (var file in dataFiles)
      {
        // Result.json is the output file so don't include it again. / 2016-01-09 / Wethospu
        if (Path.GetExtension(file) != ".json" || Path.GetFileNameWithoutExtension(file).Equals("result"))
          continue;
        if (!File.Exists(file))
        {
          Console.WriteLine("File " + file + " doesn't exist!");
          continue;
        }
        // Read the file. / 2015-10-11 / Wethospu
        Dictionary<string, EnemyData> enemyData;
        using (StreamReader r = new StreamReader(file))
        {
          string json = r.ReadToEnd();
          enemyData = JsonConvert.DeserializeObject<Dictionary<string, EnemyData>>(json);
        }
        // Merge it. / 2015-10-11 / Wethospu
        foreach (var key in enemyData.Keys)
        {
          if (combined.ContainsKey(key))
          {
            // Check which occurrences already exist. / 2015-10-11 / Wethospu
            var occurrenceList = combined[key].Occurrences.ToList();
            foreach (var occurrence in enemyData[key].Occurrences)
            {
              var match = false;
              foreach (var occurrence2 in occurrenceList)
              {
                if (occurrence.MapID != occurrence2.MapID && occurrence.MapID > -1 && occurrence2.MapID > -1)
                  continue;
                if (occurrence.Health != occurrence2.Health && occurrence.Health > -1 && occurrence2.Health > -1)
                  continue;
                if (occurrence.Level != occurrence2.Level && occurrence.Level > -1 && occurrence2.Level > -1)
                  continue;
                if (occurrence.ScaledLevel != occurrence2.ScaledLevel && occurrence.ScaledLevel > -1 && occurrence2.ScaledLevel > -1)
                  continue;
                if (occurrence.Position != null && occurrence2.Position != null)
                {
                  if (occurrence.Position.X != occurrence2.Position.X)
                    continue;
                  if (occurrence.Position.Y != occurrence2.Position.Y)
                    continue;
                  if (occurrence.Position.Z != occurrence2.Position.Z)
                    continue;
                }
                if (occurrence.Stamp != occurrence2.Stamp && occurrence.Stamp > -1 && occurrence2.Stamp > -1)
                  continue;
                match = true;
                // Merge data. / 2015-10-11 / Wethospu
                if (occurrence2.MapID < 0)
                  occurrence2.MapID = occurrence.MapID;
                if (occurrence2.Health < 0)
                  occurrence2.Health = occurrence.Health;
                if (occurrence2.Level < 0)
                  occurrence2.Level = occurrence.Level;
                if (occurrence2.ScaledLevel < 0)
                  occurrence2.ScaledLevel = occurrence.ScaledLevel;
                if (occurrence2.Stamp < 0)
                  occurrence2.Stamp = occurrence.Stamp;
                if (occurrence2.Position == null)
                  occurrence2.Position = occurrence.Position;
              }
              if (!match)
              {
                occurrenceList.Add(occurrence);
              }
            }
            occurrenceList.Sort();
            combined[key].Occurrences = occurrenceList.ToArray();
          }
          else
            combined.Add(key, enemyData[key]);
        }
      }
      // Initialize all values and check for errors. / 2015-10-11 / Wethospu
      foreach (var value in combined.Values)
      {
        for (var i = 0; i < value.Occurrences.Length; i++)
        {
          if (value.Occurrences[i].Position == null)
            value.Occurrences[i].Position = new Position();
          if (value.Occurrences[i].Level > 1000)
          {
            // Remove corrupted data. / 2015-10-11 / Wethospu
            var list = value.Occurrences.ToList();
            list.RemoveAt(i);
            value.Occurrences = list.ToArray();
          }
        }
      }
      // Save the result. / 2015-10-11 / Wethospu
      using (StreamWriter r = new StreamWriter("result.json"))
      {
        using (JsonTextWriter jw = new JsonTextWriter(r))
        {
          jw.Formatting = Formatting.Indented;
          jw.IndentChar = ' ';
          jw.Indentation = 4;

          JsonSerializer serializer = new JsonSerializer();
          serializer.Serialize(jw, combined);
        }
      }
    }
  }
}
