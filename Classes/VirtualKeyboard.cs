using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

namespace TwitchKeyboard.Classes
{
    public class VirtualKeyboard
    {
        public List<KeyRule> rules = new List<KeyRule>();
        public bool enabled = false;
        InputSimulator simulator = new();
        Timer t = new Timer();

        public delegate void OnRuleUsedHandler(object sender, string user, Key key);
        public event OnRuleUsedHandler OnRuleUsed;

        /// <summary>
        /// Create and launch keyboard timer
        /// </summary>
        public void init ()
        {
            this.t.Interval = 30;
            this.t.AutoReset = true;
            this.t.Elapsed += T_Elapsed;
            this.t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.update();
        }

        /// <summary>
        /// Calls on a new message in the chat
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="message">User message</param>
        public void newMessage(string user, string message)
        {
            if (!enabled) return;
            this.rules.ForEach(rule =>
            {
                if (
                    rule.trigger == TwitchEvent.MESSAGE
                    && (rule.message == "" || message.Trim().ToUpper() == rule.message.Trim().ToUpper())
                )
                {
                    this.useRule(rule);
                    this.OnRuleUsed?.Invoke(this, user, KeyInterop.KeyFromVirtualKey((int)rule.key));
                }
            });
        }

        /// <summary>
        /// Calls on a new reward in the chat
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="rewardId">Reward id</param>
        /// <param name="message">User message</param>
        public void newReward(string user, string rewardId, string message)
        {
            if (!enabled) return;
            this.rules.ForEach(rule =>
            {
                if (
                    rule.trigger == TwitchEvent.REWARD
                    && (rule.message == "" || message.Trim().ToUpper() == rule.message.Trim().ToUpper())
                    && rule.rewardId == rewardId
                )
                {
                    this.useRule(rule);
                    this.OnRuleUsed?.Invoke(this, user, KeyInterop.KeyFromVirtualKey((int)rule.key));
                }
            });
        }

        /// <summary>
        /// Use selected rule
        /// </summary>
        /// <param name="rule">Key rule</param>
        private void useRule (KeyRule rule)
        {
            if (rule.key == null) return;
            if (rule.currentCooldown > 0) return;

            if (rule.duration == 0)
            {
                this.simulator.Keyboard.KeyPress((VirtualKeyCode)rule.key);
            }
            else
            {
                rule.currentDuration = rule.duration;
                rule.active = true;
                this.simulator.Keyboard.KeyDown((VirtualKeyCode)rule.key);
            }

            if (rule.cooldown > 0)
            {
                rule.currentCooldown = rule.cooldown;
            }
        }

        /// <summary>
        /// Timer loop for durations and cooldowns
        /// </summary>
        private void update ()
        {
            this.rules.ForEach(rule =>
            {
                if (rule.currentCooldown > 0)
                {
                    rule.currentCooldown -= 30;
                }

                if (rule.active)
                {
                    if (rule.currentDuration > 0)
                    {
                        rule.currentDuration -= 30;
                        this.simulator.Keyboard.KeyDown((VirtualKeyCode)rule.key);
                    }
                    else
                    {
                        rule.active = false;
                        this.simulator.Keyboard.KeyUp((VirtualKeyCode)rule.key);
                        rule.currentCooldown = rule.cooldown;
                        rule.currentDuration = 0;
                    }
                }
            });
        }
    }
}
