using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TwitchKeyboard.Classes.Services
{
  public class MemleakCheckService : IDisposable
  {
    Timer timer = new Timer(2000);
    Process proc = Process.GetCurrentProcess();

    public MemleakCheckService()
    {
      timer.Elapsed += Timer_Elapsed;
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      var mem = proc.PrivateMemorySize64 / 1024 / 1204;
      if (mem > 1024)
      {
        Environment.FailFast("Memory leak detected: " + mem + " MB");
      }
    }

    public void Start()
    {
      timer.Start();
    }

    public void Stop()
    {
      timer.Stop();
    }

    public void Dispose()
    {
      timer.Dispose();
      proc.Dispose();
    }
  }
}
