using System.Collections.Generic; 
namespace TwitchGQL{ 

    public class CommunityPointsSettings
    {
        public string name { get; set; }
        public Image image { get; set; }
        public string __typename { get; set; }
        public List<AutomaticReward> automaticRewards { get; set; }
        public List<CustomReward> customRewards { get; set; }
        public List<object> goals { get; set; }
        public bool isEnabled { get; set; }
        public int raidPointAmount { get; set; }
        public List<EmoteVariant> emoteVariants { get; set; }
        public Earning earning { get; set; }
    }

}