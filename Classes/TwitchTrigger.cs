using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes
{
    public class TwitchTrigger
    {
        public TwitchEvent type { get; set; } = TwitchEvent.MESSAGE;
        public string rewardId { get; set; } = null;
        public int bitsFrom { get; set; } = 10;
        public int bitsTo { get; set; } = 10;

        public bool repeated { get; set; } = false;
        public int repeatTimes { get; set; } = 3;
        public int repeatDuration { get; set; } = 5000;
        public bool repeatUniqueUsers { get; set; } = false;
        public bool repeatResetTime { get; set; } = false;

        public TextCompareMode comparisonMode { get; set; } = TextCompareMode.EQUALS;
        public string text { get; set; } = "";
        public bool caseSensitive { get; set; } = false;

        public TwitchTrigger Copy()
        {
            return (TwitchTrigger) this.MemberwiseClone();
        }
    }
}
