using DataCreator.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This little tool converts imgur gifs to gfycat gifs while also changes the media link.
/// </summary>
namespace GifConverter
{
  class Program
  {
    static void Main(string[] args)
    {
      var changes = new SuccessFailure();
      Directory.SetCurrentDirectory("..");
      foreach (var file in Directory.EnumerateFiles(Constants.DataDungeonsRaw))
        changes += HandleLinks(file);
      foreach (var file in Directory.EnumerateFiles(Constants.DataRaidsRaw))
        changes += HandleLinks(file);
      foreach(var file in Directory.EnumerateFiles(Constants.DataFractalsRaw))
        changes += HandleLinks(file);
      foreach (var file in Directory.EnumerateFiles(Constants.DataGuidesRaw))
        changes += HandleLinks(file);
      foreach (var file in Directory.EnumerateFiles(Constants.DataEnemyRaw))
        changes += HandleLinks(file);
      Console.WriteLine("Tool succeeded with " + changes.Success + "/" + changes.Total + " changes. Press a key to exit.");
      Console.ReadKey();
    }

    static SuccessFailure HandleLinks(string file)
    {
      var changes = new SuccessFailure();
      var contents = File.ReadAllText(file);
      var linkStart = 0;
      while (true)
      {
        linkStart = contents.IndexOf("http://i.imgur.com/", linkStart);
        if (linkStart < 0)
          break;
        var linkEnd = contents.IndexOf("gif", linkStart);
        if (linkEnd < 0)
          break;
        if (changes.Total == 0)
          Console.WriteLine("Reading file " + file);
        changes.Total++;
        var imgurUrl = contents.Substring(linkStart, linkEnd - linkStart + 3);
        Console.WriteLine("Converting link " + imgurUrl);
        var requestUrl = "https://upload.gfycat.com/transcode?fetchUrl=" + imgurUrl;
        var request = WebRequest.Create(requestUrl);
        request.ContentType = "application/json; charset=utf-8";
        var response = (HttpWebResponse)request.GetResponse();
        var responseText = "";
        using (var sr = new StreamReader(response.GetResponseStream()))
        {
          responseText = sr.ReadToEnd();
        }
        var splitResponse = responseText.Split('"');
        if (splitResponse.Length < 28)
        {
          Console.WriteLine("Timed out. Please run this tool later.");
          changes.Failure++;
        }
        else
        {
          var gfyUrl = splitResponse[27].Replace("\\/", "/");
          if (gfyUrl.Contains("http"))
          {
            changes.Success++;
            Console.WriteLine("Converted to " + gfyUrl);
            contents = contents.Substring(0, linkStart) + gfyUrl + contents.Substring(linkEnd + 3);
          }
          else
          {
            Console.WriteLine("No url found. Please run this tool later.");
            changes.Failure++;
          }
            
        }
        linkStart = linkEnd;
      }
      if (changes.Success > 0)
      {
        Console.WriteLine("Saving file " + file);
        File.WriteAllText(file, contents);
      }
      return changes;
    }
  }
}
