namespace Classes.APIModels.TwitchGQL
{ 

    public class Community
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public Channel channel { get; set; }
        public string __typename { get; set; }
        public object self { get; set; }
    }

}