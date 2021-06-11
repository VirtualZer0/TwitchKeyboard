using Classes.APIModels.TwitchGQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes
{
    public class UserSettings
    {
        public const int settingsVersion = 1;
        public string lang = System.Globalization.CultureInfo.CurrentCulture.Name;
        public string channel = "";
        public CustomReward[] rewardsCache = Array.Empty<CustomReward>();

        public Dictionary<ManagerType, string> activePresets = new()
        {
            {ManagerType.KEYBOARD,"Default"},
            {ManagerType.MOUSE,"Default"},
            {ManagerType.SFX,"Default"},
            {ManagerType.WEB,"Default"},
            {ManagerType.CMD,"Default"}
        };

        public Dictionary<string, KeyRule[]> keyRulesPreset = new() { { "Default", Array.Empty<KeyRule>() } };
        public Dictionary<string, MouseRule[]> mouseRulesPreset = new() { { "Default", Array.Empty<MouseRule>() } };

        public int mainSfxVolume = 100;
    }
}
