using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Classes.RuleControllers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes.Managers
{
  public class BaseRuleManager
  {
    protected BaseRuleController[] rules = Array.Empty<BaseRuleController>();
    public bool enabled { get; protected set; } = false;

    public delegate void OnRuleActivateHandler(object sender, BaseRuleController rule, string user);
    public event OnRuleActivateHandler OnRuleActivate;


    /// <summary>
    /// Creates rule controllers for all rules
    /// </summary>
    /// <typeparam name="TController">Controller type</typeparam>
    /// <param name="rules">Array of rules</param>
    public void CreateControllers<TController>(BaseRule[] rules) where TController : BaseRuleController, new()
    {
      lock (rules)
      {
        this.rules = new TController[rules.Length];
        for (int i = 0; i < rules.Length; i++)
        {
          this.rules[i] = new TController() { model = rules[i] };
          this.rules[i].Bind();
          this.rules[i].Init();
        }
      }
    }

    /// <summary>
    /// Unloads controller resources if needed
    /// </summary>
    public void DestroyControllers()
    {
      for (int i = 0; i < rules.Length; i++)
      {
        rules[i].Destroy();
      }
    }

    /// <summary>
    /// Called when a Twitch client receives a new message
    /// </summary>
    /// <param name="user">Username</param>
    /// <param name="message">Message text</param>
    public void NewMessage(string user, string message)
    {
      if (!enabled) return;
      for (int i = 0; i < rules.Length; i++)
      {
        if (rules[i].curCooldown > 0) continue;
        for (int j = 0; j < rules[i].triggers.Length; j++)
        {
          if (rules[i].triggers[j].CheckMessage(user, message))
          {
            this.LaunchRule(rules[i], user);
          }
        }
      }
    }

    /// <summary>
    /// Called when a Twitch client receives a new reward
    /// </summary>
    /// <param name="user">Username</param>
    /// <param name="message">Message text in reward</param>
    /// <param name="rewardId">Reward unique id</param>
    public void NewReward(string user, string message, string rewardId)
    {
      if (!enabled) return;
      for (int i = 0; i < rules.Length; i++)
      {
        if (rules[i].curCooldown > 0) continue;
        for (int j = 0; j < rules[i].triggers.Length; j++)
        {
          if (rules[i].triggers[j].CheckReward(user, message, rewardId))
          {
            this.LaunchRule(rules[i], user);
          }
        }
      }
    }

    /// <summary>
    /// Called when a Twitch ckient receives a new bits donation
    /// </summary>
    /// <param name="user">Username</param>
    /// <param name="message">Message text</param>
    /// <param name="amount">Amount of bits</param>
    public void NewBits(string user, string message, int amount)
    {
      if (!enabled) return;
      for (int i = 0; i < rules.Length; i++)
      {
        if (rules[i].curCooldown > 0) continue;
        for (int j = 0; j < rules[i].triggers.Length; j++)
        {
          if (rules[i].triggers[j].CheckBits(user, message, amount))
          {
            this.LaunchRule(rules[i], user);
          }
        }
      }
    }

    /// <summary>
    /// Called when a channel gets new subscriber
    /// </summary>
    /// <param name="user">Username</param>
    public void NewSub(string user)
    {
      if (!enabled) return;
      for (int i = 0; i < rules.Length; i++)
      {
        if (rules[i].curCooldown > 0) continue;
        for (int j = 0; j < rules[i].triggers.Length; j++)
        {
          if (rules[i].triggers[j].CheckNewSub(user))
          {
            this.LaunchRule(rules[i], user);
          }
        }
      }
    }

    /// <summary>
    /// Called when a channel gets re-subscribe
    /// </summary>
    /// <param name="user">Username</param>
    public void NewReSub(string user, string message)
    {
      if (!enabled) return;
      for (int i = 0; i < rules.Length; i++)
      {
        if (rules[i].curCooldown > 0) continue;
        for (int j = 0; j < rules[i].triggers.Length; j++)
        {
          if (rules[i].triggers[j].CheckReSub(user, message))
          {
            this.LaunchRule(rules[i], user);
          }
        }
      }
    }

    /// <summary>
    /// Called when a channel gets gifted subscribe
    /// </summary>
    /// <param name="user">Gifter username</param>
    public void NewGiftSub(string user)
    {
      if (!enabled) return;
      for (int i = 0; i < rules.Length; i++)
      {
        if (rules[i].curCooldown > 0) continue;
        for (int j = 0; j < rules[i].triggers.Length; j++)
        {
          if (rules[i].triggers[j].CheckGiftSub(user))
          {
            this.LaunchRule(rules[i], user);
          }
        }
      }
    }

    /// <summary>
    /// Called when a channel gets raid
    /// </summary>
    /// <param name="user">Username</param>
    /// <param name="amount">Amount of users in raid</param>
    public void NewRaid(string user, int amount)
    {
      if (!enabled) return;
      for (int i = 0; i < rules.Length; i++)
      {
        if (rules[i].curCooldown > 0) continue;
        for (int j = 0; j < rules[i].triggers.Length; j++)
        {
          if (rules[i].triggers[j].CheckRaid(user, amount))
          {
            this.LaunchRule(rules[i], user);
          }
        }
      }
    }

    /// <summary>
    /// Trying to change rule state to Delay or Active
    /// </summary>
    /// <param name="rule">Rule for activation</param>
    /// <param name="user">Last user who fired the trigger</param>
    public virtual void LaunchRule(BaseRuleController rule, string user)
    {
      if (rule.model.delay > 0)
      {
        rule.state = RuleState.Delay;
        rule.curDelay = rule.model.delay;
      }
      else
      {
        rule.state = RuleState.Active;
        rule.curCooldown = rule.model.cooldown;
      }

      OnRuleActivate?.Invoke(this, rule, user);
    }

    /// <summary>
    /// Updates each rule and each trigger
    /// </summary>
    /// <param name="elapsedTime">Timer interval</param>
    public virtual void Update(int elapsedTime)
    {
      if (!enabled) return;

      // Pass through each available rule and each available trigger in this rule
      for (int i = 0; i < rules.Length; i++)
      {
        UpdateRule(rules[i], elapsedTime);
      }
    }

    /// <summary>
    /// Updates rule cooldown, delay and triggers
    /// </summary>
    /// <param name="rule">Current rule</param>
    /// <param name="elapsedTime">Timer interval</param>
    public virtual void UpdateRule(BaseRuleController rule, int elapsedTime)
    {
      if (rule.curCooldown > 0 && rule.state == RuleState.Inactive)
      {
        rule.curCooldown -= elapsedTime;
        return;
      }

      for (int i = 0; i < rule.triggers.Length; i++)
      {
        UpdateTrigger(rule.triggers[i], elapsedTime);
      }

      if (rule.curDelay > 0 && rule.state == RuleState.Delay) rule.curDelay -= elapsedTime;
      if (rule.curDelay <= 0 && rule.state == RuleState.Delay) rule.state = RuleState.Active;

    }

    /// <summary>
    /// Updates trigger repeat time
    /// </summary>
    /// <param name="trigger">Current trigger</param>
    /// <param name="elapsedTime">Timer interval</param>
    public virtual void UpdateTrigger(TwitchTriggerController trigger, int elapsedTime)
    {
      if (!trigger.repeatTimerActive) return;
      trigger.repeatTime -= elapsedTime;
      if (trigger.repeatTime <= 0) trigger.Reset();
    }

    public void Disable()
    {
      enabled = false;

      lock (rules)
      {
        // Pass through each available rule and each available trigger in this rule
        for (int i = 0; i < rules.Length; i++)
        {
          DisableRule(rules[i]);
        }
      }
    }

    public void Enable()
    {
      enabled = true;
    }

    public virtual void DisableRule(BaseRuleController rule)
    {
      rule.state = RuleState.Inactive;
      rule.curCooldown = 0;
      rule.curDelay = rule.model.delay;

      for (int i = 0; i < rule.triggers.Length; i++)
      {
        rule.triggers[i].Reset();
      }
    }
  }
}
