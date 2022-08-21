using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Classes.RuleControllers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;
using WindowsInput;

namespace TwitchKeyboard.Classes.Managers
{
  public class MouseRuleManager : BaseRuleManager
  {
    readonly InputSimulator simulator = new();

    public void CreateControllers(List<MouseRule> rules)
    {
      CreateControllers<MouseRuleController>(rules.ToArray());
    }

    public override void LaunchRule(BaseRuleController baseRule, string user)
    {
      base.LaunchRule(baseRule, user);

      var rule = (MouseRuleController)baseRule;
      var mouseRule = (MouseRule)rule.model;
      rule.curTime = mouseRule.duration;
    }

    public override void UpdateRule(BaseRuleController baseRule, int elapsedTime)
    {
      base.UpdateRule(baseRule, elapsedTime);
      if (baseRule.state != RuleState.Active) return;

      var rule = (MouseRuleController)baseRule;
      var mouseRule = (MouseRule)rule.model;

      if (rule.curTime <= 0)
      {
        rule.state = RuleState.Inactive;
        rule.curCooldown = mouseRule.cooldown;
      }

      // Check rule type
      if (mouseRule.ruleType <= MouseRuleType.MiddleButton)
      {

        MouseButtonMode(rule, rule.state == RuleState.Inactive);

        if (mouseRule.mode == KeyPressMode.Press || mouseRule.mode == KeyPressMode.Double)
        {
          baseRule.state = RuleState.Inactive;
          rule.curCooldown = mouseRule.cooldown;
          return;
        }
      }
      else if (mouseRule.ruleType == MouseRuleType.Scroll)
      {
        simulator.Mouse.HorizontalScroll(mouseRule.X);
        simulator.Mouse.VerticalScroll(mouseRule.Y);
        baseRule.state = RuleState.Inactive;
        rule.curCooldown = mouseRule.cooldown;
        return;
      }
      else if (mouseRule.ruleType == MouseRuleType.MoveTo)
      {
        simulator.Mouse.MoveMouseToPositionOnVirtualDesktop(mouseRule.X, mouseRule.Y);
      }
      else
      {
        simulator.Mouse.MoveMouseBy(
            (int)Math.Ceiling((double)mouseRule.speedX / 1000 * elapsedTime),
            (int)Math.Ceiling((double)mouseRule.speedY / 1000 * elapsedTime)
        );
      }

      rule.curTime -= elapsedTime;
    }

    public void MouseButtonMode(MouseRuleController rule, bool keyUp = false)
    {
      var model = (MouseRule)rule.model;

      // Single click mode
      if (model.mode == KeyPressMode.Press)
      {
        switch (model.ruleType)
        {
          case MouseRuleType.LeftButton: simulator.Mouse.LeftButtonClick(); break;
          case MouseRuleType.RightButton: simulator.Mouse.RightButtonClick(); break;
          case MouseRuleType.MiddleButton: simulator.Mouse.MiddleButtonClick(); break;
        }

        return;
      }


      // Spam mode
      else if (model.mode == KeyPressMode.Spam)
      {
        if (rule.curSpamDelay <= 0)
        {
          switch (model.ruleType)
          {
            case MouseRuleType.LeftButton: simulator.Mouse.LeftButtonClick(); break;
            case MouseRuleType.RightButton: simulator.Mouse.RightButtonClick(); break;
            case MouseRuleType.MiddleButton: simulator.Mouse.MiddleButtonClick(); break;
          }

          rule.curSpamDelay = 4;
        }
        else
        {
          rule.curSpamDelay--;
        }

        return;
      }

      // Double click mode
      else if (model.mode == KeyPressMode.Double)
      {
        switch (model.ruleType)
        {
          case MouseRuleType.LeftButton: simulator.Mouse.LeftButtonDoubleClick(); break;
          case MouseRuleType.RightButton: simulator.Mouse.RightButtonDoubleClick(); break;
          case MouseRuleType.MiddleButton: simulator.Mouse.MiddleButtonDoubleClick(); break;
        }

        return;
      }

      // Hold button mode
      else if (model.mode == KeyPressMode.Hold)
      {
        switch (model.ruleType)
        {
          case MouseRuleType.LeftButton:
            if (keyUp) simulator.Mouse.LeftButtonUp();
            else simulator.Mouse.LeftButtonDown();
            break;

          case MouseRuleType.RightButton:
            if (keyUp) simulator.Mouse.RightButtonUp();
            else simulator.Mouse.RightButtonDown();
            break;

          case MouseRuleType.MiddleButton:
            if (keyUp) simulator.Mouse.MiddleButtonUp();
            else simulator.Mouse.MiddleButtonDown();
            break;
        }
      }


    }
  }
}
