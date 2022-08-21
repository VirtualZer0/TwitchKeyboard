using Classes.APIModels.TwitchGQL;
using MaterialDesignColors;
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
    public bool hideInTray = false;

    public string notificationFile = "./notify.wav";
    public int notificationVolume = 100;

    public bool isDarkTheme = true;
    public string primaryColor = "deeppurple";

    public CustomReward[] rewardsCache = Array.Empty<CustomReward>();

    public Dictionary<ManagerType, bool> activeNotificationsSound = new()
    {
      { ManagerType.KEYBOARD, false },
      { ManagerType.MOUSE, false },
      { ManagerType.SFX, false },
      { ManagerType.WEB, false },
      { ManagerType.CMD, false }
    };

    public Dictionary<ManagerType, bool> activeNotificationsIndicators = new()
    {
      { ManagerType.KEYBOARD, true },
      { ManagerType.MOUSE, false },
      { ManagerType.SFX, false },
      { ManagerType.WEB, false },
      { ManagerType.CMD, false }
    };

    public Dictionary<ManagerType, string> activePresets = new()
    {
      {ManagerType.KEYBOARD,"Default"},
      {ManagerType.MOUSE,"Default"},
      {ManagerType.SFX,"Default"},
      {ManagerType.WEB,"Default"},
      {ManagerType.CMD,"Default"}
    };

    public Dictionary<string, List<KeyRule>> keyRulesPreset = new();
    public Dictionary<string, List<MouseRule>> mouseRulesPreset = new();
    public Dictionary<string, List<SfxRule>> sfxRulesPreset = new();
    public Dictionary<string, List<WebRule>> webRulesPreset = new();
    public Dictionary<string, List<CmdRule>> cmdRulesPreset = new();

    public int mainSfxVolume = 100;
  }
}
