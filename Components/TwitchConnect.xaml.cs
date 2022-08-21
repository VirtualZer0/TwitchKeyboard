using MaterialDesignThemes.Wpf;
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
using TwitchKeyboard.Classes.Services;

namespace TwitchKeyboard.Components
{
  /// <summary>
  /// Логика взаимодействия для TwitchConnect.xaml
  /// </summary>
  public partial class TwitchConnect : UserControl
  {
    public delegate void OnActionClickHandler(object sender, string channelName);
    public event OnActionClickHandler OnActionClick;

    string channel = "";

    public TwitchConnect()
    {
      InitializeComponent();
      if (Helper.settings == null) return;

      this.channel = Helper.settings.channel;
      this.channelName.Text = this.channel;
    }

    public void ChangeConnectionState(TwitchConnectionState state)
    {
      switch (state)
      {
        case TwitchConnectionState.DISCONNECTED:
          connectionStatus.Content = Properties.Resources.t_disconnected;
          connectionStatus.Background = Helper.Brushes.bGrayO;
          connectionStatus.IconBackground = Helper.Brushes.bGray;
          ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.MessageBulletedOff;
          connectButton.Content = Properties.Resources.t_connect;
          connectButton.IsEnabled = true;
          ButtonProgressAssist.SetIsIndicatorVisible(connectButton, false);
          break;

        case TwitchConnectionState.IN_PROGRESS:
          connectionStatus.Content = Properties.Resources.t_connecting;
          connectionStatus.Background = Helper.Brushes.bYellowO;
          connectionStatus.IconBackground = Helper.Brushes.bYellow;
          ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.ContactlessPayment;
          connectButton.Content = Properties.Resources.t_wait;
          ButtonProgressAssist.SetIsIndicatorVisible(connectButton, true);
          connectButton.IsEnabled = false;
          break;

        case TwitchConnectionState.CONNECTED:
          connectionStatus.Content = Properties.Resources.t_joining;
          connectionStatus.Background = Helper.Brushes.bBlueO;
          connectionStatus.IconBackground = Helper.Brushes.bBlue;
          ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.AccountArrowRight;
          connectButton.Content = Properties.Resources.t_wait;
          ButtonProgressAssist.SetIsIndicatorVisible(connectButton, true);
          connectButton.IsEnabled = false;
          break;

        case TwitchConnectionState.JOINED:
          connectionStatus.Content = Properties.Resources.t_connected;
          connectionStatus.Background = Helper.Brushes.bGreenO;
          connectionStatus.IconBackground = Helper.Brushes.bGreen;
          ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.MessageBulleted;
          connectButton.Content = Properties.Resources.t_disconnect;
          connectButton.IsEnabled = true;
          ButtonProgressAssist.SetIsIndicatorVisible(connectButton, false);
          break;

        case TwitchConnectionState.ERROR:
          connectionStatus.Content = Properties.Resources.t_error;
          connectionStatus.Background = Helper.Brushes.bRedO;
          connectionStatus.IconBackground = Helper.Brushes.bRed;
          ((PackIcon)connectionStatus.Icon).Kind = PackIconKind.AlertCircle;
          connectButton.Content = Properties.Resources.t_connect;
          connectButton.IsEnabled = true;
          ButtonProgressAssist.SetIsIndicatorVisible(connectButton, false);
          break;
      }
    }

    public void SetChannel(string channel)
    {
      channelName?.Dispatcher.Invoke(() => channelName.Text = channel);
      this.channel = channel;
    }

    public string GetChannel()
    {
      channelName?.Dispatcher.Invoke(() =>
      {
        if (channel == "")
        {
          this.channel = channelName.Text;
        }
      });

      return channel;
    }

    private void connectButton_Click(object sender, RoutedEventArgs e)
    {
      this.channel = channelName.Text;
      OnActionClick?.Invoke(this, channel);
    }
  }
}
