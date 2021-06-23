using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchKeyboard.Enums
{
    /// <summary>
    /// Available Twitch events
    /// </summary>
    public enum TwitchEvent
    {
        MESSAGE = 0,
        REWARD,
        BITS,
        RESUB,
        NEWSUB,
        GIFTSUB,
        RAID
    }
}
