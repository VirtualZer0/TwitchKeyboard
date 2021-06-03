namespace TwitchGQL{ 

    public class GlobalCooldownSetting
    {
        public bool isEnabled { get; set; }
        public int globalCooldownSeconds { get; set; }
        public string __typename { get; set; }
    }

}