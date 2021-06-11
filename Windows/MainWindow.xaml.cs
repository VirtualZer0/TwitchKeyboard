using MaterialDesignThemes.Wpf;
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
        /// All managers
        /// </summary>
        readonly BaseRuleManager[] managers = new BaseRuleManager[(int)ManagerType.MANAGERS_COUNT];

        // Current rules
        List<KeyRule> keyRules;
        List<MouseRule> mouseRules;

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

        /// <summary>
        /// If false, clear event log before writing a new entry
        /// </summary>
        public bool logListCleared = false;

        public MainWindow()
        {
            InitializeComponent();

            // Load settings from file
            this.LoadSettings();

            // Load current presets
            keyRules = settings.keyRulesPreset[settings.activePresets[ManagerType.KEYBOARD]].ToList();
            mouseRules = settings.mouseRulesPreset[settings.activePresets[ManagerType.MOUSE]].ToList();

            // Create managers
            managers[(int)ManagerType.KEYBOARD] = new KeyRuleManager();

            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] != null)
                {
                    managers[i].Enable();
                }
            }
            

            // Load rules into managers
            managers[(int)ManagerType.KEYBOARD].CreateControllers<KeyRuleController>(keyRules.ToArray());

            // Bind events
            twitch.OnConnectionStateChanged += Twitch_OnConnectionStateChanged;
            twitch.OnMessage += Twitch_OnMessage;
            twitch.OnReward += Twitch_OnReward;

            // Launch keyboard/mouse timer
            inputOperationsTimer = new(UpdateInputManagers, null, 0, 30);

            // Launch other managers timer
            otherOperationsTimer = new(UpdateOtherManagers, null, 100, 100);

            // Construct UI elements
            keyRuleList.SetRules(keyRules);

            GC.Collect();
        }

        void UpdateInputManagers (Object statenfo)
        {
            managers[(int)ManagerType.KEYBOARD].Update(30);
        }

        void UpdateOtherManagers(Object statenfo)
        {

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
                managers[i].NewReward(e.Username, rewardId, e.Message);
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

        /// <summary>
        /// Save current settings to JSON
        /// </summary>
        public void SaveSettings()
        {
            this.settings.channel = this.twitch.channel;
            this.settings.rewardsCache = this.twitch.customRewards;

            settings.keyRulesPreset[settings.activePresets[ManagerType.KEYBOARD]] = keyRules.ToArray();

            Helper.settings = settings;

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
                twitchConnect.SetChannel(settings.channel);
                twitch.customRewards = settings.rewardsCache;
            }
            else
            {
                settings = new();
                File.WriteAllText("./settings.json", JsonConvert.SerializeObject(settings));
            }

            Helper.settings = settings;
        }

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


        private async void twitchConnect_OnActionClick(object sender, string channelName)
        {
            await twitch.JoinChannel(channelName);
        }



        private void keyRuleList_OnRulesChanged(object sender, List<KeyRule> rules)
        {
            managers[(int)ManagerType.KEYBOARD].Disable();
            managers[(int)ManagerType.KEYBOARD].CreateControllers<KeyRuleController>(rules.ToArray());
            managers[(int)ManagerType.KEYBOARD].Enable();
            SaveSettings();
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
    }
}
