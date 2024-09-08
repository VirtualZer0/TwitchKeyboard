using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchKeyboard.Classes.RuleControllers
{
  /// <summary>
  /// Controls the operation of a specific trigger, based on its model
  /// </summary>
  public class TwitchTriggerController
  {
    public TwitchTrigger trigger { get; private set; }

    public bool repeatTimerActive;
    public int repeats;
    public int repeatTime;
    public List<string> users;


    public TwitchTriggerController(TwitchTrigger trigger)
    {
      this.trigger = trigger;
      Reset();
    }

    public void Reset()
    {
      if (trigger.repeated)
      {
        repeats = 0;
        repeatTime = trigger.repeatDuration;
        repeatTimerActive = false;

        if (trigger.repeatUniqueUsers)
        {
          users = new();
        }
      }
    }

    public bool CheckMessage(string user, string message)
    {
      if (trigger.type != Enums.TwitchEvent.MESSAGE) return false;
      if (!CheckText(message)) return false;
      if (!CheckRepeat(user)) return false;

      return true;
    }

    public bool CheckReward(string user, string message, string rewardId)
    {
      if (trigger.type != Enums.TwitchEvent.REWARD) return false;
      if (trigger.rewardId != rewardId) return false;
      if (!CheckText(message)) return false;
      if (!CheckRepeat(user)) return false;

      return true;
    }

    public bool CheckBits(string user, string message, int amount)
    {
      if (trigger.type != Enums.TwitchEvent.BITS) return false;
      if (!(amount >= trigger.amountFrom && amount <= trigger.amountTo)) return false;
      if (!CheckText(message)) return false;
      if (!CheckRepeat(user)) return false;

      return true;
    }

    public bool CheckNewSub(string user)
    {
      if (trigger.type != Enums.TwitchEvent.NEWSUB) return false;
      if (!CheckRepeat(user)) return false;

      return true;
    }

    public bool CheckReSub(string user, string message)
    {
      if (trigger.type != Enums.TwitchEvent.RESUB) return false;
      if (!CheckText(message)) return false;
      if (!CheckRepeat(user)) return false;

      return true;
    }

    public bool CheckGiftSub(string user)
    {
      if (trigger.type != Enums.TwitchEvent.GIFTSUB) return false;
      if (!CheckRepeat(user)) return false;

      return true;
    }

    public bool CheckRaid(string user, int amount)
    {
      if (trigger.type != Enums.TwitchEvent.RAID) return false;
      if (!(amount >= trigger.amountFrom && amount <= trigger.amountTo)) return false;
      if (!CheckRepeat(user)) return false;

      return true;
    }

    private bool CheckText(string message)
    {
      if (trigger.text == "")
      {
        return true;
      }
      else
      {
        if (trigger.comparisonMode == Enums.TextCompareMode.EQUALS)
        {
          return trigger.caseSensitive ? trigger.text.Equals(message) : trigger.text.ToUpper().Equals(message.ToUpper());
        }
        else
        {
          return trigger.caseSensitive ? message.Contains(trigger.text) : message.ToUpper().Contains(trigger.text.ToUpper());
        }
      }
    }

    private bool CheckUnique(string user)
    {
      if (!trigger.repeatUniqueUsers) return true;

      if (users.Contains(user))
      {
        return false;
      }

      users.Add(user);
      return true;
    }

    private bool CheckRepeat(string user)
    {
      if (!trigger.repeated) return true;

      if (!CheckUnique(user)) return false;

      if (repeats == 0 || trigger.repeatResetTime)
      {
        repeatTime = trigger.repeatDuration;
        repeatTimerActive = true;
      }

      repeats++;

      if (repeats >= trigger.repeatTimes)
      {
        Reset();
        return true;
      }

      return false;
    }
  }
}
