using Classes.APIModels.TwitchGQL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TwitchKeyboard.Classes
{
  public static class Helper
  {
    public static UserSettings settings = null;

    /// <summary>
    /// Most used brushes
    /// </summary>
    public static class Brushes
    {
      public static readonly SolidColorBrush bFg = new(Color.FromRgb(250, 250, 250));

      public static readonly SolidColorBrush bGray = new(Color.FromRgb(158, 158, 158));
      public static readonly SolidColorBrush bGrayO = new(Color.FromArgb(63, 158, 158, 158));

      public static readonly SolidColorBrush bRed = new(Color.FromRgb(244, 67, 54));
      public static readonly SolidColorBrush bRedO = new(Color.FromArgb(63, 244, 67, 54));

      public static readonly SolidColorBrush bYellow = new(Color.FromRgb(255, 235, 59));
      public static readonly SolidColorBrush bYellowO = new(Color.FromArgb(63, 255, 235, 59));

      public static readonly SolidColorBrush bGreen = new(Color.FromRgb(76, 175, 80));
      public static readonly SolidColorBrush bGreenO = new(Color.FromArgb(63, 76, 175, 80));

      public static readonly SolidColorBrush bBlue = new(Color.FromRgb(3, 169, 244));
      public static readonly SolidColorBrush bBlueO = new(Color.FromArgb(63, 3, 169, 244));

      public static readonly SolidColorBrush bPurple = new(Color.FromRgb(103, 58, 183));
      public static readonly SolidColorBrush bPurpleO = new(Color.FromArgb(63, 103, 58, 183));

      public static readonly SolidColorBrush bOrange = new(Color.FromRgb(239, 108, 0));
      public static readonly SolidColorBrush bOrangeO = new(Color.FromArgb(63, 239, 108, 0));
    }

    public static string TimerIntToString(int i)
    {
      return (i / 1000.0).ToString("F1", new CultureInfo("en-US").NumberFormat);
    }

    public static int StringToTimerInt(string s)
    {
      return (int)(float.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat) * 1000);
    }

    public static double Interpolate(double minVal, double maxVal, double minPos, double maxPos, double curPos)
    {
      return minVal + (curPos - minPos) * ((maxVal - minVal) / (maxPos - minPos));
    }
  }
}
