using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Enums;
using WindowsInput.Native;

namespace TwitchKeyboard.Classes.Rules
{
    public class MouseRule : BaseRule
    {
        public MouseRuleType ruleType = MouseRuleType.LeftButton;
        public KeyPressMode mode = KeyPressMode.Press;
        public int duration = 0;
        public int X = 0;
        public int Y = 0;
        public double speedX = 100;
        public double speedY = 100;

        public override string GetName()
        {
            return ruleType.ToString();
        }
    }
}
