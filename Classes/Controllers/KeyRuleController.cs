using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Classes.Rules;

namespace TwitchKeyboard.Classes.Controllers
{
    /// <summary>
    /// Controls the operation of a specific key rule, based on its model
    /// </summary>
    public class KeyRuleController : BaseRuleController
    {
        public int curTime = 0;
    }
}
