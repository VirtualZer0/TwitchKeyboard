using Classes.APIModels.TwitchGQL;
using Newtonsoft.Json;
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
using System.Windows.Shapes;
using TwitchKeyboard.Classes;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Components;
using WindowsInput.Native;

namespace TwitchKeyboard.Windows.Editors
{
    /// <summary>
    /// Логика взаимодействия для KeyRuleEditor.xaml
    /// </summary>
    public partial class KeyRuleEditor : Window
    {
        readonly KeyRule rule = null;
        bool enableKeyCatch = false;

        /// <summary>
        /// Raised when the rule is saved or created.
        /// </summary>
        /// <param name="sender">Current window</param>
        /// <param name="rule">New rule object</param>
        public delegate void OnSaveRuleHandler (object sender, KeyRule rule);
        public event OnSaveRuleHandler OnSaveRule;

        public KeyRuleEditor(KeyRule rule = null)
        {
            InitializeComponent();

            if (rule == null)
            {
                rule = this.rule = new KeyRule();
                this.rule.events.Add(new TwitchTrigger());
            }
            else
            {
                // Creating copy of current rule (not the most optimal method, but simple enough)
                this.rule = JsonConvert.DeserializeObject<KeyRule>(
                    JsonConvert.SerializeObject(rule)
                );

                titleText.Text = Properties.Resources.t_editKeyRule;
            }

            for(int i = 0; i < this.rule.events.Count; i++)
            {
                createEventUIElement(this.rule.events[i]);
            }

            for (int i = 0; i < this.rule.keys.Count; i++)
            {
                keysContainer.AddKey(KeyInterop.KeyFromVirtualKey((int)this.rule.keys[i]));
            }

            delayValue.Text = Helper.TimerIntToString(rule.delay);
            durationValue.Text = Helper.TimerIntToString(rule.duration);
            cooldownValue.Text = Helper.TimerIntToString(rule.cooldown);

            switch (rule.mode)
            {
                case Enums.KeyPressMode.Press:
                    keyModePressRadio.IsChecked = true; break;

                case Enums.KeyPressMode.Spam:
                    durationBlock.Visibility = Visibility.Visible;
                    keyModeSpamRadio.IsChecked = true; break;

                default:
                    durationBlock.Visibility = Visibility.Visible;
                    keyModeHoldRadio.IsChecked = true; break;
            }
        }

        private void createEventUIElement (TwitchTrigger trigger)
        {
            TriggerEditor editor = new(trigger);
            editor.OnRemove += Editor_OnRemove;
            editor.OnDuplicate += Editor_OnDuplicate;
            editor.Margin = new Thickness(0, 0, 0, 12);
            eventsContainer.Children.Add(editor);
        }

        private void Editor_OnDuplicate(object sender, TwitchTrigger triggerCopy)
        {
            createEventUIElement(triggerCopy);
            rule.events.Add(triggerCopy);
        }

        private void Editor_OnRemove(object sender, TwitchTrigger removedTrigger)
        {
            eventsContainer.Children.Remove((UIElement)sender);
            rule.events.Remove(removedTrigger);
        }

        private void keyRuleEditorWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!enableKeyCatch) return;
            keysContainer.AddKey(e.Key);
            switch (e.Key)
            {
                case Key.System:
                    rule.keys.Add(VirtualKeyCode.MENU);
                    break;

                default:
                    rule.keys.Add((VirtualKeyCode)KeyInterop.VirtualKeyFromKey(e.Key));
                    break;
            }
            
            enableKeyCatch = false;
            e.Handled = true;
        }

        private void keysContainer_OnAddKeyPressed(object sender)
        {
            enableKeyCatch = true;
        }

        private void keysContainer_OnKeyRemove(object sender, Key k)
        {
            keysContainer.RemoveKey(k);
            rule.keys.Remove((VirtualKeyCode)KeyInterop.VirtualKeyFromKey(k));
        }

        private void addEventButton_Click(object sender, RoutedEventArgs e)
        {
            TwitchTrigger trigger = new();
            rule.events.Add(trigger);
            createEventUIElement(trigger);
        }

        private void keyModePressRadio_Click(object sender, RoutedEventArgs e)
        {
            rule.mode = Enums.KeyPressMode.Press;
            durationBlock.Visibility = Visibility.Collapsed;
            durationValue.Text = "0";
        }

        private void keyModeSpamRadio_Click(object sender, RoutedEventArgs e)
        {
            rule.mode = Enums.KeyPressMode.Spam;
            durationBlock.Visibility = Visibility.Visible;
        }

        private void keyModeHoldRadio_Click(object sender, RoutedEventArgs e)
        {
            rule.mode = Enums.KeyPressMode.Hold;
            durationBlock.Visibility = Visibility.Visible;
        }

        private void saveRuleButton_Click(object sender, RoutedEventArgs e)
        {
            rule.delay = Helper.StringToTimerInt(delayValue.Text);
            rule.duration = Helper.StringToTimerInt(durationValue.Text);
            rule.cooldown = Helper.StringToTimerInt(cooldownValue.Text);

            for (int i = 0; i < eventsContainer.Children.Count; i++)
            {
                if (eventsContainer.Children[i] is TriggerEditor tEditor)
                {
                    tEditor.SaveTrigger();
                }
            }

            OnSaveRule?.Invoke(this, rule);
            this.Close();
        }

        private void cancelRuleButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
