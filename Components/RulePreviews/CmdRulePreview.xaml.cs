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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwitchKeyboard.Classes.Rules;

namespace TwitchKeyboard.Components.RulePreviews
{
    /// <summary>
    /// Логика взаимодействия для CmdRulePreview.xaml
    /// </summary>
    public partial class CmdRulePreview : UserControl
    {
        CmdRule rule;

        public delegate void OnRuleRemoveClickHandler(object sender, CmdRule rule);
        public event OnRuleRemoveClickHandler OnRuleRemoveClick;

        public delegate void OnRuleChangeClickHandler(object sender, CmdRule rule);
        public event OnRuleChangeClickHandler OnRuleChangeClick;

        public delegate void OnRuleDuplicateClickHandler(object sender, CmdRule rule);
        public event OnRuleDuplicateClickHandler OnRuleDuplicateClick;

        public CmdRulePreview(CmdRule rule)
        {
            InitializeComponent();
            UpdateRule(rule);
        }

        public void UpdateRule(CmdRule rule)
        {
            this.rule = rule;
            SetCmdRuleValues();
        }

        public void SetCmdRuleValues()
        {
            sfxText.Text = rule.GetName();

            modeText.Text = rule.openFile ? Properties.Resources.t_file : Properties.Resources.t_command;
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

        private void duplicateRuleButton_Click(object sender, RoutedEventArgs e)
        {
            OnRuleDuplicateClick?.Invoke(this, rule);
        }
    }
}
