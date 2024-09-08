using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TwitchKeyboard.Components;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes.Core
{
  public class SettingsManager
  {
    string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchKeyboard");


    public delegate void SettingsLoadedHandler(object sender);
    public event SettingsLoadedHandler SettingsLoaded;

    /// <summary>
    /// Current user settings
    /// </summary>
    UserSettings settings;

    public SettingsManager()
    {
      if (!Directory.Exists(appData))
        Directory.CreateDirectory(appData);

      if (File.Exists("./settings.json") && !File.Exists(Path.Combine(appData, "settings.json")))
      {
        File.WriteAllText(Path.Combine(appData, "settings.json"), File.ReadAllText("./settings.json"));
        File.Delete("./settings.json");
      }
      
      // Load settings
      LoadSettings();
    }

    public void LoadSettings()
    {

      var settingsPath = Path.Combine(appData, "settings.json");

      if (File.Exists(settingsPath))
      {
        settings = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(settingsPath));
        Helper.settings = settings;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(settings.lang);
      }
      else
      {
        settings = new();
        settings.keyRulesPreset.Add("Default", new());
        settings.mouseRulesPreset.Add("Default", new());
        settings.sfxRulesPreset.Add("Default", new());
        settings.webRulesPreset.Add("Default", new());
        settings.cmdRulesPreset.Add("Default", new());

        Helper.settings = settings;

        File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings));
      }
    }

    public void SaveSettings()
    {
      var settingsPath = Path.Combine(appData, "settings.json");
      File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings));
    }
  }
}
