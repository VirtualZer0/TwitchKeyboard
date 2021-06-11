using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Enums;
using WindowsInput.Native;

namespace TwitchKeyboard.Classes.Rules
{
    public class KeyRule : BaseRule
    {
        public List<VirtualKeyCode> keys = new();
        public KeyPressMode mode = KeyPressMode.Press;
        public int duration = 0;
    }
}
