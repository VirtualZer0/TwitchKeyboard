using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Enums;
using WindowsInput.Native;

namespace TwitchKeyboard.Classes.Rules
{
    public class CmdRule : BaseRule
    {
        public string name = "";
        public string cmd = "";
        public string file = "";
        public bool openFile = false;

        public override string GetName()
        {
            return name;
        }
    }
}
