using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes
{
    public class LaunchedRuleIndicator
    {
        public readonly Button buttonIndicator = new() { Margin = new Thickness(0,0,8,8) };
        int delay = 0;
        int duration = 3000;
        bool isEnabled = false;

        public bool markedForRemove = false;

        public LaunchedRuleIndicator (ManagerType eventType, string text, int delay, int duration = 0)
        {
            this.duration += duration;
            this.delay = delay;

            StackPanel bContainer = new() { Orientation = Orientation.Horizontal};
            PackIcon bIcon = new() { Margin = new Thickness(0, 0, 8, 0) };
            TextBlock bText = new() { Text = text };
            ButtonProgressAssist.SetIsIndicatorVisible(buttonIndicator, true);

            switch (eventType)
            {
                case ManagerType.KEYBOARD: bIcon.Kind = PackIconKind.Keyboard; break;
                case ManagerType.MOUSE: bIcon.Kind = PackIconKind.Mouse; break;
                case ManagerType.SFX: bIcon.Kind = PackIconKind.VolumeHigh; break;
                case ManagerType.WEB: bIcon.Kind = PackIconKind.Web; break;
                case ManagerType.CMD: bIcon.Kind = PackIconKind.Console; break;
            }

            if (delay == 0)
            {
                isEnabled = true;
                ButtonProgressAssist.SetMaximum(buttonIndicator, duration);
                ButtonProgressAssist.SetValue(buttonIndicator, duration);
            }
            else
            {
                buttonIndicator.IsEnabled = false;
                ButtonProgressAssist.SetMaximum(buttonIndicator, delay);
                ButtonProgressAssist.SetValue(buttonIndicator, delay);
            }

            bContainer.Children.Add(bIcon);
            bContainer.Children.Add(bText);
            buttonIndicator.Content = bContainer;
        }

        public void Update(int elapsedTime)
        {
            if (markedForRemove) return;

            if (isEnabled)
            {
                duration -= elapsedTime;
                buttonIndicator.Dispatcher.Invoke(() => ButtonProgressAssist.SetValue(buttonIndicator, duration));
            }
            else
            {
                delay -= elapsedTime;
                buttonIndicator.Dispatcher.Invoke(() => ButtonProgressAssist.SetValue(buttonIndicator, delay));
            }

            if (!isEnabled && delay <= 0)
            {
                isEnabled = true;
                buttonIndicator.Dispatcher.Invoke(() => {
                    buttonIndicator.IsEnabled = true;
                    ButtonProgressAssist.SetMaximum(buttonIndicator, duration);
                });
            }
            else if (isEnabled && duration <= 0)
            {
                markedForRemove = true;
            }
        }
    }
}
