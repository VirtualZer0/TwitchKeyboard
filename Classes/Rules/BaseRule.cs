using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchKeyboard.Classes.Rules
{
    public class BaseRule
    {
        public List<TwitchTrigger> events = new();
        public int cooldown = 0;
        public int delay = 0;
    }
}
