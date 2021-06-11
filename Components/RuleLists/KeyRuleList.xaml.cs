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
using TwitchKeyboard.Windows.Editors;

namespace TwitchKeyboard.Components.RuleLists
{
    /// <summary>
    /// Логика взаимодействия для KeyRuleList.xaml
    /// </summary>
    public partial class KeyRuleList : UserControl
    {
        List<KeyRule> rules;

        public delegate void OnRulesChangedHandler(object sender, List<KeyRule> rules);
        public event OnRulesChangedHandler OnRulesChanged;

        public KeyRuleList()
        {
            InitializeComponent();
        }

        public void SetRules(List<KeyRule> rules)
        {
            this.rules = rules;

            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            for (int i = 0; i < rules.Count; i++)
            {
                KeyRulePreview keyRule = new(rules[i]);
                keyRule.OnRuleChangeClick += KeyRule_OnRuleChangeClick;
                keyRule.OnRuleRemoveClick += KeyRule_OnRuleRemoveClick;

                ruleList.Children.Add(keyRule);
            }

            ruleList.Children.Add(addButton);
        }

        public void ChangeRule(KeyRule oldRule, KeyRule newRule)
        {
            int index = rules.IndexOf(oldRule);
            rules[index] = newRule;
            ((KeyRulePreview)ruleList.Children[index]).UpdateRule(newRule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void AddRule(KeyRule rule)
        {
            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            KeyRulePreview keyRule = new(rule);
            keyRule.OnRuleChangeClick += KeyRule_OnRuleChangeClick;
            keyRule.OnRuleRemoveClick += KeyRule_OnRuleRemoveClick;

            ruleList.Children.Add(keyRule);
            ruleList.Children.Add(addButton);

            rules.Add(rule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void RemoveRule(KeyRule rule)
        {
            if (rules.Contains(rule))
            {
                int index = rules.IndexOf(rule);
                ruleList.Children.RemoveAt(index);
                rules.RemoveAt(index);
                OnRulesChanged?.Invoke(this, rules);
            }
        }

        private void KeyRule_OnRuleChangeClick(object sender, KeyRule rule)
        {
            KeyRuleEditor ruleEditor = new(rule);
            ruleEditor.OnSaveRule += (object sender, KeyRule newRule) => {
                if (rules.Contains(rule))
                {
                    ChangeRule(rule, newRule);
                }
                else
                {
                    AddRule(newRule);
                }
            };
            ruleEditor.ShowDialog();
        }

        private void KeyRule_OnRuleRemoveClick(object sender, KeyRule rule)
        {
            RemoveRule(rule);
        }

        private void addNewRule_Click(object sender, RoutedEventArgs e)
        {
            KeyRuleEditor ruleEditor = new();
            ruleEditor.OnSaveRule += (object sender, KeyRule newRule) => {
                AddRule(newRule);
            };
            ruleEditor.ShowDialog();
        }
    }
}
