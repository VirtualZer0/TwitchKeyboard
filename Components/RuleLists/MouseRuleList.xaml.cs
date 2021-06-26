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
using TwitchKeyboard.Classes;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Components.RulePreviews;
using TwitchKeyboard.Enums;
using TwitchKeyboard.Windows;
using TwitchKeyboard.Windows.Editors;
using Newtonsoft.Json;

namespace TwitchKeyboard.Components.RuleLists
{
    /// <summary>
    /// Логика взаимодействия для MouseRuleList.xaml
    /// </summary>
    public partial class MouseRuleList : UserControl, IRuleList
    {
        MainWindow mainWindow;
        List<MouseRule> rules;

        public bool isEditMode = false;

        public delegate void OnRulesChangedHandler(object sender, List<MouseRule> rules);
        public event OnRulesChangedHandler OnRulesChanged;

        public MouseRuleList()
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
            var keys = Helper.settings.mouseRulesPreset.Keys.ToArray();
            presetList.Items.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                presetList.Items.Add(keys[i]);
            }

            presetList.SelectedItem = Helper.settings.activePresets[Enums.ManagerType.KEYBOARD];
        }

        public void ReloadRules()
        {
            this.rules = Helper.settings.mouseRulesPreset[Helper.settings.activePresets[Enums.ManagerType.MOUSE]];

            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            ruleList.Children.Clear();

            for (int i = 0; i < rules.Count; i++)
            {
                MouseRulePreview mouseRule = new(this.rules[i]);
                mouseRule.OnRuleChangeClick += MouseRule_OnRuleChangeClick;
                mouseRule.OnRuleRemoveClick += MouseRule_OnRuleRemoveClick;
                mouseRule.OnRuleDuplicateClick += MouseRule_OnRuleDuplicateClick;

                ruleList.Children.Add(mouseRule);
            }

            ruleList.Children.Add(addButton);
        }

        public void ChangeRule(MouseRule oldRule, MouseRule newRule)
        {
            int index = rules.IndexOf(oldRule);
            rules[index] = newRule;
            ((MouseRulePreview)ruleList.Children[index]).UpdateRule(newRule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void AddRule(MouseRule rule)
        {
            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            MouseRulePreview mouseRule = new(rule);
            mouseRule.OnRuleChangeClick += MouseRule_OnRuleChangeClick;
            mouseRule.OnRuleRemoveClick += MouseRule_OnRuleRemoveClick;
            mouseRule.OnRuleDuplicateClick += MouseRule_OnRuleDuplicateClick;

            ruleList.Children.Add(mouseRule);
            ruleList.Children.Add(addButton);

            rules.Add(rule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void RemoveRule(MouseRule rule)
        {
            if (rules.Contains(rule))
            {
                int index = rules.IndexOf(rule);
                ruleList.Children.RemoveAt(index);
                rules.RemoveAt(index);
                OnRulesChanged?.Invoke(this, rules);
            }
        }

        private void MouseRule_OnRuleChangeClick(object sender, MouseRule rule)
        {
            MouseRuleEditor ruleEditor = new(rule);
            ruleEditor.OnSaveRule += (object sender, MouseRule newRule) => {
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

        private void MouseRule_OnRuleRemoveClick(object sender, MouseRule rule)
        {
            RemoveRule(rule);
        }

        private void MouseRule_OnRuleDuplicateClick(object sender, MouseRule rule)
        {
            AddRule(
                JsonConvert.DeserializeObject<MouseRule>(
                    JsonConvert.SerializeObject(rule)
                )
            );
        }

        private void addNewRule_Click(object sender, RoutedEventArgs e)
        {
            MouseRuleEditor ruleEditor = new();
            ruleEditor.OnSaveRule += (object sender, MouseRule newRule) => {
                AddRule(newRule);
            };
            ruleEditor.ShowDialog();
        }

        private void presetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainWindow == null || presetList.Items.Count == 0) return;

            mainWindow.SwitchPreset<MouseRule, MouseRuleController>(
                presetList.SelectedItem.ToString(), Enums.ManagerType.MOUSE, Helper.settings.mouseRulesPreset
            );

            ReloadRules();
        }

        private void mouseRulesRenamePresetButton_Click(object sender, RoutedEventArgs e)
        {
            this.isEditMode = true;
            this.presetEditName.Text = Helper.settings.activePresets[Enums.ManagerType.KEYBOARD];
            this.presetEditTitle.Text = Properties.Resources.t_renamePreset;
            this.presetEditCard.Visibility = Visibility.Visible;
        }

        private void mouseRulesDeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.RemovePreset<MouseRule, MouseRuleController>(
                    Helper.settings.activePresets[ManagerType.KEYBOARD], ManagerType.KEYBOARD, Helper.settings.mouseRulesPreset
                );
            this.ReloadPresets();
        }

        private void mouseRulesDuplicatePresetButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.DuplicatePreset<MouseRule, MouseRuleController>(
                ManagerType.MOUSE, Helper.settings.mouseRulesPreset
            );
            this.ReloadPresets();
        }

        private void mouseRulesAddPresetButton_Click(object sender, RoutedEventArgs e)
        {
            this.isEditMode = false;
            this.presetEditName.Text = $"{Properties.Resources.t_preset} {Helper.settings.keyRulesPreset.Count}";
            this.presetEditTitle.Text = Properties.Resources.t_createPreset;
            this.presetEditCard.Visibility = Visibility.Visible;
        }

        private void presetEditSave_Click(object sender, RoutedEventArgs e)
        {
            this.presetEditCard.Visibility = Visibility.Collapsed;

            if (isEditMode)
            {
                mainWindow.RenamePreset<MouseRule, MouseRuleController>(
                    presetEditName.Text, ManagerType.KEYBOARD, Helper.settings.mouseRulesPreset
                );

            }
            else
            {
                mainWindow.CreatePreset<MouseRule, MouseRuleController>(
                    presetEditName.Text, ManagerType.KEYBOARD, Helper.settings.mouseRulesPreset
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
