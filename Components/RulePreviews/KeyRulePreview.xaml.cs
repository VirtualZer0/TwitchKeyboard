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
using TwitchKeyboard.Classes.Rules;

namespace TwitchKeyboard.Components.RulePreviews
{
    /// <summary>
    /// Логика взаимодействия для KeyRulePreview.xaml
    /// </summary>
    public partial class KeyRulePreview : UserControl
    {
        KeyRule rule;

        public delegate void OnRuleRemoveClickHandler(object sender, KeyRule rule);
        public event OnRuleRemoveClickHandler OnRuleRemoveClick;

        public delegate void OnRuleChangeClickHandler(object sender, KeyRule rule);
        public event OnRuleChangeClickHandler OnRuleChangeClick;

        public KeyRulePreview(KeyRule rule)
        {
            InitializeComponent();
            UpdateRule(rule);
        }

        public void UpdateRule(KeyRule rule)
        {
            this.rule = rule;
            SetKeyRuleValues();
        }

        public void SetKeyRuleValues ()
        {
            if (rule.keys.Count > 0)
            {
                keysText.Text = rule.GetName();
            }

            modeText.Text = $"{rule.mode}";
            eventsText.Text = $"{rule.events.Count} {Properties.Resources.t_events}";
        }

        private void removeRuleButton_Click(object sender, RoutedEventArgs e)
        {
            OnRuleRemoveClick?.Invoke(this, rule);
        }

        private void editRuleButton_Click(object sender, RoutedEventArgs e)
        {
            OnRuleChangeClick?.Invoke(this, rule);
        }
    }
}
