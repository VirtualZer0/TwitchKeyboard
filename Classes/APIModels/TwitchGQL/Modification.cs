using System; 
namespace Classes.APIModels.TwitchGQL
{ 

    public class Modification
    {
        public string id { get; set; }
        public Emote emote { get; set; }
        public Modifier modifier { get; set; }
        public DateTime globallyUpdatedForIndicatorAt { get; set; }
        public string __typename { get; set; }
    }

}