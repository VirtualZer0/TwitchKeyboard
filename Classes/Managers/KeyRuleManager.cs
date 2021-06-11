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
using WindowsInput.Native;

namespace TwitchKeyboard.Classes.Managers
{
    public class KeyRuleManager : BaseRuleManager
    {
        readonly InputSimulator simulator = new();

        public void CreateControllers (List<KeyRule> rules)
        {
            CreateControllers<KeyRuleController>(rules.ToArray());
        }

        public override void LaunchRule(BaseRuleController baseRule, string user)
        {
            base.LaunchRule(baseRule, user);
            if (baseRule.state == RuleState.Inactive) return;


            var rule = (KeyRuleController)baseRule;
            var keyRule = (KeyRule)rule.model;
            rule.curTime = keyRule.duration;
        }

        public override void UpdateRule(BaseRuleController baseRule, int elapsedTime)
        {
            base.UpdateRule(baseRule, elapsedTime);
            if (baseRule.state != RuleState.Active) return;

            var rule = (KeyRuleController)baseRule;
            KeyRule keyRule = (KeyRule)rule.model;

            // Check rule type
            if (keyRule.mode == KeyPressMode.Press)
            {
                rule.state = RuleState.Inactive;
                for (int i = 0; i < keyRule.keys.Count; i++)
                {
                    simulator.Keyboard.KeyPress(keyRule.keys[i]);
                }
            }
            else
            {
                // Decrease current duration time
                if (rule.curTime > 0) rule.curTime -= elapsedTime;

                int keyCount = (rule.model as KeyRule).keys.Count;

                // Check if rule time is over
                if (rule.curTime <= 0)
                {
                    rule.state = RuleState.Inactive;
                    rule.curCooldown = keyRule.cooldown;
                    if (keyRule.mode == KeyPressMode.Hold)
                    {
                        for (int i = 0; i < keyCount; i++)
                        {
                            simulator.Keyboard.KeyUp(keyRule.keys[i]);
                        }
                    }

                    return;
                }

                // If not over - execute rule mechanic
                for (int i = 0; i < keyCount; i++)
                {
                    if (keyRule.mode == KeyPressMode.Hold)
                    {
                        simulator.Keyboard.KeyDown(keyRule.keys[i]);
                    }
                    else
                    {
                        simulator.Keyboard.KeyPress(keyRule.keys[i]);
                    }
                }
            }
        }
    }
}
