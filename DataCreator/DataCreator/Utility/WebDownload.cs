using System;
using System.Net;

/// <summary>
/// WebClient override with a custom timeout time. The default length makes downloading lots of files very tedious.
/// </summary>
public class WebDownload : WebClient
{
  public int Timeout { get; set; }

  public WebDownload(int timeout)
  {
    Timeout = timeout;
  }

  protected override WebRequest GetWebRequest(Uri address)
  {
    var request = base.GetWebRequest(address);
    if (request != null)
    {
      request.Timeout = Timeout;
    }
    return request;
  }
}