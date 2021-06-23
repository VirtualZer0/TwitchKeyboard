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
    /// Логика взаимодействия для NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        readonly List<LaunchedRuleIndicator> indicators = new();
        readonly List<LaunchedRuleIndicator> indicatorsForRemoving = new();

        public NotificationWindow()
        {
            InitializeComponent();
        }

        public void AddNewRule (LaunchedRuleIndicator indicator)
        {
            indicators.Add(indicator);
            indicatorsContainer.Children.Add(indicator.buttonIndicator);
        }

        public void Update (int elapsedTime)
        {
            int indicatorsCount = indicators.Count;
            for (int i = 0; i < indicatorsCount; i++)
            {
                indicators[i].Update(elapsedTime);
                if (indicators[i].markedForRemove)
                {
                    indicatorsForRemoving.Add(indicators[i]);
                    this.Dispatcher.Invoke(() => indicatorsContainer.Children.Remove(indicators[i].buttonIndicator));
                }
            }

            int indicatorsForRemovingCount = indicatorsForRemoving.Count;
            for (int i = 0; i < indicatorsForRemovingCount; i++)
            {
                indicators.Remove(indicatorsForRemoving[i]);
            }

            indicatorsForRemoving.Clear();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
