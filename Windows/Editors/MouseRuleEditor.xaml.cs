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
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Windows.Editors
{
  /// <summary>
  /// Логика взаимодействия для MouseRuleEditor.xaml
  /// </summary>
  public partial class MouseRuleEditor : Window
  {

    readonly MouseRule rule = null;

    /// <summary>
    /// Raised when the rule is saved or created.
    /// </summary>
    /// <param name="sender">Current window</param>
    /// <param name="rule">New rule object</param>
    public delegate void OnSaveRuleHandler(object sender, MouseRule rule);
    public event OnSaveRuleHandler OnSaveRule;

    public MouseRuleEditor(MouseRule rule = null)
    {
      InitializeComponent();

      if (rule == null)
      {
        rule = this.rule = new MouseRule();
        this.rule.events.Add(new TwitchTrigger());
      }
      else
      {
        // Creating copy of current rule (not the most optimal method, but simple enough)
        this.rule = JsonConvert.DeserializeObject<MouseRule>(
            JsonConvert.SerializeObject(rule)
        );

        titleText.Text = Properties.Resources.t_editMouseRule;
      }

      for (int i = 0; i < this.rule.events.Count; i++)
      {
        createEventUIElement(this.rule.events[i]);
      }

      delayValue.Text = Helper.TimerIntToString(this.rule.delay);
      durationValue.Text = Helper.TimerIntToString(this.rule.duration);
      cooldownValue.Text = Helper.TimerIntToString(this.rule.cooldown);

      switch (this.rule.mode)
      {
        case Enums.KeyPressMode.Press:
          mouseModeClickRadio.IsChecked = true; break;

        case Enums.KeyPressMode.Spam:
          mouseModeSpamRadio.IsChecked = true; break;

        case Enums.KeyPressMode.Double:
          mouseModeDoubleClickRadio.IsChecked = true; break;

        default:
          mouseModeHoldRadio.IsChecked = true; break;
      }

      xValue.Text = this.rule.X.ToString();
      yValue.Text = this.rule.Y.ToString();
      xMoveSpeed.Value = Math.Abs(this.rule.speedX);
      yMoveSpeed.Value = Math.Abs(this.rule.speedY);
      xMoveDirection.SelectedIndex = this.rule.speedX > 0 ? 1 : 0;
      yMoveDirection.SelectedIndex = this.rule.speedY > 0 ? 1 : 0;

      mouseType.SelectedIndex = (int)rule.ruleType;
    }

    private void addEventButton_Click(object sender, RoutedEventArgs e)
    {
      TwitchTrigger trigger = new();
      rule.events.Add(trigger);
      createEventUIElement(trigger);
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

    private void mouseType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      // Let's pretend that you don't see this line
      if (coordinatesBlock == null) return;

      if (mouseType.SelectedIndex <= (int)MouseRuleType.MiddleButton)
      {
        coordinatesBlock.Visibility = Visibility.Collapsed;
        xSpeedBlock.Visibility = Visibility.Collapsed;
        ySpeedBlock.Visibility = Visibility.Collapsed;
        mouseButtonModeBlock.Visibility = Visibility.Visible;
        durationBlock.Visibility =
            (bool)mouseModeSpamRadio.IsChecked || (bool)mouseModeHoldRadio.IsChecked ?
            Visibility.Visible : Visibility.Collapsed;
      }
      else if (mouseType.SelectedIndex == (int)MouseRuleType.Scroll)
      {
        coordinatesBlock.Visibility = Visibility.Visible;
        xSpeedBlock.Visibility = Visibility.Collapsed;
        ySpeedBlock.Visibility = Visibility.Collapsed;
        mouseButtonModeBlock.Visibility = Visibility.Collapsed;
        durationBlock.Visibility = Visibility.Collapsed;
      }
      else if (mouseType.SelectedIndex == (int)MouseRuleType.MoveTo)
      {
        coordinatesBlock.Visibility = Visibility.Visible;
        xSpeedBlock.Visibility = Visibility.Collapsed;
        ySpeedBlock.Visibility = Visibility.Collapsed;
        mouseButtonModeBlock.Visibility = Visibility.Collapsed;
        durationBlock.Visibility = Visibility.Collapsed;
      }
      else if (mouseType.SelectedIndex == (int)MouseRuleType.MoveBy)
      {
        coordinatesBlock.Visibility = Visibility.Collapsed;
        xSpeedBlock.Visibility = Visibility.Visible;
        ySpeedBlock.Visibility = Visibility.Visible;
        mouseButtonModeBlock.Visibility = Visibility.Collapsed;
        durationBlock.Visibility = Visibility.Visible;
      }
    }

    private void saveRuleButton_Click(object sender, RoutedEventArgs e)
    {
      for (int i = 0; i < eventsContainer.Children.Count; i++)
      {
        if (eventsContainer.Children[i] is TriggerEditor tEditor)
        {
          tEditor.SaveTrigger();
        }
      }

      rule.ruleType = (MouseRuleType)mouseType.SelectedIndex;
      rule.delay = Helper.StringToTimerInt(delayValue.Text);
      rule.duration = Helper.StringToTimerInt(durationValue.Text);
      rule.cooldown = Helper.StringToTimerInt(cooldownValue.Text);
      rule.X = int.Parse(xValue.Text);
      rule.Y = int.Parse(yValue.Text);

      if (xMoveSpeed.Value != 0 && xMoveSpeed.Value < 30) xMoveSpeed.Value = 30;
      if (yMoveSpeed.Value != 0 && yMoveSpeed.Value < 30) yMoveSpeed.Value = 30;

      rule.speedX = xMoveSpeed.Value * (xMoveDirection.SelectedIndex == 0 ? -1 : 1);
      rule.speedY = yMoveSpeed.Value * (yMoveDirection.SelectedIndex == 0 ? -1 : 1);

      OnSaveRule?.Invoke(this, rule);
      this.Close();
    }

    private void cancelRuleButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void mouseModeClickRadio_Checked(object sender, RoutedEventArgs e)
    {
      if (durationBlock == null) return;
      durationValue.Text = "0";
      durationBlock.Visibility = Visibility.Collapsed;
      rule.mode = KeyPressMode.Press;
    }

    private void mouseModeDoubleClickRadio_Checked(object sender, RoutedEventArgs e)
    {
      if (durationBlock == null) return;
      durationValue.Text = "0";
      durationBlock.Visibility = Visibility.Collapsed;
      rule.mode = KeyPressMode.Double;
    }

    private void mouseModeSpamRadio_Checked(object sender, RoutedEventArgs e)
    {
      if (durationBlock == null) return;
      durationBlock.Visibility = Visibility.Visible;
      rule.mode = KeyPressMode.Spam;
    }

    private void mouseModeHoldRadio_Checked(object sender, RoutedEventArgs e)
    {
      if (durationBlock == null) return;
      durationBlock.Visibility = Visibility.Visible;
      rule.mode = KeyPressMode.Hold;
    }
  }
}
