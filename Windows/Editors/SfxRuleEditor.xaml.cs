using Microsoft.Win32;
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
using System.Windows.Shapes;
using TwitchKeyboard.Classes;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Components;

namespace TwitchKeyboard.Windows.Editors
{
  /// <summary>
  /// Логика взаимодействия для SfxRuleEditor.xaml
  /// </summary>
  public partial class SfxRuleEditor : Window
  {
    readonly SfxRule rule = null;
    readonly MediaPlayerExt player = new();

    public delegate void OnSaveRuleHandler(object sender, SfxRule rule);
    public event OnSaveRuleHandler OnSaveRule;

    public SfxRuleEditor(SfxRule rule = null)
    {
      InitializeComponent();
      player.MediaOpened += (object sender, EventArgs e) => { player.Stop(); };

      if (rule == null)
      {
        rule = this.rule = new SfxRule();
        this.rule.events.Add(new TwitchTrigger());
      }
      else
      {
        // Creating copy of current rule (not the most optimal method, but simple enough)
        this.rule = JsonConvert.DeserializeObject<SfxRule>(
            JsonConvert.SerializeObject(rule)
        );

        titleText.Text = Properties.Resources.t_editSfxRule;
        player.Open(rule.file);
        player.Stop();
      }

      for (int i = 0; i < this.rule.events.Count; i++)
      {
        createEventUIElement(this.rule.events[i]);
      }

      delayValue.Text = Helper.TimerIntToString(rule.delay);
      cooldownValue.Text = Helper.TimerIntToString(rule.cooldown);
      sfxVolume.Value = rule.volume;
      sfxBalance.Value = rule.balance;
      sfxLoop.Text = rule.loopCount.ToString();
      selectSfxButton.Content = rule.file == null ? Properties.Resources.t_selectFile : System.IO.Path.GetFileName(rule.file.LocalPath);
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
      rule.delay = Helper.StringToTimerInt(delayValue.Text);
      rule.cooldown = Helper.StringToTimerInt(cooldownValue.Text);

      rule.volume = (int)Math.Round(sfxVolume.Value);
      rule.balance = (int)Math.Round(sfxBalance.Value);
      rule.loopCount = int.Parse(sfxLoop.Text);

      for (int i = 0; i < eventsContainer.Children.Count; i++)
      {
        if (eventsContainer.Children[i] is TriggerEditor tEditor)
        {
          tEditor.SaveTrigger();
        }
      }

      OnSaveRule?.Invoke(this, rule);
      player.Close();
      this.Close();
    }

    private void cancelRuleButton_Click(object sender, RoutedEventArgs e)
    {
      player.Close();
      this.Close();
    }

    private void selectSfxButton_Click(object sender, RoutedEventArgs e)
    {
      var fileDialog = new OpenFileDialog
      {
        Title = Properties.Resources.t_selectAudiofile,
        Filter = "Audiofiles|*.mp3;*.wav;*.mp2;*.mid;*.snd;*.aif;*.aiff;*.aifc"
      };

      if ((bool)fileDialog.ShowDialog())
      {
        rule.file = new Uri(fileDialog.FileName);
        player.Open(rule.file);
        player.Stop();
      }

      selectSfxButton.Content = rule.file == null ? Properties.Resources.t_selectFile : System.IO.Path.GetFileName(rule.file.LocalPath);
    }

    private void sfxVolume_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      player.Stop();
      rule.volume = (int)Math.Round(sfxVolume.Value);
      player.Volume = rule.volume / 100.0;
      player.Play();

    }

    private void sfxSpeed_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      player.Stop();
      player.Play();
    }

    private void sfxBalance_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      player.Stop();
      rule.balance = (int)Math.Round(sfxBalance.Value);
      player.Balance = rule.balance / 100.0;
      player.Play();
    }
  }
}
