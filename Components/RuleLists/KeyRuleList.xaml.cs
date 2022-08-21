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
  /// Логика взаимодействия для KeyRuleList.xaml
  /// </summary>
  public partial class KeyRuleList : UserControl, IRuleList
  {
    MainWindow mainWindow;
    List<KeyRule> rules;

    public bool isEditMode = false;
    public bool isFirstSelection = true;

    public delegate void OnRulesChangedHandler(object sender, List<KeyRule> rules);
    public event OnRulesChangedHandler OnRulesChanged;

    public KeyRuleList()
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
      var keys = Helper.settings.keyRulesPreset.Keys.ToArray();
      presetList.Items.Clear();
      for (int i = 0; i < keys.Length; i++)
      {
        presetList.Items.Add(keys[i]);
      }

      presetList.SelectedItem = Helper.settings.activePresets[Enums.ManagerType.KEYBOARD];
    }

    public void ReloadRules()
    {
      this.rules = Helper.settings.keyRulesPreset[Helper.settings.activePresets[Enums.ManagerType.KEYBOARD]];

      var addButton = ruleList.Children[^1];
      ruleList.Children.Remove(addButton);

      ruleList.Children.Clear();

      for (int i = 0; i < rules.Count; i++)
      {
        KeyRulePreview keyRule = new(this.rules[i]);
        keyRule.OnRuleChangeClick += KeyRule_OnRuleChangeClick;
        keyRule.OnRuleRemoveClick += KeyRule_OnRuleRemoveClick;
        keyRule.OnRuleDuplicateClick += KeyRule_OnRuleDuplicateClick;

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
      keyRule.OnRuleDuplicateClick += KeyRule_OnRuleDuplicateClick;

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
      ruleEditor.OnSaveRule += (object sender, KeyRule newRule) =>
      {
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

    private void KeyRule_OnRuleDuplicateClick(object sender, KeyRule rule)
    {
      AddRule(
          JsonConvert.DeserializeObject<KeyRule>(
              JsonConvert.SerializeObject(rule)
          )
      );
    }

    private void addNewRule_Click(object sender, RoutedEventArgs e)
    {
      KeyRuleEditor ruleEditor = new();
      ruleEditor.OnSaveRule += (object sender, KeyRule newRule) =>
      {
        AddRule(newRule);
      };
      ruleEditor.ShowDialog();
    }

    private void presetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (mainWindow == null || presetList.Items.Count == 0) return;
      if (isFirstSelection)
      {
        isFirstSelection = false;
        return;
      }

      mainWindow.SwitchPreset<KeyRule, KeyRuleController>(
          presetList.SelectedItem.ToString(), Enums.ManagerType.KEYBOARD, Helper.settings.keyRulesPreset
      );

      ReloadRules();
    }

    private void keyRulesRenamePresetButton_Click(object sender, RoutedEventArgs e)
    {
      this.isEditMode = true;
      this.presetEditName.Text = Helper.settings.activePresets[Enums.ManagerType.KEYBOARD];
      this.presetEditTitle.Text = Properties.Resources.t_renamePreset;
      this.presetEditCard.Visibility = Visibility.Visible;
    }

    private void keyRulesDeletePresetButton_Click(object sender, RoutedEventArgs e)
    {
      mainWindow.RemovePreset<KeyRule, KeyRuleController>(
              Helper.settings.activePresets[ManagerType.KEYBOARD], ManagerType.KEYBOARD, Helper.settings.keyRulesPreset
          );
      this.ReloadPresets();
    }

    private void keyRulesDuplicatePresetButton_Click(object sender, RoutedEventArgs e)
    {
      mainWindow.DuplicatePreset<KeyRule, KeyRuleController>(
          ManagerType.KEYBOARD, Helper.settings.keyRulesPreset
      );
      this.ReloadPresets();
    }

    private void keyRulesAddPresetButton_Click(object sender, RoutedEventArgs e)
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
        mainWindow.RenamePreset<KeyRule, KeyRuleController>(
            presetEditName.Text, ManagerType.KEYBOARD, Helper.settings.keyRulesPreset
        );

      }
      else
      {
        mainWindow.CreatePreset<KeyRule, KeyRuleController>(
            presetEditName.Text, ManagerType.KEYBOARD, Helper.settings.keyRulesPreset
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
