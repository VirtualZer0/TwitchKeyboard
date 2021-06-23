using Classes.APIModels.TwitchGQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwitchKeyboard.Classes;
using TwitchKeyboard.Enums;
using TwitchKeyboard.Windows;

namespace TwitchKeyboard.Components
{
    /// <summary>
    /// Логика взаимодействия для EventEditor.xaml
    /// </summary>
    public partial class TriggerEditor : UserControl
    {
        readonly TwitchTrigger trigger;

        public delegate void OnDuplicateHandler(object sender, TwitchTrigger triggerCopy);
        public event OnDuplicateHandler OnDuplicate;

        public delegate void OnRemoveHandler(object sender, TwitchTrigger removedTrigger);
        public event OnRemoveHandler OnRemove;

        public TriggerEditor(TwitchTrigger trigger = null)
        {
            // Create new trigger or set an existing
            if (trigger == null)
            {
                trigger = new();
            }

            this.trigger = trigger;

            InitializeComponent();

            // Load trigger parameters
            triggerType.SelectedIndex = (int)trigger.type;
            triggerSelectReward.Visibility = trigger.type != Enums.TwitchEvent.REWARD ? Visibility.Collapsed : Visibility.Visible;
            triggerAmount.Visibility = trigger.type != Enums.TwitchEvent.BITS ? Visibility.Collapsed : Visibility.Visible;

            triggerRepeated.IsChecked = trigger.repeated;
            triggerRepeatedBlock.IsEnabled = trigger.repeated;
            triggerRepeatedTimes.Text = trigger.repeatTimes.ToString();
            triggerRepeatedDuration.Text = Helper.TimerIntToString(trigger.repeatDuration);
            triggerRepeatedUnqiue.IsChecked = trigger.repeatUniqueUsers;
            triggerRepeatedReset.IsChecked = trigger.repeatResetTime;

            triggerTextCompMode.SelectedIndex = (int)trigger.comparisonMode;
            triggerText.Text = trigger.text;
            triggerCaseSensitive.IsChecked = trigger.caseSensitive;

            triggerMinAmount.Text = trigger.amountFrom.ToString();
            triggerMaxAmount.Text = trigger.amountTo.ToString();

            var reward = Helper.settings.rewardsCache.FirstOrDefault(reward =>
            {
                return reward.id == trigger.rewardId;
            });

            triggerSelectReward.Content = trigger.rewardId == null || reward == null ?
                Properties.Resources.t_selectAction : reward.title;
        }

        private void duplicateTrigger_Click(object sender, RoutedEventArgs e)
        {
            SaveTrigger();
            this.OnDuplicate?.Invoke(this, trigger.Copy());
        }

        private void removeTrigger_Click(object sender, RoutedEventArgs e)
        {
            this.OnRemove?.Invoke(this, trigger);
        }

        private void triggerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (triggerSelectReward == null) return;
            triggerSelectReward.Visibility = triggerType.SelectedIndex != (int)Enums.TwitchEvent.REWARD ? Visibility.Collapsed : Visibility.Visible;
            triggerAmount.Visibility = 
                triggerType.SelectedIndex != (int)Enums.TwitchEvent.BITS && triggerType.SelectedIndex != (int)Enums.TwitchEvent.RAID ?
                Visibility.Collapsed : Visibility.Visible;
            triggerTextPanel.IsEnabled = triggerType.SelectedIndex <= (int)TwitchEvent.RESUB;
        }

        private void triggerSelectReward_Click(object sender, RoutedEventArgs e)
        {
            RewardsScreen rewardsDialog = new();
            rewardsDialog.OnRewardSelected += (object sender, CustomReward reward) =>
            {
                trigger.rewardId = reward.id;
                triggerSelectReward.Content = reward.title;
            };
            rewardsDialog.ShowDialog();
        }

        private void triggerRepeated_Click(object sender, RoutedEventArgs e)
        {
            triggerRepeatedBlock.IsEnabled = (bool)triggerRepeated.IsChecked;
        }

        public void SaveTrigger()
        {
            trigger.type = (TwitchEvent)triggerType.SelectedIndex;
            trigger.repeated = (bool)triggerRepeated.IsChecked;
            trigger.repeatTimes = int.Parse(triggerRepeatedTimes.Text);
            trigger.repeatDuration = Helper.StringToTimerInt(triggerRepeatedDuration.Text);
            trigger.repeatUniqueUsers = (bool)triggerRepeatedUnqiue.IsChecked;
            trigger.repeatResetTime = (bool)triggerRepeatedReset.IsChecked;
            trigger.comparisonMode = (TextCompareMode)triggerTextCompMode.SelectedIndex;
            trigger.text = triggerText.Text;
            trigger.caseSensitive = (bool)triggerCaseSensitive.IsChecked;
            trigger.amountFrom = int.Parse(triggerMinAmount.Text);
            trigger.amountTo = int.Parse(triggerMaxAmount.Text);
        }
    }
}
