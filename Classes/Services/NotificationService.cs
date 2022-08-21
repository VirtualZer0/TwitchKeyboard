using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchKeyboard.Classes.Services
{
  public class NotificationService
  {
    readonly HttpListener listener = new();
    readonly List<LaunchedRuleIndicator> indicators = new();

    Thread listenThread;
    bool dontStop = true;

    const string addr = "http://localhost:51473/";

    public void Start()
    {
      listener.Prefixes.Add(addr);
      listener.Start();
      listenThread = new Thread(this.Listen);
      listenThread.Start();
    }

    public void Stop()
    {
      try
      {
        dontStop = false;
        listener.Close();
      }
      catch { }

    }

    public string GetURL()
    {
      return addr;
    }

    public void AddEvent(LaunchedRuleIndicator indicator)
    {
      this.indicators.Add(indicator);
    }

    private void Listen()
    {
      HttpListenerContext context = null;
      while (dontStop)
      {
        context = listener.GetContext();

        Response(context);

        string url = context.Request.RawUrl.TrimEnd('/');
        var splittedUrl = url.Split('/');
        if (url == "")
        {
          SendFile("index.html", context);
        }
        else if (url.Contains("/static/"))
        {
          SendFile($"static/{splittedUrl[^1]}", context);
        }
        else if (url.Contains("/data"))
        {
          byte[] buffer = Encoding.UTF8.GetBytes(
              JsonConvert.SerializeObject(indicators)
          );
          context.Response.AddHeader("Content-Type", "application/json;charset=UTF-8");
          context.Response.ContentLength64 = buffer.Length;
          context.Response.OutputStream.Write(buffer, 0, buffer.Length);

          indicators.Clear();
        }
        else
        {
          context.Response.StatusCode = 404;
        }

        context.Response.OutputStream.Close();
        context.Response.Close();
      }
    }

    private void SendFile(string file, HttpListenerContext context)
    {
      context.Response.SendChunked = false;

      if (!File.Exists($"./Notifications/{file}"))
      {
        context.Response.StatusCode = 404;
        return;
      }

      using FileStream fs = File.OpenRead($"./Notifications/{file}");
      byte[] buffer = new byte[64 * 1024];
      int read;
      context.Response.ContentLength64 = fs.Length;

      using BinaryWriter bw = new(context.Response.OutputStream);
      while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
      {
        try
        {
          bw.Write(buffer, 0, read);
          bw.Flush();
        }
        catch { }
      }

      bw.Close();
    }

    private void Response(Object state)
    {
      HttpListenerContext context = (HttpListenerContext)state;

    }
  }
}
