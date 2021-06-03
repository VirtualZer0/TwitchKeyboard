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
using TwitchGQL;

namespace TwitchKeyboard
{
    /// <summary>
    /// Логика взаимодействия для RewardsScreen.xaml
    /// </summary>
    public partial class RewardsScreen : Window
    {
        public delegate void OnRewardSelectedHandler(object sender, CustomReward e);
        public event OnRewardSelectedHandler OnRewardSelected;

        public RewardsScreen(List<CustomReward> rewards)
        {
            InitializeComponent();
            if (rewards.Count == 0)
            {
                this.noRewardsMsg.Visibility = Visibility.Visible;
            }

            rewards.ForEach(reward =>
            {
                Button container = new Button();
                container.Padding = new Thickness(8);
                container.Margin = new Thickness(8);
                container.Height = 48;
                container.Width = 280;
                container.Background = new SolidColorBrush(Color.FromRgb(66, 66, 66));
                container.BorderBrush = null;

                StackPanel innerContainer = new StackPanel();
                innerContainer.Orientation = Orientation.Horizontal;
                innerContainer.HorizontalAlignment = HorizontalAlignment.Left;
                innerContainer.VerticalAlignment = VerticalAlignment.Center;

                System.Windows.Controls.Image img = new();
                img.Width = 30;
                img.Height = 30;
                img.Margin = new Thickness(0, 0, 16, 0);

                BitmapImage bImg = new(new Uri(reward.image?.url2x ?? reward.defaultImage.url2x));
                img.Source = bImg;

                TextBlock text = new TextBlock();
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Text = reward.title;

                innerContainer.Children.Add(img);
                innerContainer.Children.Add(text);
                container.Content = innerContainer;
                container.Click += (object sender, RoutedEventArgs e) =>
                {
                    this.OnRewardSelected?.Invoke(this, reward);
                    this.Close();
                };

                rewardsContainer.Children.Add(container);
            });
        }
    }
}
