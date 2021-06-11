namespace Classes.APIModels.TwitchGQL
{ 

    public class MaxPerUserPerStreamSetting
    {
        public bool isEnabled { get; set; }
        public int maxPerUserPerStream { get; set; }
        public string __typename { get; set; }
    }

}