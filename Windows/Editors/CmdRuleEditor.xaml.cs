using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using TwitchKeyboard.Classes;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Components;

namespace TwitchKeyboard.Windows.Editors
{
  /// <summary>
  /// Логика взаимодействия для CmdRuleEditor.xaml
  /// </summary>
  public partial class CmdRuleEditor : Window
  {
    readonly CmdRule rule = null;

    /// <summary>
    /// Raised when the rule is saved or created.
    /// </summary>
    /// <param name="sender">Current window</param>
    /// <param name="rule">New rule object</param>
    public delegate void OnSaveRuleHandler(object sender, CmdRule rule);
    public event OnSaveRuleHandler OnSaveRule;

    public CmdRuleEditor(CmdRule rule = null)
    {
      InitializeComponent();

      if (rule == null)
      {
        rule = this.rule = new CmdRule();
        this.rule.events.Add(new TwitchTrigger());
      }
      else
      {
        // Creating copy of current rule (not the most optimal method, but simple enough)
        this.rule = JsonConvert.DeserializeObject<CmdRule>(
            JsonConvert.SerializeObject(rule)
        );

        titleText.Text = Properties.Resources.t_editCmdRule;
      }

      for (int i = 0; i < this.rule.events.Count; i++)
      {
        createEventUIElement(this.rule.events[i]);
      }

      cmdValue.Text = rule.cmd;
      selectFileButton.Content = rule.file == "" ? Properties.Resources.t_selectFile : System.IO.Path.GetFileName(rule.file);
      modeValue.SelectedIndex = rule.openFile ? 1 : 0;
      ruleNameValue.Text = rule.name;
      delayValue.Text = Helper.TimerIntToString(rule.delay);
      cooldownValue.Text = Helper.TimerIntToString(rule.cooldown);
    }

    private void createEventUIElement(TwitchTrigger trigger)
    {
      TriggerEditor editor = new(trigger);
      editor.OnRemove += Editor_OnRemove;
      editor.OnDuplicate += Editor_OnDuplicate;
      editor.Margin = new Thickness(0, 0, 0, 12);
      eventsContainer.Children.Add(editor);
    }

    private void Editor_OnDuplicate(object sender, TwitchTrigger triggerCopy)
    {
      createEventUIElement(triggerCopy);
      rule.events.Add(triggerCopy);
    }

    private void Editor_OnRemove(object sender, TwitchTrigger removedTrigger)
    {
      eventsContainer.Children.Remove((UIElement)sender);
      rule.events.Remove(removedTrigger);
    }

    private void addEventButton_Click(object sender, RoutedEventArgs e)
    {
      TwitchTrigger trigger = new();
      rule.events.Add(trigger);
      createEventUIElement(trigger);
    }

    private void saveRuleButton_Click(object sender, RoutedEventArgs e)
    {
      rule.cmd = cmdValue.Text;
      rule.openFile = modeValue.SelectedIndex == 1;
      rule.name = ruleNameValue.Text;
      rule.delay = Helper.StringToTimerInt(delayValue.Text);
      rule.cooldown = Helper.StringToTimerInt(cooldownValue.Text);

      for (int i = 0; i < eventsContainer.Children.Count; i++)
      {
        if (eventsContainer.Children[i] is TriggerEditor tEditor)
        {
          tEditor.SaveTrigger();
        }
      }

      OnSaveRule?.Invoke(this, rule);
      this.Close();
    }

    private void cancelRuleButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void runRuleButton_Click(object sender, RoutedEventArgs e)
    {
      if (modeValue.SelectedIndex == 1)
      {
        Task.Run(() =>
        {
          try
          {
            new Process { StartInfo = new ProcessStartInfo(rule.file) { UseShellExecute = true } }.Start();
          }
          catch { }
        });
      }
      else
      {
        Task.Run(() =>
        {
          try
          {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
              WindowStyle = ProcessWindowStyle.Hidden,
              FileName = "cmd.exe",
              CreateNoWindow = true
            };
            cmdValue.Dispatcher.Invoke(() => startInfo.Arguments = $"/c \"{cmdValue.Text}\"");
            process.StartInfo = startInfo;
            process.Start();
          }
          catch { }
        });
      }
    }

    private void modeValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmdFileBlock == null) return;
      if (modeValue.SelectedIndex == 0)
      {
        cmdFileBlock.Visibility = Visibility.Collapsed;
        cmdValue.Visibility = Visibility.Visible;
      }
      else
      {
        cmdFileBlock.Visibility = Visibility.Visible;
        cmdValue.Visibility = Visibility.Collapsed;
      }
    }

    private void selectFileButton_Click(object sender, RoutedEventArgs e)
    {
      var fileDialog = new OpenFileDialog
      {
        Title = Properties.Resources.t_selectFile
      };

      if ((bool)fileDialog.ShowDialog())
      {
        rule.file = fileDialog.FileName;
      }

      selectFileButton.Content = rule.file == null ? Properties.Resources.t_selectFile : System.IO.Path.GetFileName(rule.file);
    }
  }
}
