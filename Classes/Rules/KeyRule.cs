using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwitchKeyboard.Enums;
using WindowsInput;

namespace TwitchKeyboard.Classes.Rules
{
  public class KeyRule : BaseRule
  {
    public List<VirtualKeyCode> keys = new();
    public KeyPressMode mode = KeyPressMode.Press;
    public int duration = 0;

    public override string GetName()
    {
      string keysDesc = "";
      for (int i = 0; i < keys.Count; i++)
      {
        keysDesc += $"{KeyInterop.KeyFromVirtualKey((int)keys[i])}+";
      }

      return keysDesc[0..^1];
    }
  }
}
