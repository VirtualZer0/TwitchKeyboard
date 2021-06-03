using System.Collections.Generic; 
namespace TwitchGQL{ 

    public class EmoteVariant
    {
        public string id { get; set; }
        public bool isUnlockable { get; set; }
        public Emote emote { get; set; }
        public List<Modification> modifications { get; set; }
        public string __typename { get; set; }
    }

}