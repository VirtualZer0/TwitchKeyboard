using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Windows;

namespace TwitchKeyboard.Components.RuleLists
{
    public interface IRuleList
    {
        public void Init(MainWindow window);
        public void ReloadPresets();
        public void ReloadRules();
    }
}
