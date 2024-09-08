using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Classes.RuleControllers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;
using WindowsInput;

namespace TwitchKeyboard.Classes.Managers
{
  public class SfxRuleManager : BaseRuleManager
  {

    public void CreateControllers(List<SfxRule> rules)
    {
      CreateControllers<SfxRuleController>(rules.ToArray());
    }

    public void ChangeSFXVolume()
    {
      for (int i = 0; i < rules.Length; i++)
      {
        var sfxController = (SfxRuleController)rules[i];
        sfxController.ChangeSFXVolume();
      }
    }

    public override void LaunchRule(BaseRuleController baseRule, string user)
    {
      base.LaunchRule(baseRule, user);

      var rule = (SfxRuleController)baseRule;
      rule.curLoop = 0;
    }

    public override void UpdateRule(BaseRuleController baseRule, int elapsedTime)
    {
      base.UpdateRule(baseRule, elapsedTime);
      if (baseRule.state != RuleState.Active) return;
      baseRule.state = RuleState.Inactive;

      var rule = (SfxRuleController)baseRule;
      rule.player.Dispatcher.Invoke(() =>
      {
        rule.player.Stop();
        rule.player.Play();
      });
    }

    public override void DisableRule(BaseRuleController baseRule)
    {
      base.DisableRule(baseRule);
      var rule = (SfxRuleController)baseRule;
      rule.player.Dispatcher.Invoke(() =>
      {
        rule.player.Stop();
      });
    }
  }
}
