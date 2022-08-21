using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchKeyboard.Classes.Rules
{
  public class BaseRule
  {
    public string name = "BaseRule";
    public List<TwitchTrigger> events = new();
    public int cooldown = 0;
    public int delay = 0;

    public bool customSfxEnabled = false;
    public Uri customSfxPath = null;
    public int customSfxVolume = 100;

    public virtual string GetName()
    {
      return "Base rule";
    }
  }
}
