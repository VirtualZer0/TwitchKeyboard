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
using TwitchKeyboard.Classes.Rules;

namespace TwitchKeyboard.Components.RulePreviews
{
    /// <summary>
    /// Логика взаимодействия для WebRulePreview.xaml
    /// </summary>
    public partial class WebRulePreview : UserControl
    {
        WebRule rule;

        public delegate void OnRuleRemoveClickHandler(object sender, WebRule rule);
        public event OnRuleRemoveClickHandler OnRuleRemoveClick;

        public delegate void OnRuleChangeClickHandler(object sender, WebRule rule);
        public event OnRuleChangeClickHandler OnRuleChangeClick;

        public WebRulePreview(WebRule rule)
        {
            InitializeComponent();
            UpdateRule(rule);
        }

        public void UpdateRule(WebRule rule)
        {
            this.rule = rule;
            SetWebRuleValues();
        }

        public void SetWebRuleValues()
        {
            sfxText.Text = rule.GetName();

            modeText.Text = $"{rule.method}";
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
