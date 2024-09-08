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

namespace TwitchKeyboard.Components
{
  /// <summary>
  /// Логика взаимодействия для KeysContainer.xaml
  /// </summary>
  public partial class KeysContainer : UserControl
  {
    public delegate void OnAddKeyPressedHandler(object sender);
    public event OnAddKeyPressedHandler OnAddKeyPressed;

    public delegate void OnKeyRemoveHandler(object sender, Key k);
    public event OnKeyRemoveHandler OnKeyRemove;

    List<Key> keys = new();

    public KeysContainer()
    {
      InitializeComponent();
    }

    public void SetKeys(List<Key> keys)
    {
      this.keys = keys;
      keysPanel.Children.Clear();

      for (int i = 0; i < keys.Count; i++)
      {
        addKeyToPanel(keys[i]);
      }
    }

    public void AddKey(Key key)
    {
      pressKeyText.Visibility = Visibility.Collapsed;
      if (keys.Contains(key)) return;

      this.keys.Add(key);
      this.addKeyToPanel(key);
    }

    public void RemoveKey(Key key)
    {
      // In theory, index in list and index in container should be equal
      keysPanel.Children.RemoveAt(keys.IndexOf(key));
      keys.Remove(key);
    }

    public void addKeyToPanel(Key key)
    {
      Chip keyChip = new();
      keyChip.Content = key.ToString();
      keyChip.Margin = new Thickness(0, 0, 8, 8);
      keyChip.IsDeletable = true;
      keyChip.DeleteClick += (object sender, RoutedEventArgs e) =>
      {
        this.OnKeyRemove?.Invoke(this, key);
      };
      keysPanel.Children.Add(keyChip);
    }

    private void addKeyButton_Click(object sender, RoutedEventArgs e)
    {
      pressKeyText.Visibility = Visibility.Visible;
      this.OnAddKeyPressed?.Invoke(this);
    }
  }
}
