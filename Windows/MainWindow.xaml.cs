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
using TwitchKeyboard.Classes;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Classes.Managers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Classes.Services;
using TwitchKeyboard.Components.RuleLists;
using TwitchKeyboard.Enums;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using WindowsInput;
using WindowsInput.Native;

namespace TwitchKeyboard.Windows
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        /// Current user settings
        /// </summary>
        UserSettings settings;

        /// <summary>
        /// Timer for KeyRuleManager and MouseRuleManager
        /// because for these managers we need higher accuracy and shorter response times
        /// </summary>
        public Timer inputOperationsTimer;

        /// <summary>
        /// Timer for all other managers
        /// </summary>
        public Timer otherOperationsTimer;

        public MainWindow()
        {
            // Bind exception catching
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;

            // Load settings from file
            this.LoadSettings();

            // Launch notifications server
            this.notifications.Start();

            // Set URL
            notificationsUrlValue.Text = this.notifications.GetURL();

            // Set current theme
            SetTheme();

            // Load notification sound
            notificationPlayer.Open(new Uri(settings.notificationFile, UriKind.Relative));
            notificationPlayer.Volume = settings.notificationVolume/100.0;
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

        private void checkUpdate ()
        {
            if (UpdateService.checkUpdate())
            {
                this.Dispatcher.Invoke(() =>
                {
                    UpdateAvailableWindow notification = new();
                    notification.Topmost = true;
                    notification.Show();
                });
            }
        }

        private void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (File.Exists("crashlog.txt"))
            {
                File.Delete("crashlog.txt");
            }

            File.WriteAllText("crashlog.txt", e.Exception.ToString());
        }

        void UpdateInputManagers (Object statenfo)
        {
            managers[(int)ManagerType.KEYBOARD].Update(30);
            managers[(int)ManagerType.MOUSE].Update(30);
        }

        void UpdateOtherManagers(Object statenfo)
        {
            managers[(int)ManagerType.SFX].Update(100);
            managers[(int)ManagerType.WEB].Update(100);
            managers[(int)ManagerType.CMD].Update(100);
        }

        private void toggleManager(object sender, SelectionChangedEventArgs e)
        {
            var button = (ListBox)sender;
            var tag = button.Tag.ToString();
            var mType = (ManagerType)Enum.Parse(typeof(ManagerType), tag);

            if (button.SelectedIndex == 1)
            {
                managers[(int)mType]?.Disable();
            }
            else
            {
                managers[(int)mType]?.Enable();
            }
        }

        // ------------------ Notifications ------------------//

        private void indicatorRuleSwitch_Click(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            var tag = button.Tag.ToString();
            settings.activeNotificationsIndicators[(ManagerType)Enum.Parse(typeof(ManagerType), tag)] = (bool)button.IsChecked;
        }

        private void soundRuleSwitch_Click(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            var tag = button.Tag.ToString();
            settings.activeNotificationsSound[(ManagerType)Enum.Parse(typeof(ManagerType), tag)] = (bool)button.IsChecked;
        }

        private void RuleMnager_OnRuleActivate(object sender, BaseRuleController rule, string user)
        {
            logList.Dispatcher.InvokeAsync(() =>
            {
                if (logList.Items.Count >= 100)
                {
                    logList.Items.RemoveAt(logList.Items.Count - 1);
                }

                logList.Items.Add($"[{DateTime.Now.ToShortTimeString()} - {rule.cType}] {user} {Properties.Resources.t_launch} {rule.model.GetName()}");
            });
            

            if (settings.activeNotificationsSound[rule.cType])
            {
                notificationPlayer.Dispatcher.InvokeAsync(() =>
                {
                    notificationPlayer.Stop();
                    notificationPlayer.Play();
                });
            }

            if (!settings.activeNotificationsIndicators[rule.cType]) return;

            int duration = 0;

            if (rule.model is KeyRule keyRule)
            {
                duration = keyRule.duration;
            }
            else if (rule.model is MouseRule mouseRule)
            {
                duration = mouseRule.duration;
            }

            notifications.AddEvent(new() {
                delay = rule.model.delay,
                eventType = rule.cType.ToString(),
                text = rule.model.GetName(),
                duration = duration
            });
        }

        private void notificationFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Title = Properties.Resources.t_selectAudiofile,
                Filter = "Audiofiles|*.mp3;*.wav;*.mp2;*.mid;*.snd;*.aif;*.aiff;*.aifc"
            };

            if ((bool)fileDialog.ShowDialog())
            {
                settings.notificationFile = fileDialog.FileName;
                notificationPlayer.Open(new Uri(fileDialog.FileName));
                notificationPlayer.Stop();
            }

            notificationFileButton.Content = notificationPlayer.Source == null ?
                Properties.Resources.t_selectFile : System.IO.Path.GetFileName(notificationPlayer.Source.ToString());

            SaveSettings();
        }

        private void notificationVolumeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            notificationPlayer.Volume = notificationVolumeSlider.Value / 100.0;
            settings.notificationVolume = (int)notificationVolumeSlider.Value;
            notificationPlayer.Stop();
            notificationPlayer.Play();
            SaveSettings();
        }

        private void copyNotificationUrl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(notificationsUrlValue.Text);
        }

        // ------------------ Settings ------------------//

        /// <summary>
        /// Save current settings to JSON
        /// </summary>
        public void SaveSettings()
        {
            this.settings.channel = twitchConnect.GetChannel();
            this.settings.rewardsCache = this.twitch.customRewards;

            File.WriteAllText("./settings.json", JsonConvert.SerializeObject(settings));
        }

        /// <summary>
        /// Load current settings from file or create empty savefile
        /// </summary>
        public void LoadSettings ()
        {
            if (File.Exists("./settings.json"))
            {
                settings = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText("./settings.json"));
                Helper.settings = settings;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(settings.lang);
                InitializeComponent();
                twitchConnect.SetChannel(settings.channel);
                twitch.customRewards = settings.rewardsCache;
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

                File.WriteAllText("./settings.json", JsonConvert.SerializeObject(settings));
                InitializeComponent();
            }
            
            

            // Load settings into UI
            indicatorKeyRuleSwitch.IsChecked = settings.activeNotificationsIndicators[ManagerType.KEYBOARD];
            indicatorMouseRuleSwitch.IsChecked = settings.activeNotificationsIndicators[ManagerType.MOUSE];
            indicatorSfxRuleSwitch.IsChecked = settings.activeNotificationsIndicators[ManagerType.SFX];
            indicatorWebRuleSwitch.IsChecked = settings.activeNotificationsIndicators[ManagerType.WEB];
            indicatorCmdRuleSwitch.IsChecked = settings.activeNotificationsIndicators[ManagerType.CMD];

            soundKeyRuleSwitch.IsChecked = settings.activeNotificationsSound[ManagerType.KEYBOARD];
            soundMouseRuleSwitch.IsChecked = settings.activeNotificationsSound[ManagerType.MOUSE];
            soundWebRuleSwitch.IsChecked = settings.activeNotificationsSound[ManagerType.WEB];
            soundCmdRuleSwitch.IsChecked = settings.activeNotificationsSound[ManagerType.CMD];

            notificationVolumeSlider.Value = settings.notificationVolume;

            if (Thread.CurrentThread.CurrentUICulture.Name == "ru-RU")
            {
                languageList.SelectedIndex = 1;
            }

            restartRequiredText.Visibility = Visibility.Collapsed;
        }

        public void SetTheme ()
        {
            
            IBaseTheme baseTheme = settings.isDarkTheme ? new MaterialDesignDarkTheme() : new MaterialDesignLightTheme();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(baseTheme);
            theme.SetPrimaryColor(
                swatchesProvider.Swatches.FirstOrDefault(sw => sw.Name == settings.primaryColor).PrimaryHues[5].Color
            );
            paletteHelper.SetTheme(theme);
        }

        private void themeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (themeList.SelectedItem == null) return;
            settings.primaryColor = themeList.SelectedItem.ToString();
            SetTheme();
            SaveSettings();
        }

        private void themeDarkMode_Click(object sender, RoutedEventArgs e)
        {
            settings.isDarkTheme = (bool)themeDarkMode.IsChecked;
            SetTheme();
            SaveSettings();
        }

        private void volumeSfxRuleSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (managers[(int)ManagerType.SFX] == null) return;
            var sfxManager = (SfxRuleManager)managers[(int)ManagerType.SFX];

            settings.mainSfxVolume = (int)volumeSfxRuleSlider.Value;
            sfxManager.ChangeSFXVolume();
            SaveSettings();
        }

        private void languageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (languageList.SelectedIndex)
            {
                case 0: settings.lang = "en-US"; break;
                case 1: settings.lang = "ru-RU"; break;
            }

            SaveSettings();

            if (restartRequiredText == null) return;
            restartRequiredText.Visibility = Visibility.Visible;
        }


        // ------------------ Presets ------------------//

        /// <summary>
        /// Changes current active preset, reloads controllers and rule lists
        /// </summary>
        /// <typeparam name="TRule">Rule type</typeparam>
        /// <typeparam name="TController">Controller type</typeparam>
        /// <param name="name">Preset name</param>
        /// <param name="mType">Manager type</param>
        /// <param name="storage">Selected preset storage</param>
        // P.S. Maybe this method is too complicated?
        public void SwitchPreset<TRule, TController>(string name, ManagerType mType, Dictionary<string, List<TRule>> storage)
            where TRule : BaseRule
            where TController : BaseRuleController, new()
        {
            settings.activePresets[mType] = name;
            InitManager<TController, TRule>(mType, storage);
            SaveSettings();
        }

        /// <summary>
        /// Creates new preset in selected rule storage and immediately select him
        /// </summary>
        /// <typeparam name="TRule">Rule type</typeparam>
        /// <typeparam name="TController">Controller type</typeparam>
        /// <param name="name">Preset name</param>
        /// <param name="mType">Manager type</param>
        /// <param name="storage">Selected preset storage</param>
        public void CreatePreset<TRule, TController>(string name, ManagerType mType, Dictionary<string, List<TRule>> storage)
            where TRule : BaseRule
            where TController : BaseRuleController, new()
        {
            storage.Add(name, new());
            SwitchPreset<TRule, TController>(name, mType, storage);
        }

        /// <summary>
        /// Duplicates current active preset
        /// </summary>
        /// <typeparam name="TRule">Rule type</typeparam>
        /// <typeparam name="TController">Controller type</typeparam>
        /// <param name="name">Preset name</param>
        /// <param name="mType">Manager type</param>
        /// <param name="storage">Selected preset storage</param>
        public void DuplicatePreset<TRule, TController>(ManagerType mType, Dictionary<string, List<TRule>> storage)
            where TRule : BaseRule
            where TController : BaseRuleController, new()
        {
            string name = settings.activePresets[mType];

            int copyNum = 1;
            while (storage.ContainsKey($"{name}-{copyNum}")) { copyNum++; }

            storage.Add($"{name}-{copyNum}", 
                JsonConvert.DeserializeObject<List<TRule>>(
                    JsonConvert.SerializeObject(
                        storage[settings.activePresets[mType]]
                    )
                )    
            );

            SwitchPreset<TRule, TController>($"{name}-{copyNum}", mType, storage);
        }

        /// <summary>
        /// Renames current active preset
        /// </summary>
        /// <typeparam name="TRule">Rule type</typeparam>
        /// <typeparam name="TController">Controller type</typeparam>
        /// <param name="name">Preset name</param>
        /// <param name="mType">Manager type</param>
        /// <param name="storage">Selected preset storage</param>
        /// <param name="ruleList">Rule list component</param>
        public void RenamePreset<TRule, TController>(string name, ManagerType mType, Dictionary<string, List<TRule>> storage)
            where TRule : BaseRule
            where TController : BaseRuleController, new()
        {
            string oldName = settings.activePresets[mType];
            settings.activePresets[mType] = name;
            storage.Add(name, storage[oldName]);
            storage.Remove(oldName);
            SwitchPreset<TRule, TController>(name, mType, storage);
        }

        /// <summary>
        /// Remove current active preset
        /// </summary>
        /// <typeparam name="TRule">Rule type</typeparam>
        /// <typeparam name="TController">Controller type</typeparam>
        /// <param name="name">Preset name</param>
        /// <param name="mType">Manager type</param>
        /// <param name="storage">Selected preset storage</param>
        /// <param name="ruleList">Rule list component</param>
        public void RemovePreset<TRule, TController>(string name, ManagerType mType, Dictionary<string, List<TRule>> storage)
            where TRule : BaseRule
            where TController : BaseRuleController, new()
        {
            storage.Remove(name);

            if (storage.Count > 0)
            {
                var nextPreset = storage.First();
                settings.activePresets[mType] = nextPreset.Key;
            }
            else
            {
                settings.activePresets[mType] = "Default";
                storage.Add("Default", new());
            }
            
            SwitchPreset<TRule, TController>(settings.activePresets[mType], mType, storage);
        }


        /// <summary>
        /// Loads preset into rules manager and safely restarts it
        /// </summary>
        /// <typeparam name="TController">Rule controller type</typeparam>
        /// <typeparam name="TRule">Rule type</typeparam>
        /// <param name="managerType">Manager type</param>
        /// <param name="storage">Rule storage</param>
        public void InitManager<TController, TRule> (ManagerType managerType, Dictionary<string,List<TRule>> storage)
            where TController : BaseRuleController, new()
            where TRule : BaseRule
        {
            bool reenable = managers[(int)managerType].enabled;
            managers[(int)managerType].Disable();
            managers[(int)managerType].DestroyControllers();
            managers[(int)managerType].CreateControllers<TController>(storage[settings.activePresets[managerType]].ToArray());

            if (reenable)
                managers[(int)managerType].Enable();
        }


        private async void twitchConnect_OnActionClick(object sender, string channelName)
        {
            if (twitch.connectionState != TwitchConnectionState.JOINED)
            {
                await twitch.JoinChannel(channelName);
            }
            else
            {
                twitch.Disconnect();
            }            
        }



        private void keyRuleList_OnRulesChanged(object sender, List<KeyRule> rules)
        {
            InitManager<KeyRuleController, KeyRule>(ManagerType.KEYBOARD, settings.keyRulesPreset);
            SaveSettings();
        }

        private void mouseRuleList_OnRulesChanged(object sender, List<MouseRule> rules)
        {
            InitManager<MouseRuleController, MouseRule>(ManagerType.MOUSE, settings.mouseRulesPreset);
            SaveSettings();
        }

        private void sfxRuleList_OnRulesChanged(object sender, List<SfxRule> rules)
        {
            InitManager<SfxRuleController, SfxRule>(ManagerType.SFX, settings.sfxRulesPreset);
            SaveSettings();
        }

        private void webRuleList_OnRulesChanged(object sender, List<WebRule> rules)
        {
            InitManager<WebRuleController, WebRule>(ManagerType.WEB, settings.webRulesPreset);
            SaveSettings();
        }

        private void cmdRuleList_OnRulesChanged(object sender, List<CmdRule> rules)
        {
            InitManager<CmdRuleController, CmdRule>(ManagerType.CMD, settings.cmdRulesPreset);
            SaveSettings();
        }


        // ------------------ Twitch events ------------------//

        /// <summary>
        /// Raised when Twitch connection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">New connection state</param>
        private void Twitch_OnConnectionStateChanged(object sender, TwitchConnectionState e)
        {
            Dispatcher.Invoke(() =>
            {
                twitchConnect.ChangeConnectionState(e);
            });

            if (e == TwitchConnectionState.CONNECTED)
            {
                settings.rewardsCache = twitch.customRewards;
                SaveSettings();
            }
        }

        /// <summary>
        /// Raised when user activates a reward
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="rewardId">Reward id</param>
        /// <param name="e">Twitch message</param>
        private void Twitch_OnReward(object sender, string rewardId, ChatMessage e)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] == null) continue;
                managers[i].NewReward(e.Username, e.Message, rewardId);
            }
        }

        /// <summary>
        /// Raised when user sends a basic message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Twitch message</param>
        private void Twitch_OnMessage(object sender, ChatMessage e)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] == null) continue;
                managers[i].NewMessage(e.Username, e.Message);
            }
        }

        private void Twitch_OnRaid(object sender, RaidNotification e)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] == null) continue;
                managers[i].NewRaid(e.MsgParamDisplayName, int.Parse(e.MsgParamViewerCount));
            }
        }

        private void Twitch_OnGiftSubscribe(object sender, GiftedSubscription e)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] == null) continue;
                managers[i].NewGiftSub(e.DisplayName);
            }
        }

        private void Twitch_OnReSubscribe(object sender, ReSubscriber e)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] == null) continue;
                managers[i].NewReSub(e.DisplayName, e.ResubMessage);
            }
        }

        private void Twitch_OnNewSubscribe(object sender, Subscriber e)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] == null) continue;
                managers[i].NewSub(e.DisplayName);
            }
        }

        private void Twitch_OnBits(object sender, ChatMessage e)
        {
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] == null) continue;
                managers[i].NewBits(e.Username, e.Message, e.Bits);
            }
        }


        // ------------------ Commands for window state ------------------//

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void mainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(8);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifications.Stop();
            SaveSettings();
        }
    }
}
