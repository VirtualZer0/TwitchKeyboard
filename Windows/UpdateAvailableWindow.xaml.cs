﻿using System;
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

namespace TwitchKeyboard.Windows
{
  /// <summary>
  /// Логика взаимодействия для UpdateAvailable.xaml
  /// </summary>
  public partial class UpdateAvailableWindow : Window
  {
    public UpdateAvailableWindow()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      new Process
      {
        StartInfo =
          new ProcessStartInfo("https://github.com/VirtualZer0/TwitchKeyboard/releases")
          { UseShellExecute = true }
      }.Start();
    }

    private void Changelog_Click(object sender, RoutedEventArgs e)
    {
      new Process
      {
        StartInfo =
          new ProcessStartInfo("https://github.com/VirtualZer0/TwitchKeyboard/releases")
          { UseShellExecute = true }
      }.Start();
    }
  }
}
