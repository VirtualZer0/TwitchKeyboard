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
using TwitchKeyboard.Classes;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Components.RulePreviews;
using TwitchKeyboard.Enums;
using TwitchKeyboard.Windows;
using TwitchKeyboard.Windows.Editors;

namespace TwitchKeyboard.Components.RuleLists
{
    /// <summary>
    /// Логика взаимодействия для CmdRuleList.xaml
    /// </summary>
    public partial class CmdRuleList : UserControl
    {
        MainWindow mainWindow;
        List<CmdRule> rules;

        public bool isEditMode = false;

        public delegate void OnRulesChangedHandler(object sender, List<CmdRule> rules);
        public event OnRulesChangedHandler OnRulesChanged;

        public CmdRuleList()
        {
            InitializeComponent();
        }

        public void Init(MainWindow window)
        {
            mainWindow = window;
            ReloadPresets();
            ReloadRules();
        }

        public void ReloadPresets()
        {
            var keys = Helper.settings.cmdRulesPreset.Keys.ToArray();
            presetList.Items.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                presetList.Items.Add(keys[i]);
            }

            presetList.SelectedItem = Helper.settings.activePresets[ManagerType.CMD];
        }

        public void ReloadRules()
        {
            this.rules = Helper.settings.cmdRulesPreset[Helper.settings.activePresets[ManagerType.CMD]];

            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            ruleList.Children.Clear();

            for (int i = 0; i < rules.Count; i++)
            {
                CmdRulePreview cmdRule = new(this.rules[i]);
                cmdRule.OnRuleChangeClick += CmdRule_OnRuleChangeClick;
                cmdRule.OnRuleRemoveClick += CmdRule_OnRuleRemoveClick;
                cmdRule.OnRuleDuplicateClick += CmdRule_OnRuleDuplicateClick;

                ruleList.Children.Add(cmdRule);
            }

            ruleList.Children.Add(addButton);
        }

        public void ChangeRule(CmdRule oldRule, CmdRule newRule)
        {
            int index = rules.IndexOf(oldRule);
            rules[index] = newRule;
            ((CmdRulePreview)ruleList.Children[index]).UpdateRule(newRule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void AddRule(CmdRule rule)
        {
            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            CmdRulePreview cmdRule = new(rule);
            cmdRule.OnRuleChangeClick += CmdRule_OnRuleChangeClick;
            cmdRule.OnRuleRemoveClick += CmdRule_OnRuleRemoveClick;
            cmdRule.OnRuleDuplicateClick += CmdRule_OnRuleDuplicateClick;

            ruleList.Children.Add(cmdRule);
            ruleList.Children.Add(addButton);

            rules.Add(rule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void RemoveRule(CmdRule rule)
        {
            if (rules.Contains(rule))
            {
                int index = rules.IndexOf(rule);
                ruleList.Children.RemoveAt(index);
                rules.RemoveAt(index);
                OnRulesChanged?.Invoke(this, rules);
            }
        }

        private void CmdRule_OnRuleChangeClick(object sender, CmdRule rule)
        {
            CmdRuleEditor ruleEditor = new(rule);
            ruleEditor.OnSaveRule += (object sender, CmdRule newRule) => {
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

        private void CmdRule_OnRuleRemoveClick(object sender, CmdRule rule)
        {
            RemoveRule(rule);
        }

        private void CmdRule_OnRuleDuplicateClick(object sender, CmdRule rule)
        {
            AddRule(
                JsonConvert.DeserializeObject<CmdRule>(
                    JsonConvert.SerializeObject(rule)
                )
            );
        }

        private void addNewRule_Click(object sender, RoutedEventArgs e)
        {
            CmdRuleEditor ruleEditor = new();
            ruleEditor.OnSaveRule += (object sender, CmdRule newRule) => {
                AddRule(newRule);
            };
            ruleEditor.ShowDialog();
        }

        private void presetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainWindow == null || presetList.Items.Count == 0) return;

            mainWindow.SwitchPreset<CmdRule, CmdRuleController>(
                presetList.SelectedItem.ToString(), Enums.ManagerType.CMD, Helper.settings.cmdRulesPreset
            );

            ReloadRules();
        }

        private void cmdRulesRenamePresetButton_Click(object sender, RoutedEventArgs e)
        {
            this.isEditMode = true;
            this.presetEditName.Text = Helper.settings.activePresets[Enums.ManagerType.CMD];
            this.presetEditTitle.Text = Properties.Resources.t_renamePreset;
            this.presetEditCard.Visibility = Visibility.Visible;
        }

        private void cmdRulesDeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.RemovePreset<CmdRule, CmdRuleController>(
                Helper.settings.activePresets[ManagerType.CMD], ManagerType.CMD, Helper.settings.cmdRulesPreset
            );
            this.ReloadPresets();
        }

        private void cmdRulesDuplicatePresetButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.DuplicatePreset<CmdRule, CmdRuleController>(
                ManagerType.CMD, Helper.settings.cmdRulesPreset
            );
            this.ReloadPresets();
        }

        private void cmdRulesAddPresetButton_Click(object sender, RoutedEventArgs e)
        {
            this.isEditMode = false;
            this.presetEditName.Text = $"{Properties.Resources.t_preset} {Helper.settings.cmdRulesPreset.Count}";
            this.presetEditTitle.Text = Properties.Resources.t_createPreset;
            this.presetEditCard.Visibility = Visibility.Visible;
        }

        private void presetEditSave_Click(object sender, RoutedEventArgs e)
        {
            this.presetEditCard.Visibility = Visibility.Collapsed;

            if (isEditMode)
            {
                mainWindow.RenamePreset<CmdRule, CmdRuleController>(
                    presetEditName.Text, ManagerType.CMD, Helper.settings.cmdRulesPreset
                );

            }
            else
            {
                mainWindow.CreatePreset<CmdRule, CmdRuleController>(
                    presetEditName.Text, ManagerType.CMD, Helper.settings.cmdRulesPreset
                );
            }

            this.ReloadPresets();
        }

        private void presetEditCancel_Click(object sender, RoutedEventArgs e)
        {
            this.presetEditCard.Visibility = Visibility.Collapsed;
        }
    }
}
