using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using TwitchGQL;
using TwitchKeyboard.Classes;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using WindowsInput;
using WindowsInput.Native;

namespace TwitchKeyboard
{
    /// <summary>
    /// Most used brushes
    /// </summary>
    public static class Brushes {
        public static SolidColorBrush bFg = new SolidColorBrush(Color.FromRgb(250, 250, 250));

        public static SolidColorBrush bGray = new SolidColorBrush(Color.FromRgb(158, 158, 158));
        public static SolidColorBrush bGrayO = new SolidColorBrush(Color.FromArgb(63, 158, 158, 158));

        public static SolidColorBrush bRed = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        public static SolidColorBrush bRedO = new SolidColorBrush(Color.FromArgb(63, 244, 67, 54));

        public static SolidColorBrush bYellow = new SolidColorBrush(Color.FromRgb(255, 235, 59));
        public static SolidColorBrush bYellowO = new SolidColorBrush(Color.FromArgb(63, 255, 235, 59));

        public static SolidColorBrush bGreen = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        public static SolidColorBrush bGreenO = new SolidColorBrush(Color.FromArgb(63, 76, 175, 80));

        public static SolidColorBrush bBlue = new SolidColorBrush(Color.FromRgb(3, 169, 244));
        public static SolidColorBrush bBlueO = new SolidColorBrush(Color.FromArgb(63, 3, 169, 244));

        public static SolidColorBrush bPurple = new SolidColorBrush(Color.FromRgb(103, 58, 183));
        public static SolidColorBrush bPurpleO = new SolidColorBrush(Color.FromArgb(63, 103, 58, 183));
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Twitch controller, controls chat connection and Twitch API requests
        /// </summary>
        TwitchController twitch = new TwitchController();

        /// <summary>
        /// Virtual keyboard, controls simulated keys
        /// </summary>
        VirtualKeyboard keyboard = new VirtualKeyboard();

        /// <summary>
        /// Current user settings
        /// </summary>
        UserSettings settings = new UserSettings();

        /// <summary>
        /// Currently edited rule id. If null, will be saved as a new rule
        /// </summary>
        public int? editRuleId = null;

        /// <summary>
        /// Currently edited rule object
        /// </summary>
        public KeyRule editRule = new KeyRule { 
            cooldown = 0,
            duration = 0,
            key = null,
            message = "",
            rewardId = null,
            trigger = TwitchEvent.MESSAGE
        };

        /// <summary>
        /// If true, next KeyDown event will be catched and writed to currently edited rule
        /// </summary>
        public bool keyBindMode = false;

        /// <summary>
        /// If false, clear event log before writing a new entry
        /// </summary>
        public bool logListCleared = false;

        public MainWindow()
        {
            InitializeComponent();

            // Load settings from file
            this.LoadSettings();

            // Init rule editor UI
            this.constructNewRuleUI();

            // Bind events
            twitch.OnConnectionStateChanged += Twitch_OnConnectionStateChanged;
            twitch.OnMessage += Twitch_OnMessage;
            twitch.OnReward += Twitch_OnReward;

            keyboard.OnRuleUsed += Keyboard_OnRuleUsed;

            // Launch keyboard timer
            this.keyboard.init();
        }

        /// <summary>
        /// Raised when key successfully pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="user">Twitch username</param>
        /// <param name="key">Key code</param>
        private void Keyboard_OnRuleUsed(object sender, string user, Key key)
        {
            if (!logListCleared)
            {
                logListCleared = true;
                eventLogList.Items.Clear();
            }

            eventLogList.Dispatcher.Invoke(() =>
            {
                eventLogList.Items.Add($"User {user} press {key}");
                if (eventLogList.Items.Count > 100)
                {
                    eventLogList.Items.RemoveAt(0);
                }
            });
        }

        /// <summary>
        /// Raised when user activates a reward
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="rewardId">Reward id</param>
        /// <param name="e">Twitch message</param>
        private void Twitch_OnReward(object sender, string rewardId, ChatMessage e)
        {
            this.Dispatcher.Invoke(() =>
            {
                keyboard.newReward(e.Username, rewardId, e.Message);
            });
        }

        /// <summary>
        /// Raised when user sends a basic message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Twitch message</param>
        private void Twitch_OnMessage(object sender, ChatMessage e)
        {
            this.Dispatcher.Invoke(() =>
            {
                keyboard.newMessage(e.Username, e.Message);
            });
        }

        /// <summary>
        /// Save current settings to JSON
        /// </summary>
        public void SaveSettings()
        {
            this.settings.channel = this.channelName.Text;
            this.settings.rewardsCache = this.twitch.customRewards;
            this.settings.rules = this.keyboard.rules;

            File.WriteAllText("./settings.json", JsonConvert.SerializeObject(settings));
        }

        /// <summary>
        /// Load current settings from file or create empty savefile
        /// </summary>
        public void LoadSettings ()
        {
            if (File.Exists("./settings.json"))
            {
                this.settings = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText("./settings.json"));
                channelName.Text = settings.channel;
                twitch.customRewards = settings.rewardsCache;
                keyboard.rules = settings.rules;

                this.constructRulesLists();
            }
            else
            {
                File.WriteAllText("./settings.json", JsonConvert.SerializeObject(settings));
            }
        }

        /// <summary>
        /// Raised when Twitch connection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">New connection state</param>
        private void Twitch_OnConnectionStateChanged(object sender, TwitchConnectionState e)
        {
            connectionStatus.Dispatcher.Invoke(() =>
            {
                switch (e)
                {
                    case TwitchConnectionState.DISCONNECTED:
                        connectionStatus.Content = "Disconnected";
                        connectionStatus.Background = Brushes.bGrayO;
                        connectionStatus.IconBackground = Brushes.bGray;
                        ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.MessageBulletedOff;
                        connectButton.Content = "Connect";
                        connectButton.IsEnabled = true;
                        break;

                    case TwitchConnectionState.IN_PROGRESS:
                        connectionStatus.Content = "Connecting...";
                        connectionStatus.Background = Brushes.bYellowO;
                        connectionStatus.IconBackground = Brushes.bYellow;
                        ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.ContactlessPayment;
                        connectButton.Content = "Wait...";
                        connectButton.IsEnabled = false;
                        break;

                    case TwitchConnectionState.CONNECTED:
                        connectionStatus.Content = "Joining...";
                        connectionStatus.Background = Brushes.bBlueO;
                        connectionStatus.IconBackground = Brushes.bBlue;
                        ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.AccountArrowRight;
                        connectButton.Content = "Wait...";
                        connectButton.IsEnabled = false;
                        break;

                    case TwitchConnectionState.JOINED:
                        connectionStatus.Content = "Connected";
                        connectionStatus.Background = Brushes.bGreenO;
                        connectionStatus.IconBackground = Brushes.bGreen;
                        ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.MessageBulleted;
                        constructNewRuleUI();
                        SaveSettings();
                        connectButton.Content = "Disconnect";
                        connectButton.IsEnabled = true;
                        break;

                    case TwitchConnectionState.ERROR:
                        connectionStatus.Content = "Error";
                        connectionStatus.Background = Brushes.bRedO;
                        connectionStatus.IconBackground = Brushes.bRed;
                        ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.AlertCircle;
                        connectButton.Content = "Connect";
                        connectButton.IsEnabled = true;
                        break;
                }
            });
        }

        /// <summary>
        /// Create rule editor based on current rule
        /// </summary>
        public void constructNewRuleUI ()
        {
            if (editRuleId == null)
            {
                newRuleTitle.Text = $"Add new rule";
            }
            else
            {
                newRuleTitle.Text = $"Edit rule #{editRuleId + 1}";
            }

            newRuleDuration.Text = (editRule.duration/1000.0).ToString("F1", new CultureInfo("en-US").NumberFormat);
            newRuleEvent.SelectedIndex = (int)editRule.trigger;
            newRuleCooldown.Text = (editRule.cooldown / 1000.0).ToString("F1", new CultureInfo("en-US").NumberFormat);
            newRuleKeyCode.Content = editRule.key == null ? "[select key]" : editRule.key.ToString();
            newRuleMessage.Text = editRule.message;
            newRuleSelectEvent.Visibility = editRule.trigger == TwitchEvent.REWARD ? Visibility.Visible : Visibility.Collapsed;

            if (keyBindMode)
            {
                newRuleKeyCode.Content = "[press key]";
                newRuleKeyCode.Background = Brushes.bGreen;
                newRuleKeyCode.BorderBrush = Brushes.bGreen;
            }
            else
            {
                newRuleKeyCode.Background = Brushes.bPurple;
                newRuleKeyCode.BorderBrush = Brushes.bPurple;

                if (editRule.key == null)
                {
                    newRuleKeyCode.Content = "[select key]";
                }
                else
                {
                    newRuleKeyCode.Content = KeyInterop.KeyFromVirtualKey((int)editRule.key).ToString();
                }
            }

            if (editRule.rewardId != null)
            {
                CustomReward selected = this.twitch.customRewards.Find(reward =>
                {
                    return reward.id == editRule.rewardId;
                });

                newRuleSelectEvent.Content = selected?.title ?? "[unknown reward]";
            }
            else
            {
                newRuleSelectEvent.Content = "[select reward]";
            }
        }

        /// <summary>
        /// Create UI-lists with rules
        /// </summary>
        public void constructRulesLists ()
        {
            allRulesList.Children.Clear();

            if (keyboard.rules.Count > 0)
            {
                keyBindingsEmpty.Visibility = Visibility.Collapsed;
            }
            else
            {
                keyBindingsEmpty.Visibility = Visibility.Visible;
            }

            if (keyBindings.Children.Count > 1)
            {
                keyBindings.Children.RemoveRange(1, keyBindings.Children.Count - 1);
            }

            for (int i = 0; i < keyboard.rules.Count; i++)
            {
                var rule = keyboard.rules[i];

                StackPanel container = new StackPanel();
                container.Orientation = Orientation.Horizontal;
                container.Margin = new Thickness(0, 0, 0, 12);

                TextBlock ruleText = new TextBlock();
                ruleText.TextWrapping = TextWrapping.Wrap;
                ruleText.MaxWidth = mainWindow.Width / 100 * 65;
                ruleText.VerticalAlignment = VerticalAlignment.Center;
                ruleText.Text = $"#{i+1} Press {KeyInterop.KeyFromVirtualKey((int)rule.key)} for {rule.duration / 1000.0}s when receiving a {rule.trigger} ";
                ruleText.Text += $"{(rule.trigger == TwitchEvent.REWARD ? twitch.customRewards.Find(reward => reward.id == rule.rewardId).title : "") ?? "unknown"} ";
                ruleText.Text += $"with text {rule.message} and cooldown {rule.cooldown / 1000.0}s";

                ruleText.Margin = new Thickness(0, 0, 16, 0);

                Button editButton = new Button();
                editButton.Content = "Edit";
                editButton.Margin = new Thickness(0, 0, 8, 0);
                editButton.VerticalAlignment = VerticalAlignment.Center;
                editButton.Click += (object sender, RoutedEventArgs e) =>
                {
                    editRuleId = keyboard.rules.IndexOf(rule);
                    editRule = rule;
                    constructNewRuleUI();
                };

                Button removeButton = new Button();
                removeButton.Content = "Remove";
                removeButton.VerticalAlignment = VerticalAlignment.Center;
                removeButton.Click += (object sender, RoutedEventArgs e) =>
                {
                    keyboard.rules.RemoveAt(keyboard.rules.IndexOf(rule));
                    this.SaveSettings();
                    constructRulesLists();
                    if (editRuleId != null)
                    {
                        resetEditRuleState();
                    }
                };

                container.Children.Add(ruleText);
                container.Children.Add(editButton);
                container.Children.Add(removeButton);

                allRulesList.Children.Add(container);

                Chip chip = new Chip();
                chip.Content = KeyInterop.KeyFromVirtualKey((int)rule.key).ToString();
                chip.Margin = new Thickness(6);

                keyBindings.Children.Add(chip);
            }
        }

        /// <summary>
        /// Detect rule trigger type and add additional elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newRuleEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (newRuleSelectEvent == null) return;
            editRule.trigger = (TwitchEvent)newRuleEvent.SelectedIndex;
            newRuleSelectEvent.Visibility = editRule.trigger == TwitchEvent.REWARD ? Visibility.Visible : Visibility.Collapsed;
            newRuleSelectEvent.Content = editRule.rewardId == null ? "[select reward]" : "";
        }

        /// <summary>
        /// Connect/disconnect Twitch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            connectButton.Content = "Wait...";
            connectButton.IsEnabled = false;

            if (twitch.connectionState == TwitchConnectionState.DISCONNECTED || twitch.connectionState == TwitchConnectionState.ERROR)
            {
                await this.twitch.JoinChannel(channelName.Text);
            }
            else
            {
                this.twitch.Disconnect();
            }
        }

        /// <summary>
        /// Open reward selection window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newRuleSelectEvent_Click(object sender, RoutedEventArgs e)
        {
            RewardsScreen s = new RewardsScreen(this.twitch.customRewards);
            s.OnRewardSelected += (object sender, CustomReward reward) =>
            {
                this.editRule.rewardId = reward.id;
                this.constructNewRuleUI();
            };
            s.ShowDialog();
        }

        /// <summary>
        /// Catch pressed key if user clicks on [select key] button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyBindMode)
            {
                VirtualKeyCode codeOfKey = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(e.Key);
                this.editRule.key = codeOfKey;
                keyBindMode = false;
                constructNewRuleUI();
            }
        }

        /// <summary>
        /// Enable key catching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newRuleKeyCode_Click(object sender, RoutedEventArgs e)
        {
            this.keyBindMode = true;
            constructNewRuleUI();
        }

        /// <summary>
        /// Saves currently edited rule (or creates a new one)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveEditRule_Click(object sender, RoutedEventArgs e)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            editRule.duration = (int)(float.Parse(newRuleDuration.Text, CultureInfo.InvariantCulture.NumberFormat) * 1000);
            editRule.cooldown = (int)(float.Parse(newRuleCooldown.Text, CultureInfo.InvariantCulture.NumberFormat) * 1000);
            editRule.message = newRuleMessage.Text;

            if (this.editRuleId != null)
            {
                this.keyboard.rules[(int)editRuleId] = editRule;
            }
            else
            {
                this.keyboard.rules.Add(editRule);
            }

            this.resetEditRuleState();
            this.constructRulesLists();
            this.SaveSettings();
        }

        /// <summary>
        /// Declines rule editing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelEditRule_Click(object sender, RoutedEventArgs e)
        {
            this.resetEditRuleState();
        }

        /// <summary>
        /// Reset state for rules editor
        /// </summary>
        private void resetEditRuleState()
        {
            this.editRuleId = null;
            this.editRule = new KeyRule
            {
                cooldown = 0,
                duration = 0,
                key = null,
                message = "",
                rewardId = null,
                trigger = TwitchEvent.MESSAGE
            };

            constructNewRuleUI();
        }

        /// <summary>
        /// Open settings tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setKeysButton_Click(object sender, RoutedEventArgs e)
        {
            mainTab.SelectedIndex = 1;
        }

        /// <summary>
        /// Enable/disable virtual keyboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void keyboardToggle_Checked(object sender, RoutedEventArgs e)
        {
            this.keyboard.enabled = (bool)((ToggleButton)sender).IsChecked;
        }
    }
}
