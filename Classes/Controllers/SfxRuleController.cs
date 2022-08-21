using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes.Controllers
{
  /// <summary>
  /// Controls the operation of a specific sfx rule, based on its model
  /// </summary>
  public class SfxRuleController : BaseRuleController
  {
    public override ManagerType cType { get => ManagerType.SFX; }

    public readonly MediaPlayerExt player = new();
    public int curLoop = 0;

    public override void Init()
    {
      SfxRule rule = (SfxRule)model;
      player.Dispatcher.Invoke(() =>
      {
        player.Volume = (rule.volume / 100.0) * (Helper.settings.mainSfxVolume / 100.0);
        player.Open(rule.file);
        player.Stop();
        player.Balance = rule.balance / 100.0;
        player.MediaEnded += Player_MediaEnded;
      });
    }

    public override void Destroy()
    {
      base.Destroy();
      player.Dispatcher.Invoke(() =>
      {
        player.Stop();
        player.Close();
      });
    }

    public void ChangeSFXVolume()
    {
      player.Dispatcher.Invoke(() =>
      {
        player.Volume = ((model as SfxRule).volume / 100.0) * (Helper.settings.mainSfxVolume / 100.0);
      });
    }

    private void Player_MediaEnded(object sender, EventArgs e)
    {
      SfxRule rule = (SfxRule)model;
      if (curLoop < rule.loopCount - 1)
      {
        curLoop++;
        player.Dispatcher.Invoke(() =>
        {
          player.Stop();
          player.Play();
        });
      }
    }
  }
}
