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
  /// Логика взаимодействия для WebRuleList.xaml
  /// </summary>
  public partial class WebRuleList : UserControl
  {
    MainWindow mainWindow;
    List<WebRule> rules;

    public bool isEditMode = false;
    public bool isFirstSelection = true;

    public delegate void OnRulesChangedHandler(object sender, List<WebRule> rules);
    public event OnRulesChangedHandler OnRulesChanged;

    public WebRuleList()
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
      var keys = Helper.settings.webRulesPreset.Keys.ToArray();
      presetList.Items.Clear();
      for (int i = 0; i < keys.Length; i++)
      {
        presetList.Items.Add(keys[i]);
      }

      presetList.SelectedItem = Helper.settings.activePresets[Enums.ManagerType.WEB];
    }

    public void ReloadRules()
    {
      this.rules = Helper.settings.webRulesPreset[Helper.settings.activePresets[ManagerType.WEB]];

      var addButton = ruleList.Children[^1];
      ruleList.Children.Remove(addButton);

      ruleList.Children.Clear();

      for (int i = 0; i < rules.Count; i++)
      {
        WebRulePreview webRule = new(this.rules[i]);
        webRule.OnRuleChangeClick += WebRule_OnRuleChangeClick;
        webRule.OnRuleRemoveClick += WebRule_OnRuleRemoveClick;
        webRule.OnRuleDuplicateClick += WebRule_OnRuleDuplicateClick;

        ruleList.Children.Add(webRule);
      }

      ruleList.Children.Add(addButton);
    }

    public void ChangeRule(WebRule oldRule, WebRule newRule)
    {
      int index = rules.IndexOf(oldRule);
      rules[index] = newRule;
      ((WebRulePreview)ruleList.Children[index]).UpdateRule(newRule);
      OnRulesChanged?.Invoke(this, rules);
    }

    public void AddRule(WebRule rule)
    {
      var addButton = ruleList.Children[^1];
      ruleList.Children.Remove(addButton);

      WebRulePreview webRule = new(rule);
      webRule.OnRuleChangeClick += WebRule_OnRuleChangeClick;
      webRule.OnRuleRemoveClick += WebRule_OnRuleRemoveClick;
      webRule.OnRuleDuplicateClick += WebRule_OnRuleDuplicateClick;

      ruleList.Children.Add(webRule);
      ruleList.Children.Add(addButton);

      rules.Add(rule);
      OnRulesChanged?.Invoke(this, rules);
    }

    public void RemoveRule(WebRule rule)
    {
      if (rules.Contains(rule))
      {
        int index = rules.IndexOf(rule);
        ruleList.Children.RemoveAt(index);
        rules.RemoveAt(index);
        OnRulesChanged?.Invoke(this, rules);
      }
    }

    private void WebRule_OnRuleChangeClick(object sender, WebRule rule)
    {
      WebRuleEditor ruleEditor = new(rule);
      ruleEditor.OnSaveRule += (object sender, WebRule newRule) =>
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

    private void WebRule_OnRuleRemoveClick(object sender, WebRule rule)
    {
      RemoveRule(rule);
    }

    private void WebRule_OnRuleDuplicateClick(object sender, WebRule rule)
    {
      AddRule(
          JsonConvert.DeserializeObject<WebRule>(
              JsonConvert.SerializeObject(rule)
          )
      );
    }

    private void addNewRule_Click(object sender, RoutedEventArgs e)
    {
      WebRuleEditor ruleEditor = new();
      ruleEditor.OnSaveRule += (object sender, WebRule newRule) =>
      {
        AddRule(newRule);
      };
      ruleEditor.ShowDialog();
    }

    private void presetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (mainWindow == null || presetList.Items.Count == 0) return;

      mainWindow.SwitchPreset<WebRule, WebRuleController>(
          presetList.SelectedItem.ToString(), Enums.ManagerType.WEB, Helper.settings.webRulesPreset
      );

      ReloadRules();
    }

    private void webRulesRenamePresetButton_Click(object sender, RoutedEventArgs e)
    {
      this.isEditMode = true;
      this.presetEditName.Text = Helper.settings.activePresets[Enums.ManagerType.WEB];
      this.presetEditTitle.Text = Properties.Resources.t_renamePreset;
      this.presetEditCard.Visibility = Visibility.Visible;
    }

    private void webRulesDeletePresetButton_Click(object sender, RoutedEventArgs e)
    {
      mainWindow.RemovePreset<WebRule, WebRuleController>(
              Helper.settings.activePresets[ManagerType.WEB], ManagerType.WEB, Helper.settings.webRulesPreset
          );
      this.ReloadPresets();
    }

    private void webRulesDuplicatePresetButton_Click(object sender, RoutedEventArgs e)
    {
      mainWindow.DuplicatePreset<WebRule, WebRuleController>(
          ManagerType.WEB, Helper.settings.webRulesPreset
      );
      this.ReloadPresets();
    }

    private void webRulesAddPresetButton_Click(object sender, RoutedEventArgs e)
    {
      this.isEditMode = false;
      this.presetEditName.Text = $"{Properties.Resources.t_preset} {Helper.settings.webRulesPreset.Count}";
      this.presetEditTitle.Text = Properties.Resources.t_createPreset;
      this.presetEditCard.Visibility = Visibility.Visible;
    }

    private void presetEditSave_Click(object sender, RoutedEventArgs e)
    {
      this.presetEditCard.Visibility = Visibility.Collapsed;

      if (isEditMode)
      {
        mainWindow.RenamePreset<WebRule, WebRuleController>(
            presetEditName.Text, ManagerType.WEB, Helper.settings.webRulesPreset
        );

      }
      else
      {
        mainWindow.CreatePreset<WebRule, WebRuleController>(
            presetEditName.Text, ManagerType.WEB, Helper.settings.webRulesPreset
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
