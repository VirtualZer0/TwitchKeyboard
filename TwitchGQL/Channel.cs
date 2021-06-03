namespace TwitchGQL{ 

    public class Channel
    {
        public string id { get; set; }
        public Self self { get; set; }
        public string __typename { get; set; }
        public CommunityPointsSettings communityPointsSettings { get; set; }
    }

}