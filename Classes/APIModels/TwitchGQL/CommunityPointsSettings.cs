using System.Collections.Generic; 
namespace Classes.APIModels.TwitchGQL
{ 

    public class CommunityPointsSettings
    {
        public string name { get; set; }
        public Image image { get; set; }
        public string __typename { get; set; }
        public AutomaticReward[] automaticRewards { get; set; }
        public CustomReward[] customRewards { get; set; }
        public List<object> goals { get; set; }
        public bool isEnabled { get; set; }
        public int raidPointAmount { get; set; }
        public EmoteVariant[] emoteVariants { get; set; }
        public Earning earning { get; set; }
    }

}