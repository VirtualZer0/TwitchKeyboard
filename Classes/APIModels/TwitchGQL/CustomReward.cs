namespace Classes.APIModels.TwitchGQL
{ 

    public class CustomReward
    {
        public string id { get; set; }
        public string backgroundColor { get; set; }
        public object cooldownExpiresAt { get; set; }
        public int cost { get; set; }
        public DefaultImage defaultImage { get; set; }
        public Image image { get; set; }
        public MaxPerStreamSetting maxPerStreamSetting { get; set; }
        public MaxPerUserPerStreamSetting maxPerUserPerStreamSetting { get; set; }
        public GlobalCooldownSetting globalCooldownSetting { get; set; }
        public bool isEnabled { get; set; }
        public bool isInStock { get; set; }
        public bool isPaused { get; set; }
        public bool isSubOnly { get; set; }
        public bool isUserInputRequired { get; set; }
        public bool shouldRedemptionsSkipRequestQueue { get; set; }
        public object redemptionsRedeemedCurrentStream { get; set; }
        public string prompt { get; set; }
        public string title { get; set; }
        public string updatedForIndicatorAt { get; set; }
        public string __typename { get; set; }
    }

}