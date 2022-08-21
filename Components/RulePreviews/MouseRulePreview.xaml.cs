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
  /// Логика взаимодействия для MouseRulePreview.xaml
  /// </summary>
  public partial class MouseRulePreview : UserControl
  {
    MouseRule rule;

    public delegate void OnRuleRemoveClickHandler(object sender, MouseRule rule);
    public event OnRuleRemoveClickHandler OnRuleRemoveClick;

    public delegate void OnRuleChangeClickHandler(object sender, MouseRule rule);
    public event OnRuleChangeClickHandler OnRuleChangeClick;

    public delegate void OnRuleDuplicateClickHandler(object sender, MouseRule rule);
    public event OnRuleDuplicateClickHandler OnRuleDuplicateClick;

    public MouseRulePreview(MouseRule rule)
    {
      InitializeComponent();
      UpdateRule(rule);
    }

    public void UpdateRule(MouseRule rule)
    {
      this.rule = rule;
      SetKeyRuleValues();
    }

    public void SetKeyRuleValues()
    {
      titleText.Text = rule.GetName();

      if (rule.ruleType <= Enums.MouseRuleType.MiddleButton)
      {
        modeText.Text = $"{rule.mode}";
      }
      else if (rule.ruleType <= Enums.MouseRuleType.MoveTo)
      {
        modeText.Text = $"X:{rule.X} | Y:{rule.Y}";
      }
      else
      {
        modeText.Text = $"X:{Math.Round(rule.speedX)} | Y:{Math.Round(rule.speedY)}";
      }

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
