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
    /// Controls the operation of a specific mouse rule, based on its model
    /// </summary>
    public class MouseRuleController : BaseRuleController
    {
        public override ManagerType cType { get => ManagerType.MOUSE; }

        public int curTime = 0;
    }
}
