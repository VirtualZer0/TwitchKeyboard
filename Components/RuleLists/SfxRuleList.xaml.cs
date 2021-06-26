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
    /// Логика взаимодействия для SfxRuleList.xaml
    /// </summary>
    public partial class SfxRuleList : UserControl
    {
        MainWindow mainWindow;
        List<SfxRule> rules;

        public bool isEditMode = false;

        public delegate void OnRulesChangedHandler(object sender, List<SfxRule> rules);
        public event OnRulesChangedHandler OnRulesChanged;

        public SfxRuleList()
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
            var keys = Helper.settings.sfxRulesPreset.Keys.ToArray();
            presetList.Items.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                presetList.Items.Add(keys[i]);
            }

            presetList.SelectedItem = Helper.settings.activePresets[Enums.ManagerType.SFX];
        }

        public void ReloadRules()
        {
            this.rules = Helper.settings.sfxRulesPreset[Helper.settings.activePresets[Enums.ManagerType.SFX]];

            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            ruleList.Children.Clear();

            for (int i = 0; i < rules.Count; i++)
            {
                SfxRulePreview sfxRule = new(this.rules[i]);
                sfxRule.OnRuleChangeClick += SfxRule_OnRuleChangeClick;
                sfxRule.OnRuleRemoveClick += SfxRule_OnRuleRemoveClick;
                sfxRule.OnRuleDuplicateClick += SfxRule_OnRuleDuplicateClick;

                ruleList.Children.Add(sfxRule);
            }

            ruleList.Children.Add(addButton);
        }

        public void ChangeRule(SfxRule oldRule, SfxRule newRule)
        {
            int index = rules.IndexOf(oldRule);
            rules[index] = newRule;
            ((SfxRulePreview)ruleList.Children[index]).UpdateRule(newRule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void AddRule(SfxRule rule)
        {
            var addButton = ruleList.Children[^1];
            ruleList.Children.Remove(addButton);

            SfxRulePreview sfxRule = new(rule);
            sfxRule.OnRuleChangeClick += SfxRule_OnRuleChangeClick;
            sfxRule.OnRuleRemoveClick += SfxRule_OnRuleRemoveClick;
            sfxRule.OnRuleDuplicateClick += SfxRule_OnRuleDuplicateClick;

            ruleList.Children.Add(sfxRule);
            ruleList.Children.Add(addButton);

            rules.Add(rule);
            OnRulesChanged?.Invoke(this, rules);
        }

        public void RemoveRule(SfxRule rule)
        {
            if (rules.Contains(rule))
            {
                int index = rules.IndexOf(rule);
                ruleList.Children.RemoveAt(index);
                rules.RemoveAt(index);
                OnRulesChanged?.Invoke(this, rules);
            }
        }

        private void SfxRule_OnRuleChangeClick(object sender, SfxRule rule)
        {
            SfxRuleEditor ruleEditor = new(rule);
            ruleEditor.OnSaveRule += (object sender, SfxRule newRule) => {
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

        private void SfxRule_OnRuleRemoveClick(object sender, SfxRule rule)
        {
            RemoveRule(rule);
        }

        private void SfxRule_OnRuleDuplicateClick(object sender, SfxRule rule)
        {
            AddRule(
                JsonConvert.DeserializeObject<SfxRule>(
                    JsonConvert.SerializeObject(rule)
                )
            );
        }

        private void addNewRule_Click(object sender, RoutedEventArgs e)
        {
            SfxRuleEditor ruleEditor = new();
            ruleEditor.OnSaveRule += (object sender, SfxRule newRule) => {
                AddRule(newRule);
            };
            ruleEditor.ShowDialog();
        }

        private void presetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainWindow == null || presetList.Items.Count == 0) return;

            mainWindow.SwitchPreset<SfxRule, SfxRuleController>(
                presetList.SelectedItem.ToString(), Enums.ManagerType.SFX, Helper.settings.sfxRulesPreset
            );

            ReloadRules();
        }

        private void sfxRulesRenamePresetButton_Click(object sender, RoutedEventArgs e)
        {
            this.isEditMode = true;
            this.presetEditName.Text = Helper.settings.activePresets[Enums.ManagerType.SFX];
            this.presetEditTitle.Text = Properties.Resources.t_renamePreset;
            this.presetEditCard.Visibility = Visibility.Visible;
        }

        private void sfxRulesDeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.RemovePreset<SfxRule, SfxRuleController>(
                    Helper.settings.activePresets[ManagerType.SFX], ManagerType.SFX, Helper.settings.sfxRulesPreset
                );
            this.ReloadPresets();
        }

        private void sfxRulesDuplicatePresetButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.DuplicatePreset<SfxRule, SfxRuleController>(
                ManagerType.SFX, Helper.settings.sfxRulesPreset
            );
            this.ReloadPresets();
        }

        private void sfxRulesAddPresetButton_Click(object sender, RoutedEventArgs e)
        {
            this.isEditMode = false;
            this.presetEditName.Text = $"{Properties.Resources.t_preset} {Helper.settings.sfxRulesPreset.Count}";
            this.presetEditTitle.Text = Properties.Resources.t_createPreset;
            this.presetEditCard.Visibility = Visibility.Visible;
        }

        private void presetEditSave_Click(object sender, RoutedEventArgs e)
        {
            this.presetEditCard.Visibility = Visibility.Collapsed;

            if (isEditMode)
            {
                mainWindow.RenamePreset<SfxRule, SfxRuleController>(
                    presetEditName.Text, ManagerType.SFX, Helper.settings.sfxRulesPreset
                );

            }
            else
            {
                mainWindow.CreatePreset<SfxRule, SfxRuleController>(
                    presetEditName.Text, ManagerType.SFX, Helper.settings.sfxRulesPreset
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
