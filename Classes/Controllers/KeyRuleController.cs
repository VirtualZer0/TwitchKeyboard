using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes.Controllers
{
    /// <summary>
    /// Controls the operation of a specific key rule, based on its model
    /// </summary>
    public class KeyRuleController : BaseRuleController
    {
        public override ManagerType cType { get => ManagerType.KEYBOARD; }

        public int curTime = 0;
        public int curSpamDelay = 0;
    }
}
