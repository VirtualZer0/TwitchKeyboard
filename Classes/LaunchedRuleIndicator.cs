using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes
{
    public class LaunchedRuleIndicator
    {
        public int delay { get; set; } = 0;
        public int duration { get; set; } = 0;
        public string eventType { get; set; } = "";
        public string text { get; set; } = "";
    }
}
