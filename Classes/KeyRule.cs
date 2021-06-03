using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace TwitchKeyboard.Classes
{
    public class KeyRule
    {
        /// <summary>
        /// Key code that will be pressed when the rule is called
        /// </summary>
        public VirtualKeyCode? key { get; set; }

        /// <summary>
        /// Duration of the key press. If 0, key will only be pressed once
        /// </summary>
        public int duration { get; set; } = 0;

        /// <summary>
        /// Cooldown between two uses of the current rule
        /// </summary>
        public int cooldown { get; set; } = 0;

        /// <summary>
        /// Rule trigger
        /// </summary>
        public TwitchEvent trigger { get; set; } = TwitchEvent.MESSAGE;

        /// <summary>
        /// Reward id, uses if trigger = REWARD
        /// </summary>
        public string rewardId { get; set; } = null;

        /// <summary>
        /// User message to activate the rule. If empty, works for any messages and rewards
        /// </summary>
        public string message { get; set; } = "";

        [JsonIgnore]
        public int currentCooldown = 0;

        [JsonIgnore]
        public int currentDuration = 0;

        [JsonIgnore]
        public bool active = false;
    }

    /// <summary>
    /// Available Twitch events
    /// </summary>
    public enum TwitchEvent
    {
        MESSAGE = 0,
        REWARD
    }
}
