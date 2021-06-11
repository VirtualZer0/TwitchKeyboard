using Classes.APIModels.TwitchGQL;
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

namespace TwitchKeyboard.Windows
{
    /// <summary>
    /// Логика взаимодействия для RewardsScreen.xaml
    /// </summary>
    public partial class RewardsScreen : Window
    {
        public delegate void OnRewardSelectedHandler(object sender, CustomReward e);
        public event OnRewardSelectedHandler OnRewardSelected;

        public RewardsScreen()
        {
            var rewards = Helper.settings.rewardsCache;

            InitializeComponent();
            if (rewards.Length == 0)
            {
                this.noRewardsMsg.Visibility = Visibility.Visible;
            }

            foreach (var reward in rewards)
            {
                var bgColor = (SolidColorBrush)new BrushConverter().ConvertFromString(reward.backgroundColor);

                Button container = new();
                container.Padding = new Thickness(8);
                container.Margin = new Thickness(8);
                container.Height = 48;
                container.Width = 280;
                container.Background = bgColor;
                container.BorderBrush = null;

                StackPanel innerContainer = new();
                innerContainer.Orientation = Orientation.Horizontal;
                innerContainer.HorizontalAlignment = HorizontalAlignment.Left;
                innerContainer.VerticalAlignment = VerticalAlignment.Center;

                System.Windows.Controls.Image img = new();
                img.Width = 30;
                img.Height = 30;
                img.Margin = new Thickness(0, 0, 16, 0);

                BitmapImage bImg = new(new Uri(reward.image?.url2x ?? reward.defaultImage.url2x));
                img.Source = bImg;

                TextBlock text = new();
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Text = reward.title;
                if (bgColor.Color.R > 128 && bgColor.Color.G > 128 && bgColor.Color.B > 128)
                {
                    text.Foreground = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                }
                else
                {
                    text.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }

                innerContainer.Children.Add(img);
                innerContainer.Children.Add(text);
                container.Content = innerContainer;
                container.Click += (object sender, RoutedEventArgs e) =>
                {
                    this.OnRewardSelected?.Invoke(this, reward);
                    this.Close();
                };

                rewardsContainer.Children.Add(container);
            }
        }
    }
}
