using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchGQL;

namespace TwitchKeyboard.Classes
{
    public class UserSettings
    {
        public string channel { get; set; } = "";
        public List<CustomReward> rewardsCache = new List<CustomReward>();
        public List<KeyRule> rules = new List<KeyRule>();
    }
}
