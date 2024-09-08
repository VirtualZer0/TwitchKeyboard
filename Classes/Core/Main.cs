using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Classes.Managers;
using TwitchKeyboard.Classes.Services;
using TwitchKeyboard.Enums;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Classes.Managers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Classes.Services;
using TwitchKeyboard.Components.RuleLists;
using TwitchKeyboard.Enums;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using WindowsInput;
using TwitchKeyboard.Windows;

namespace TwitchKeyboard.Classes.Core
{
  public class Main
  {
    /// <summary>
    /// Memleak checker, crashes app when used ram more than 1Gb
    /// </summary>
    readonly MemleakCheckService memleakCheckService = new MemleakCheckService();

    /// <summary>
    /// Twitch controller, controls chat connection and Twitch API requests
    /// </summary>
    readonly TwitchService twitch = new();

    /// <summary>
    /// Notifications server
    /// </summary>
    readonly NotificationService notifications = new();

    /// <summary>
    /// All managers
    /// </summary>
    readonly BaseRuleManager[] managers = new BaseRuleManager[(int)ManagerType.MANAGERS_COUNT];

    /// <summary>
    /// Sound for notifications
    /// </summary>
    readonly MediaPlayerExt notificationPlayer = new();

    /// <summary>
    /// Palette helper for chaange themes
    /// </summary>
    readonly PaletteHelper paletteHelper = new();

    /// <summary>
    /// Contains all available colors
    /// </summary>
    readonly SwatchesProvider swatchesProvider = new();

    

    /// <summary>
    /// Timer for KeyRuleManager and MouseRuleManager
    /// because for these managers we need higher accuracy and shorter response times
    /// </summary>
    public Timer inputOperationsTimer;

    /// <summary>
    /// Timer for all other managers
    /// </summary>
    public Timer otherOperationsTimer;

    public readonly SettingsManager settingsManager = new();

    public delegate void SetNotificationUrlHandler(object sender, string e);
    public event SetNotificationUrlHandler SetNotificationUrl;

    public void Init()
    {
      // Run memleak checker
      memleakCheckService.Start();

      // Bind exception catching
      Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;

      // Load settings from file
      settingsManager.LoadSettings();

      // Launch notifications server
      notifications.Start();

      // Set URL

      SetNotificationUrl?.Invoke(this, notifications.GetURL());
      //mainWindow.notificationsUrlValue.Text = this.notifications.GetURL();

      // Set current theme
      SetTheme();

      // Load notification sound
      notificationPlayer.Open(new Uri(settings.notificationFile, UriKind.Relative));
      notificationPlayer.Volume = settings.notificationVolume / 100.0;
      notificationFileButton.Content = notificationPlayer.Source == null ?
          "Select file" : System.IO.Path.GetFileName(notificationPlayer.Source.ToString());

      // Create managers
      managers[(int)ManagerType.KEYBOARD] = new KeyRuleManager();
      managers[(int)ManagerType.MOUSE] = new MouseRuleManager();
      managers[(int)ManagerType.SFX] = new SfxRuleManager();
      managers[(int)ManagerType.WEB] = new WebRuleManager();
      managers[(int)ManagerType.CMD] = new CmdRuleManager();

      for (int i = 0; i < managers.Length; i++)
      {
        if (managers[i] == null) continue;
        managers[i].OnRuleActivate += RuleMnager_OnRuleActivate;
      }

      // Initialize managers
      InitManager<KeyRuleController, KeyRule>(ManagerType.KEYBOARD, settings.keyRulesPreset);
      InitManager<MouseRuleController, MouseRule>(ManagerType.MOUSE, settings.mouseRulesPreset);

      InitManager<SfxRuleController, SfxRule>(ManagerType.SFX, settings.sfxRulesPreset);
      InitManager<WebRuleController, WebRule>(ManagerType.WEB, settings.webRulesPreset);
      InitManager<CmdRuleController, CmdRule>(ManagerType.CMD, settings.cmdRulesPreset);

      // Fill rule lists
      keyRuleList.Init(this);
      mouseRuleList.Init(this);
      sfxRuleList.Init(this);
      webRuleList.Init(this);
      cmdRuleList.Init(this);

      // Bind events
      twitch.OnConnectionStateChanged += Twitch_OnConnectionStateChanged;
      twitch.OnMessage += Twitch_OnMessage;
      twitch.OnReward += Twitch_OnReward;
      twitch.OnBits += Twitch_OnBits;
      twitch.OnNewSubscribe += Twitch_OnNewSubscribe;
      twitch.OnReSubscribe += Twitch_OnReSubscribe;
      twitch.OnGiftSubscribe += Twitch_OnGiftSubscribe;
      twitch.OnRaid += Twitch_OnRaid;

      // Launch keyboard/mouse timer
      inputOperationsTimer = new(UpdateInputManagers, null, 0, 28);

      // Launch other managers timer
      otherOperationsTimer = new(UpdateOtherManagers, null, 100, 100);

      // Load available themes into theme list
      foreach (var color in swatchesProvider.Swatches)
      {
        themeList.Items.Add(color.Name);
      }

      themeList.SelectedItem = settings.primaryColor;
      themeDarkMode.IsChecked = settings.isDarkTheme;
      volumeSfxRuleSlider.Value = settings.mainSfxVolume;

      Task.Run(checkUpdate);

      GC.Collect();
    }
  }
}
